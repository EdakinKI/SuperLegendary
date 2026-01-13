using ClassLibrary1;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1
{
    public partial class UpdateCoefForm : Form
    {
        private DatabaseService _dbService = new DatabaseService();
        private DataTable _excelData;
        private List<EnergySystem> _allSystems = new List<EnergySystem>();
        private DataGridView dgvSystems;
        private string _sheetType = "unknown";

        // Ключевые слова для определения типа листа
        private string[] EnergyKeywords => new string[] {
            "энерг", "электро", "ээ", "energy",
            "э/э", "э-э", "э.э", "e",
            "E", "Э/Э", "Э-Э", "Э.Э", "ЭЭ",
            "Э/э", "Э-э", "Э.э", "Ээ"
        };

        private string[] PowerKeywords => new string[] {
            "м", "мощность", "power", "мощ", "pwr", "p",
            "М", "Мощность", "Power", "Мощ", "Pwr", "P",
            "МОЩ", "PWR"
        };

        public UpdateCoefForm()
        {
            InitializeComponent();
            ConfigureForm();
            InitializePlaceholderText();
        }

        private void ConfigureForm()
        {
            this.Text = "Обновление коэффициентов влияния";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(800, 600);

            // Скрываем выбор системы и DataGridView из дизайнера
            cmbSystems.Visible = false;
            lblSystem.Visible = false;
            dgvRanges.Visible = false;

            // Создаем новый DataGridView для отображения систем
            dgvSystems = new DataGridView
            {
                Location = new Point(12, 170),
                Size = new Size(760, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            this.Controls.Add(dgvSystems);

            // Добавляем информационную метку
            var lblInfo = new Label
            {
                Location = new Point(12, 430),
                Size = new Size(760, 40),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular),
                Text = "Все найденные энергосистемы будут сохранены одновременно."
            };
            this.Controls.Add(lblInfo);

            // Перемещаем кнопки вниз
            btnSave.Location = new Point(584, 480);
            btnCancel.Location = new Point(680, 480);
        }

        private void InitializePlaceholderText()
        {
            txtSetName.Text = "Например: Коэффициенты 2024";
            txtSetName.ForeColor = Color.Gray;
            txtSetName.GotFocus += TxtSetName_GotFocus;
            txtSetName.LostFocus += TxtSetName_LostFocus;
        }

        private void TxtSetName_GotFocus(object sender, EventArgs e)
        {
            if (txtSetName.Text == "Например: Коэффициенты 2024")
            {
                txtSetName.Text = "";
                txtSetName.ForeColor = Color.Black;
            }
        }

        private void TxtSetName_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSetName.Text))
            {
                txtSetName.Text = "Например: Коэффициенты 2024";
                txtSetName.ForeColor = Color.Gray;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                    LoadExcelData(openFileDialog.FileName);
                }
            }
        }

        private void LoadExcelData(string filePath)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = false
                        },
                        UseColumnDataType = true
                    });

                    if (result.Tables.Count > 0)
                    {
                        // Пробуем определить тип по имени файла
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        _sheetType = DetectSheetType(fileName, result.Tables[0].TableName);

                        _excelData = result.Tables[0];
                        ParseExcelData();
                        DisplaySystems();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string DetectSheetType(string fileName, string sheetName)
        {
            string textToCheck = $"{fileName} {sheetName}".ToLower();

            // Проверяем на электроэнергию
            foreach (var keyword in EnergyKeywords)
            {
                if (textToCheck.Contains(keyword.ToLower()))
                {
                    lblSheetType.Text = "Тип листа: Коэффициенты для ЭЛЕКТРОЭНЕРГИИ";
                    return "energy";
                }
            }

            // Проверяем на мощность
            foreach (var keyword in PowerKeywords)
            {
                if (textToCheck.Contains(keyword.ToLower()))
                {
                    lblSheetType.Text = "Тип листа: Коэффициенты для МОЩНОСТИ";
                    return "power";
                }
            }

            // Если не нашли, пробуем определить по содержимому
            lblSheetType.Text = "Тип листа: Не определен (определите вручную)";
            return ShowTypeSelectionDialog();
        }

        private string ShowTypeSelectionDialog()
        {
            var dialog = new Form
            {
                Text = "Выберите тип коэффициентов",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblMessage = new Label
            {
                Text = "Не удалось определить тип коэффициентов.\nПожалуйста, выберите вручную:",
                Location = new Point(20, 20),
                Size = new Size(350, 40)
            };

            var rbEnergy = new RadioButton
            {
                Text = "Коэффициенты для электроэнергии",
                Location = new Point(20, 70),
                Size = new Size(350, 25),
                Checked = true
            };

            var rbPower = new RadioButton
            {
                Text = "Коэффициенты для мощности",
                Location = new Point(20, 100),
                Size = new Size(350, 25)
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(150, 140),
                Size = new Size(100, 30)
            };

            btnOk.Click += (s, e) => dialog.DialogResult = DialogResult.OK;

            dialog.Controls.AddRange(new Control[] { lblMessage, rbEnergy, rbPower, btnOk });

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                if (rbEnergy.Checked)
                {
                    lblSheetType.Text = "Тип листа: Коэффициенты для ЭЛЕКТРОЭНЕРГИИ (выбрано вручную)";
                    return "energy";
                }
                else
                {
                    lblSheetType.Text = "Тип листа: Коэффициенты для МОЩНОСТИ (выбрано вручную)";
                    return "power";
                }
            }

            return "unknown";
        }

        private void ParseExcelData()
        {
            _allSystems.Clear();

            // Парсим все системы из файла
            Dictionary<string, List<DataRow>> systemRows = new Dictionary<string, List<DataRow>>();

            for (int i = 0; i < _excelData.Rows.Count; i++)
            {
                var row = _excelData.Rows[i];
                if (row.ItemArray.Length > 4 && row[4] != null && !string.IsNullOrEmpty(row[4].ToString()))
                {
                    string systemName = row[4].ToString().Trim();
                    if (!systemRows.ContainsKey(systemName))
                        systemRows[systemName] = new List<DataRow>();
                    systemRows[systemName].Add(row);
                }
            }

            foreach (var kvp in systemRows)
            {
                var systemName = kvp.Key;
                var rows = kvp.Value;

                var system = new EnergySystem { Name = systemName };

                // Парсим диапазоны для каждой системы
                for (int i = 0; i < rows.Count; i += 3)
                {
                    if (i + 2 < rows.Count)
                    {
                        var rangeRows = rows.Skip(i).Take(3).ToList();
                        ParseRangeData(system, rangeRows);
                    }
                }

                if (system.Ranges.Count > 0)
                {
                    _allSystems.Add(system);
                }
            }

            btnSave.Enabled = _allSystems.Count > 0 && _sheetType != "unknown";
        }

        private void ParseRangeData(EnergySystem system, List<DataRow> rows)
        {
            if (rows.Count != 3) return;

            int startColumn = 6;
            int maxColumns = 0;

            for (int i = 0; i < 3; i++)
            {
                maxColumns = Math.Max(maxColumns, rows[i].ItemArray.Length);
            }

            for (int col = startColumn; col < maxColumns; col++)
            {
                try
                {
                    string fromStr = rows[0][col]?.ToString()?.Trim();
                    string toStr = rows[1][col]?.ToString()?.Trim();
                    string coeffStr = rows[2][col]?.ToString()?.Trim();

                    if (!string.IsNullOrEmpty(fromStr) && !string.IsNullOrEmpty(toStr) && !string.IsNullOrEmpty(coeffStr))
                    {
                        fromStr = fromStr.Replace(',', '.');
                        toStr = toStr.Replace(',', '.');
                        coeffStr = coeffStr.Replace(',', '.');

                        if (double.TryParse(fromStr, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double from) &&
                            double.TryParse(toStr, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double to) &&
                            double.TryParse(coeffStr, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double coefficient))
                        {
                            var range = new TemperatureRange
                            {
                                From = from,
                                To = to,
                                Coefficient = coefficient
                            };
                            system.Ranges.Add(range);
                        }
                    }
                }
                catch
                {
                    // Игнорируем ошибки парсинга
                }
            }
        }

        private void DisplaySystems()
        {
            dgvSystems.Rows.Clear();
            dgvSystems.Columns.Clear();

            dgvSystems.Columns.Add("SystemName", "Энергосистема");
            dgvSystems.Columns.Add("RangeCount", "Кол-во диапазонов");
            dgvSystems.Columns.Add("Ranges", "Диапазоны температур");

            dgvSystems.Columns["SystemName"].Width = 200;
            dgvSystems.Columns["RangeCount"].Width = 120;
            dgvSystems.Columns["Ranges"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            foreach (var system in _allSystems)
            {
                string rangesText = string.Join("; ", system.Ranges
                    .OrderBy(r => r.From)
                    .Select(r => $"{r.From:F0}...{r.To:F0}°C"));

                dgvSystems.Rows.Add(
                    system.Name,
                    system.Ranges.Count,
                    rangesText
                );
            }

            // Добавляем информационный текст в заголовок
            lblTitle.Text = $"Обновление коэффициентов влияния ({_allSystems.Count} систем)";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string setName = txtSetName.Text;

            // Проверяем placeholder текст
            if (setName == "Например: Коэффициенты 2024" || string.IsNullOrWhiteSpace(setName))
            {
                MessageBox.Show("Введите название набора коэффициентов", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_allSystems.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_sheetType == "unknown")
            {
                MessageBox.Show("Не определен тип коэффициентов. Пожалуйста, выберите тип вручную.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                DateTime loadDate = DateTime.Now;
                int savedSystems = 0;
                int totalSystems = _allSystems.Count;

                // Получаем ID типа таблицы из БД
                string tableTypeName = _sheetType == "power"
                    ? "Коэффициенты влияния по мощности"
                    : "Коэффициенты влияния по электроэнергии";

                int tableTypeId = _dbService.GetOrCreateTableType(tableTypeName);

                // Сохраняем каждую энергосистему
                foreach (var system in _allSystems)
                {
                    try
                    {
                        // Получаем или создаем энергосистему
                        int systemId = _dbService.GetOrCreateEnergySystem(system.Name);

                        // Обновляем дату окончания предыдущего набора для этой системы
                        _dbService.UpdatePreviousParamSetEndDate(systemId, loadDate);

                        // Создаем новый набор параметров
                        string fullSetName = $"{setName} ({(_sheetType == "power" ? "мощность" : "энергия")})";

                        // Используем правильный метод с table_type_id
                        int paramSetId = _dbService.CreateForecastParamSet(
                            fullSetName,
                            systemId,
                            tableTypeId,
                            loadDate);

                        // Сохраняем температурные диапазоны
                        var dbRanges = new List<TemperatureRangeDb>();
                        int rangeNumber = 1;

                        foreach (var range in system.Ranges.OrderBy(r => r.From))
                        {
                            dbRanges.Add(new TemperatureRangeDb
                            {
                                RangeNumber = rangeNumber++,
                                TempLower = (int?)Math.Round(range.From),
                                TempUpper = (int?)Math.Round(range.To),
                                Coefficient = range.Coefficient
                            });
                        }

                        _dbService.SaveTemperatureRanges(paramSetId, dbRanges);
                        savedSystems++;

                        System.Diagnostics.Debug.WriteLine($"Сохранена система: {system.Name}, диапазонов: {dbRanges.Count}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении системы '{system.Name}':\n{ex.Message}",
                            "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                MessageBox.Show($"Коэффициенты успешно сохранены!\n\n" +
                               $"Тип: {(_sheetType == "power" ? "Мощность" : "Электроэнергия")}\n" +
                               $"Набор: {setName}\n" +
                               $"Сохранено систем: {savedSystems} из {totalSystems}\n" +
                               $"Дата загрузки: {loadDate:dd.MM.yyyy HH:mm}",
                               "Успешно",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка при сохранении в базу данных:\n\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}