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
        private List<SheetInfo> _sheetInfos = new List<SheetInfo>();
        private DataGridView dgvEnergySystems;
        private DataGridView dgvPowerSystems;
        private Label lblEnergyInfo;
        private Label lblPowerInfo;
        private Panel pnlEnergy;
        private Panel pnlPower;

        // Ключевые слова для определения типа листа
        private string[] EnergyKeywords => new string[] {
            "энерг", "электро", "ээ", "energy",
            "э/э", "э-э", "э.э", "e",
            "E", "Э/Э", "Э-Э", "Э.Э", "ЭЭ",
            "Э/э", "Э-э", "Э.э", "Ээ", "потребление"
        };

        private string[] PowerKeywords => new string[] {
            "м", "мощность", "power", "мощ", "pwr", "p",
            "М", "Мощность", "Power", "Мощ", "Pwr", "P",
            "МОЩ", "PWR", "нагрузка", "Нагрузка"
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

            // Скрываем ненужные элементы
            cmbSystems.Visible = false;
            lblSystem.Visible = false;
            dgvRanges.Visible = false;

            // Панель для коэффициентов электроэнергии
            pnlEnergy = new Panel
            {
                Location = new Point(12, 170),
                Size = new Size(860, 150),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            lblEnergyInfo = new Label
            {
                Location = new Point(5, 5),
                Size = new Size(850, 20),
                Text = "Коэффициенты для электроэнергии:",
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            dgvEnergySystems = new DataGridView
            {
                Location = new Point(5, 30),
                Size = new Size(850, 115),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            pnlEnergy.Controls.Add(lblEnergyInfo);
            pnlEnergy.Controls.Add(dgvEnergySystems);

            // Панель для коэффициентов мощности
            pnlPower = new Panel
            {
                Location = new Point(12, 330),
                Size = new Size(860, 150),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            lblPowerInfo = new Label
            {
                Location = new Point(5, 5),
                Size = new Size(850, 20),
                Text = "Коэффициенты для мощности:",
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            dgvPowerSystems = new DataGridView
            {
                Location = new Point(5, 30),
                Size = new Size(850, 115),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            pnlPower.Controls.Add(lblPowerInfo);
            pnlPower.Controls.Add(dgvPowerSystems);

            // Информационная метка
            var lblInfo = new Label
            {
                Location = new Point(12, 490),
                Size = new Size(860, 40),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular),
                Text = "Все найденные системы будут сохранены. Проверьте данные перед сохранением."
            };

            // Перемещаем кнопки вниз
            btnSave.Location = new Point(684, 540);
            btnCancel.Location = new Point(784, 540);

            this.Controls.Add(pnlEnergy);
            this.Controls.Add(pnlPower);
            this.Controls.Add(lblInfo);
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
                        _sheetInfos.Clear();
                        pnlEnergy.Visible = false;
                        pnlPower.Visible = false;

                        // Обрабатываем каждый лист
                        foreach (DataTable sheet in result.Tables)
                        {
                            var sheetInfo = new SheetInfo
                            {
                                Name = sheet.TableName,
                                DataTable = sheet,
                                Type = DetectSheetType(sheet.TableName)
                            };

                            if (sheetInfo.Type != SheetType.Unknown)
                            {
                                ParseSheetData(sheetInfo);
                                _sheetInfos.Add(sheetInfo);
                            }
                        }

                        DisplaySystems();
                        btnSave.Enabled = _sheetInfos.Count > 0;
                        lblTitle.Text = $"Обновление коэффициентов ({_sheetInfos.Count} листов)";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private SheetType DetectSheetType(string sheetName)
        {
            string sheetNameLower = sheetName.ToLower();

            // Проверяем на электроэнергию
            foreach (var keyword in EnergyKeywords)
            {
                if (sheetNameLower.Contains(keyword.ToLower()))
                {
                    lblSheetType.Text = $"Найдены листы: {sheetName} (электроэнергия)";
                    return SheetType.Energy;
                }
            }

            // Проверяем на мощность
            foreach (var keyword in PowerKeywords)
            {
                if (sheetNameLower.Contains(keyword.ToLower()))
                {
                    lblSheetType.Text = $"Найдены листы: {sheetName} (мощность)";
                    return SheetType.Power;
                }
            }

            return SheetType.Unknown;
        }

        private void ParseSheetData(SheetInfo sheetInfo)
        {
            sheetInfo.Systems = new List<EnergySystem>();
            Dictionary<string, List<DataRow>> systemRows = new Dictionary<string, List<DataRow>>();

            // Парсим все системы из листа
            for (int i = 0; i < sheetInfo.DataTable.Rows.Count; i++)
            {
                var row = sheetInfo.DataTable.Rows[i];
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
                    sheetInfo.Systems.Add(system);
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

        private void DisplaySystems()
        {
            // Очищаем DataGridViews
            dgvEnergySystems.Rows.Clear();
            dgvPowerSystems.Rows.Clear();
            dgvEnergySystems.Columns.Clear();
            dgvPowerSystems.Columns.Clear();

            // Настраиваем колонки для электроэнергии
            dgvEnergySystems.Columns.Add("SystemName", "Энергосистема");
            dgvEnergySystems.Columns.Add("RangeCount", "Диапазонов");
            dgvEnergySystems.Columns.Add("Ranges", "Температурные диапазоны (°C)");

            dgvEnergySystems.Columns["SystemName"].Width = 200;
            dgvEnergySystems.Columns["RangeCount"].Width = 80;
            dgvEnergySystems.Columns["Ranges"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Настраиваем колонки для мощности
            dgvPowerSystems.Columns.Add("SystemName", "Энергосистема");
            dgvPowerSystems.Columns.Add("RangeCount", "Диапазонов");
            dgvPowerSystems.Columns.Add("Ranges", "Температурные диапазоны (°C)");

            dgvPowerSystems.Columns["SystemName"].Width = 200;
            dgvPowerSystems.Columns["RangeCount"].Width = 80;
            dgvPowerSystems.Columns["Ranges"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Заполняем данные
            var energySheets = _sheetInfos.Where(s => s.Type == SheetType.Energy).ToList();
            var powerSheets = _sheetInfos.Where(s => s.Type == SheetType.Power).ToList();

            // Электроэнергия
            if (energySheets.Count > 0)
            {
                pnlEnergy.Visible = true;
                pnlEnergy.Location = new Point(12, 170);

                foreach (var sheet in energySheets)
                {
                    foreach (var system in sheet.Systems)
                    {
                        string rangesText = string.Join("; ", system.Ranges
                            .OrderBy(r => r.From)
                            .Select(r => $"{r.From:F0}...{r.To:F0}°C"));

                        dgvEnergySystems.Rows.Add(
                            system.Name,
                            system.Ranges.Count,
                            rangesText
                        );
                    }
                }
            }

            // Мощность
            if (powerSheets.Count > 0)
            {
                pnlPower.Visible = true;
                if (pnlEnergy.Visible)
                {
                    pnlPower.Location = new Point(12, pnlEnergy.Location.Y + pnlEnergy.Height + 10);
                }
                else
                {
                    pnlPower.Location = new Point(12, 170);
                }

                foreach (var sheet in powerSheets)
                {
                    foreach (var system in sheet.Systems)
                    {
                        string rangesText = string.Join("; ", system.Ranges
                            .OrderBy(r => r.From)
                            .Select(r => $"{r.From:F0}...{r.To:F0}°C"));

                        dgvPowerSystems.Rows.Add(
                            system.Name,
                            system.Ranges.Count,
                            rangesText
                        );
                    }
                }
            }

            // Обновляем информацию
            int totalEnergySystems = energySheets.Sum(s => s.Systems.Count);
            int totalPowerSystems = powerSheets.Sum(s => s.Systems.Count);

            lblEnergyInfo.Text = $"Коэффициенты для электроэнергии: {totalEnergySystems} систем";
            lblPowerInfo.Text = $"Коэффициенты для мощности: {totalPowerSystems} систем";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string setName = txtSetName.Text;

            if (setName == "Например: Коэффициенты 2024" || string.IsNullOrWhiteSpace(setName))
            {
                MessageBox.Show("Введите название набора коэффициентов", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_sheetInfos.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                DateTime loadDate = DateTime.Now;
                int totalSaved = 0;
                int totalSystems = _sheetInfos.Sum(s => s.Systems.Count);

                // Показываем прогресс
                var progressForm = new ProcessingFormWithCancel();
                progressForm.CancelRequested += (s, args) =>
                {
                    progressForm.UpdateProgress("Отмена сохранения...", 0);
                };
                progressForm.Show();
                progressForm.UpdateProgress("Начинаю сохранение...", 0);

                int processed = 0;

                // Сохраняем электроэнергию
                var energySheets = _sheetInfos.Where(s => s.Type == SheetType.Energy).ToList();
                if (energySheets.Count > 0 && !progressForm.IsCancellationRequested)
                {
                    progressForm.UpdateProgress("Сохранение коэффициентов электроэнергии...", 20);
                    int tableTypeId = _dbService.GetOrCreateTableType("Коэффициенты влияния по электроэнергии");

                    foreach (var sheet in energySheets)
                    {
                        if (progressForm.IsCancellationRequested) break;

                        foreach (var system in sheet.Systems)
                        {
                            try
                            {
                                int systemId = _dbService.GetOrCreateEnergySystem(system.Name);
                                _dbService.UpdatePreviousParamSetEndDate(systemId, loadDate);

                                string fullSetName = $"{setName} - {sheet.Name}";
                                int paramSetId = _dbService.CreateForecastParamSet(
                                    fullSetName, systemId, tableTypeId, loadDate);

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
                                totalSaved++;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
                            }

                            processed++;
                            int progress = 20 + (processed * 60 / Math.Max(1, totalSystems));
                            progressForm.UpdateProgress($"Сохранено {processed} из {totalSystems}...", progress);
                        }
                    }
                }

                // Сохраняем мощность
                var powerSheets = _sheetInfos.Where(s => s.Type == SheetType.Power).ToList();
                if (powerSheets.Count > 0 && !progressForm.IsCancellationRequested)
                {
                    progressForm.UpdateProgress("Сохранение коэффициентов мощности...", 80);
                    int tableTypeId = _dbService.GetOrCreateTableType("Коэффициенты влияния по мощности");

                    foreach (var sheet in powerSheets)
                    {
                        if (progressForm.IsCancellationRequested) break;

                        foreach (var system in sheet.Systems)
                        {
                            try
                            {
                                int systemId = _dbService.GetOrCreateEnergySystem(system.Name);
                                _dbService.UpdatePreviousParamSetEndDate(systemId, loadDate);

                                string fullSetName = $"{setName} - {sheet.Name}";
                                int paramSetId = _dbService.CreateForecastParamSet(
                                    fullSetName, systemId, tableTypeId, loadDate);

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
                                totalSaved++;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
                            }

                            processed++;
                            int progress = 80 + (processed * 20 / Math.Max(1, totalSystems));
                            progressForm.UpdateProgress($"Сохранено {processed} из {totalSystems}...", progress);
                        }
                    }
                }

                if (progressForm.IsCancellationRequested)
                {
                    progressForm.UpdateProgress("Сохранение отменено", 0);
                    System.Threading.Thread.Sleep(1000);
                    progressForm.Close();
                    MessageBox.Show("Сохранение отменено пользователем", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                progressForm.UpdateProgress("Сохранение завершено!", 100);
                System.Threading.Thread.Sleep(500);
                progressForm.Close();

                MessageBox.Show($"Коэффициенты успешно сохранены в БД!\n\n" +
                               $"Листов: {_sheetInfos.Count}\n" +
                               $"Сохранено систем: {totalSaved}\n" +
                               $"Дата: {loadDate:dd.MM.yyyy HH:mm}",
                               "Успешно",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    // Вспомогательный класс
    public class SheetInfo
    {
        public string Name { get; set; }
        public DataTable DataTable { get; set; }
        public SheetType Type { get; set; }
        public List<EnergySystem> Systems { get; set; }
    }
}