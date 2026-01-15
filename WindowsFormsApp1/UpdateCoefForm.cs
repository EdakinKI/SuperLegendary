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
        private DataSet _excelDataSet;
        private Dictionary<string, List<EnergySystem>> _allSystemsBySheet = new Dictionary<string, List<EnergySystem>>();
        private ComboBox cmbSheetSelector;
        private DataGridView dgvSystems;
        private Label lblSheetSelector;
        private string _selectedSheetType = "unknown";

        // Ключевые слова для определения типа листа
        private string[] EnergyKeywords => new string[] {
            "энерг", "электро", "ээ", "energy",
            "э/э", "э-э", "э.э", "e",
            "E", "Э/Э", "Э-Э", "Э.Э", "ЭЭ",
            "Э/э", "Э-э", "Э.э", "Ээ"
        };

        private string[] PowerKeywords => new string[] {
            "м", "мощ", "power", "мощ", "pwr", "p",
            "М", "Мощ", "Power", "Мощ", "Pwr", "P",
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
            this.Size = new Size(900, 700);

            // Скрываем выбор системы и DataGridView из дизайнера
            cmbSystems.Visible = false;
            lblSystem.Visible = false;
            dgvRanges.Visible = false;

            // Добавляем Label для выбора листа
            lblSheetSelector = new Label
            {
                Name = "lblSheetSelector",
                Location = new Point(12, 105),
                Size = new Size(120, 20),
                Text = "Выберите лист:",
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular),
                Visible = false // Изначально скрыт
            };
            this.Controls.Add(lblSheetSelector);

            // Добавляем ComboBox для выбора листа
            cmbSheetSelector = new ComboBox
            {
                Name = "cmbSheetSelector",
                Location = new Point(140, 102),
                Size = new Size(300, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false // Изначально скрыт
            };
            cmbSheetSelector.SelectedIndexChanged += CmbSheetSelector_SelectedIndexChanged;
            this.Controls.Add(cmbSheetSelector);

            // Создаем новый DataGridView для отображения систем
            dgvSystems = new DataGridView
            {
                Location = new Point(12, 170),
                Size = new Size(860, 350),
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
                Location = new Point(12, 530),
                Size = new Size(860, 40),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular),
                Text = "Все найденные энергосистемы будут сохранены одновременно."
            };
            this.Controls.Add(lblInfo);

            // Перемещаем кнопки вниз
            btnSave.Location = new Point(684, 580);
            btnCancel.Location = new Point(780, 580);
        }

        private void CmbSheetSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSheetSelector.SelectedItem != null)
            {
                string selectedSheet = cmbSheetSelector.SelectedItem.ToString();
                if (_allSystemsBySheet.ContainsKey(selectedSheet))
                {
                    DisplaySystemsForSheet(selectedSheet);
                }
                else
                {
                    // Если данные еще не загружены, загружаем их
                    DataTable sheetData = _excelDataSet.Tables[selectedSheet];
                    if (sheetData != null)
                    {
                        // Определяем тип листа
                        string sheetType = DetectSheetType(Path.GetFileNameWithoutExtension(txtFilePath.Text), selectedSheet);
                        _selectedSheetType = sheetType;
                        ParseExcelData(sheetData, sheetType);
                        DisplaySystemsForSheet(selectedSheet);
                    }
                }
            }
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
                    _excelDataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = false
                        },
                        UseColumnDataType = true
                    });

                    if (_excelDataSet.Tables.Count > 0)
                    {
                        // Заполняем список листов
                        cmbSheetSelector.Items.Clear();
                        _allSystemsBySheet.Clear();

                        // Пробуем автоматически определить типы листов
                        foreach (DataTable sheet in _excelDataSet.Tables)
                        {
                            string sheetName = sheet.TableName;
                            cmbSheetSelector.Items.Add(sheetName);
                        }

                        // Если есть несколько листов, показываем выпадающий список
                        if (_excelDataSet.Tables.Count > 1)
                        {
                            cmbSheetSelector.Visible = true;
                            lblSheetSelector.Visible = true;

                            // Устанавливаем текст для всех найденных листов
                            lblSheetType.Text = $"Найдено листов: {_excelDataSet.Tables.Count}";

                            // Загружаем данные для первого листа по умолчанию
                            if (cmbSheetSelector.Items.Count > 0)
                            {
                                cmbSheetSelector.SelectedIndex = 0;
                            }
                        }
                        else if (_excelDataSet.Tables.Count == 1)
                        {
                            // Если только один лист, загружаем его сразу
                            cmbSheetSelector.Visible = false;
                            lblSheetSelector.Visible = false;

                            string sheetName = _excelDataSet.Tables[0].TableName;
                            string sheetType = DetectSheetType(Path.GetFileNameWithoutExtension(filePath), sheetName);
                            _selectedSheetType = sheetType;
                            lblSheetType.Text = $"Тип листа: {GetSheetTypeDisplayName(sheetType)}";

                            ParseExcelData(_excelDataSet.Tables[0], sheetType);
                            DisplaySystemsForSheet(sheetName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSheetTypeDisplayName(string sheetType)
        {
            switch (sheetType)
            {
                case "energy":
                    return "Коэффициенты для Электроэнергии";
                case "power":
                    return "Коэффициенты для Мощности";
                default:
                    return "Не определен";
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
                    return "energy";
                }
            }

            // Проверяем на мощность
            foreach (var keyword in PowerKeywords)
            {
                if (textToCheck.Contains(keyword.ToLower()))
                {
                    return "power";
                }
            }

            // Если не нашли, предлагаем выбрать вручную
            return ShowTypeSelectionDialogForSheet(sheetName);
        }

        private string ShowTypeSelectionDialogForSheet(string sheetName)
        {
            var dialog = new Form
            {
                Text = $"Выберите тип коэффициентов для листа '{sheetName}'",
                Size = new Size(500, 220),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblMessage = new Label
            {
                Text = $"Для листа '{sheetName}' не удалось определить тип коэффициентов.\nПожалуйста, выберите вручную:",
                Location = new Point(20, 20),
                Size = new Size(450, 40)
            };

            var rbEnergy = new RadioButton
            {
                Text = "Коэффициенты для электроэнергии",
                Location = new Point(20, 70),
                Size = new Size(450, 25),
                Checked = true
            };

            var rbPower = new RadioButton
            {
                Text = "Коэффициенты для мощности",
                Location = new Point(20, 100),
                Size = new Size(450, 25)
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(200, 165),
                Size = new Size(100, 30)
            };

            btnOk.Click += (s, e) => dialog.DialogResult = DialogResult.OK;

            dialog.Controls.AddRange(new Control[] { lblMessage, rbEnergy, rbPower, btnOk });

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                if (rbEnergy.Checked)
                {
                    return "energy";
                }
                else
                {
                    return "power";
                }
            }

            return "unknown";
        }

        private void ParseExcelData(DataTable excelData, string sheetType)
        {
            string sheetName = excelData.TableName;

            if (!_allSystemsBySheet.ContainsKey(sheetName))
            {
                _allSystemsBySheet[sheetName] = new List<EnergySystem>();
            }

            // Парсим все системы из файла
            Dictionary<string, List<DataRow>> systemRows = new Dictionary<string, List<DataRow>>();

            for (int i = 0; i < excelData.Rows.Count; i++)
            {
                var row = excelData.Rows[i];
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
                    _allSystemsBySheet[sheetName].Add(system);
                }
            }
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

        private void DisplaySystemsForSheet(string sheetName)
        {
            if (!_allSystemsBySheet.ContainsKey(sheetName) || _allSystemsBySheet[sheetName].Count == 0)
            {
                // Если данные еще не загружены, загружаем их
                DataTable sheetData = _excelDataSet.Tables[sheetName];
                if (sheetData != null)
                {
                    // Определяем тип листа
                    string sheetType = DetectSheetType(Path.GetFileNameWithoutExtension(txtFilePath.Text), sheetName);
                    _selectedSheetType = sheetType;
                    ParseExcelData(sheetData, sheetType);
                }
            }

            var systems = _allSystemsBySheet[sheetName];
            DisplaySystems(systems, sheetName);
        }

        private void DisplaySystems(List<EnergySystem> systems, string sheetName)
        {
            dgvSystems.Rows.Clear();
            dgvSystems.Columns.Clear();

            dgvSystems.Columns.Add("SystemName", "Энергосистема");
            dgvSystems.Columns.Add("RangeCount", "Кол-во диапазонов");
            dgvSystems.Columns.Add("Ranges", "Диапазоны температур");
            dgvSystems.Columns.Add("SheetType", "Тип листа");

            dgvSystems.Columns["SystemName"].Width = 200;
            dgvSystems.Columns["RangeCount"].Width = 120;
            dgvSystems.Columns["SheetType"].Width = 150;
            dgvSystems.Columns["Ranges"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            string typeDisplay = GetSheetTypeDisplayName(_selectedSheetType);

            foreach (var system in systems)
            {
                string rangesText = string.Join("; ", system.Ranges
                    .OrderBy(r => r.From)
                    .Select(r => $"{r.From:F0}...{r.To:F0}°C"));

                dgvSystems.Rows.Add(
                    system.Name,
                    system.Ranges.Count,
                    rangesText,
                    typeDisplay
                );
            }

            // Обновляем заголовок
            lblTitle.Text = $"Обновление коэффициентов влияния - {sheetName} ({systems.Count} систем)";
            lblSheetType.Text = $"Тип листа: {typeDisplay}";

            // Активируем кнопку сохранения если есть данные
            btnSave.Enabled = systems.Count > 0 && _selectedSheetType != "unknown";
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

            if (_excelDataSet == null || _excelDataSet.Tables.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                DateTime loadDate = DateTime.Now;
                int totalSavedSystems = 0;
                int totalSheetsProcessed = 0;

                // Определяем, какой лист сохранять
                string selectedSheetName = "";
                if (cmbSheetSelector.Visible && cmbSheetSelector.SelectedItem != null)
                {
                    selectedSheetName = cmbSheetSelector.SelectedItem.ToString();
                }
                else if (_excelDataSet.Tables.Count == 1)
                {
                    selectedSheetName = _excelDataSet.Tables[0].TableName;
                }

                if (string.IsNullOrEmpty(selectedSheetName))
                {
                    MessageBox.Show("Не выбран лист для сохранения", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Получаем данные для выбранного листа
                if (_allSystemsBySheet.ContainsKey(selectedSheetName))
                {
                    var systems = _allSystemsBySheet[selectedSheetName];
                    string sheetType = _selectedSheetType;

                    if (systems.Count == 0)
                    {
                        MessageBox.Show($"На листе '{selectedSheetName}' нет данных для сохранения", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Определяем тип таблицы
                    string tableTypeName = sheetType == "power"
                        ? "Коэффициенты влияния по мощности"
                        : "Коэффициенты влияния по электроэнергии";

                    int tableTypeId = _dbService.GetOrCreateTableType(tableTypeName);

                    // Сохраняем каждую энергосистему
                    foreach (var system in systems)
                    {
                        try
                        {
                            // Получаем или создаем энергосистему
                            int systemId = _dbService.GetOrCreateEnergySystem(system.Name);

                            // Обновляем дату окончания предыдущего набора для этой системы
                            _dbService.UpdatePreviousParamSetEndDate(systemId, loadDate);

                            // Создаем новый набор параметров
                            string fullSetName = $"{setName} ({GetSheetTypeDisplayName(sheetType)})";

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
                            totalSavedSystems++;

                            System.Diagnostics.Debug.WriteLine($"Сохранена система: {system.Name} из листа {selectedSheetName}, диапазонов: {dbRanges.Count}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении системы '{system.Name}' из листа '{selectedSheetName}':\n{ex.Message}",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    if (totalSavedSystems > 0)
                    {
                        totalSheetsProcessed++;
                    }
                }

                if (totalSavedSystems > 0)
                {
                    MessageBox.Show($"Коэффициенты успешно сохранены!\n\n" +
                                   $"Лист: {selectedSheetName}\n" +
                                   $"Тип: {GetSheetTypeDisplayName(_selectedSheetType)}\n" +
                                   $"Сохранено систем: {totalSavedSystems}\n" +
                                   $"Название набора: {setName}\n" +
                                   $"Дата загрузки: {loadDate:dd.MM.yyyy HH:mm}",
                                   "Успешно",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Information);

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить данные из выбранного листа",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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