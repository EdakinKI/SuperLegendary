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
        private List<EnergySystem> _energySystems = new List<EnergySystem>();
        private List<EnergySystem> _powerSystems = new List<EnergySystem>();

        public UpdateCoefForm()
        {
            InitializeComponent();
            ConfigureForm();
            InitializePlaceholderText(); // Инициализация placeholder
        }

        private void ConfigureForm()
        {
            this.Text = "Обновление коэффициентов влияния";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(600, 400);
        }

        private void InitializePlaceholderText()
        {
            // Инициализация placeholder для txtSetName
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
                        _excelData = result.Tables[0];
                        ParseExcelData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParseExcelData()
        {
            _energySystems.Clear();
            _powerSystems.Clear();
            cmbSystems.Items.Clear();

            var sheetInfo = AnalyzeSheet(_excelData);

            if (sheetInfo.Type == SheetType.Energy)
            {
                ParseSystemData(_excelData, _energySystems, "Энергия");
                lblSheetType.Text = "Тип листа: Коэффициенты для электроэнергии";
            }
            else if (sheetInfo.Type == SheetType.Power)
            {
                ParseSystemData(_excelData, _powerSystems, "Мощность");
                lblSheetType.Text = "Тип листа: Коэффициенты для мощности";
            }
            else
            {
                lblSheetType.Text = "Тип листа: Не определен";
                return;
            }

            var allSystems = new List<string>();
            allSystems.AddRange(_energySystems.Select(es => es.Name).Distinct());
            allSystems.AddRange(_powerSystems.Select(ps => ps.Name).Distinct());

            var uniqueSystems = allSystems.Distinct().OrderBy(name => name).ToList();

            foreach (var system in uniqueSystems)
            {
                cmbSystems.Items.Add(system);
            }

            if (cmbSystems.Items.Count > 0)
            {
                cmbSystems.SelectedIndex = 0;
                btnSave.Enabled = true;
            }
        }

        private SheetInfo AnalyzeSheet(DataTable dataTable)
        {
            string sheetName = dataTable.TableName;
            string sheetNameLower = sheetName.ToLower();

            var sheetInfo = new SheetInfo
            {
                Name = sheetName,
                DataTable = dataTable
            };

            // Проверяем на лист с электроэнергией
            string[] energyKeywords = { "энерг", "электро", "ээ", "energy" };
            foreach (string keyword in energyKeywords)
            {
                if (sheetNameLower.Contains(keyword))
                {
                    sheetInfo.Type = SheetType.Energy;
                    return sheetInfo;
                }
            }

            // Проверяем на лист с мощностью
            string[] powerKeywords = { "м", "мощность", "power" };
            foreach (string keyword in powerKeywords)
            {
                if (sheetNameLower.Contains(keyword))
                {
                    sheetInfo.Type = SheetType.Power;
                    return sheetInfo;
                }
            }

            sheetInfo.Type = SheetType.Unknown;
            return sheetInfo;
        }

        private void ParseSystemData(DataTable dataTable, List<EnergySystem> systems, string systemType)
        {
            Dictionary<string, List<DataRow>> systemRows = new Dictionary<string, List<DataRow>>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
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

                var system = systems.FirstOrDefault(es => es.Name == systemName);
                if (system == null)
                {
                    system = new EnergySystem { Name = systemName };
                    systems.Add(system);
                }

                for (int i = 0; i < rows.Count; i += 3)
                {
                    if (i + 2 < rows.Count)
                    {
                        var rangeRows = rows.Skip(i).Take(3).ToList();
                        ParseRangeData(system, rangeRows);
                    }
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

        private void cmbSystems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSystems.SelectedItem == null) return;

            string systemName = cmbSystems.SelectedItem.ToString();
            DisplayRanges(systemName);
        }

        private void DisplayRanges(string systemName)
        {
            dgvRanges.Rows.Clear();
            dgvRanges.Columns.Clear();

            dgvRanges.Columns.Add("Number", "№");
            dgvRanges.Columns.Add("From", "Нижняя граница (°C)");
            dgvRanges.Columns.Add("To", "Верхняя граница (°C)");
            dgvRanges.Columns.Add("Coefficient", "Коэффициент");

            var energySystem = _energySystems.FirstOrDefault(es => es.Name == systemName);
            var powerSystem = _powerSystems.FirstOrDefault(ps => ps.Name == systemName);

            var ranges = new List<TemperatureRange>();
            if (energySystem != null) ranges.AddRange(energySystem.Ranges);
            if (powerSystem != null) ranges.AddRange(powerSystem.Ranges);

            int rowIndex = 1;
            foreach (var range in ranges.OrderBy(r => r.From))
            {
                dgvRanges.Rows.Add(
                    rowIndex,
                    range.From.ToString("F1"),
                    range.To.ToString("F1"),
                    range.Coefficient.ToString("F4")
                );
                rowIndex++;
            }
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

            if (cmbSystems.SelectedItem == null)
            {
                MessageBox.Show("Выберите энергосистему", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string systemName = cmbSystems.SelectedItem.ToString();
                DateTime loadDate = DateTime.Now;

                // Получаем или создаем энергосистему
                int systemId = _dbService.GetOrCreateEnergySystem(systemName);

                // Обновляем дату окончания предыдущего набора
                _dbService.UpdatePreviousParamSetEndDate(systemId, loadDate);

                // Создаем новый набор параметров
                int paramSetId = _dbService.CreateForecastParamSet(setName, systemId, loadDate);

                // Сохраняем температурные диапазоны
                var energySystem = _energySystems.FirstOrDefault(es => es.Name == systemName);
                var powerSystem = _powerSystems.FirstOrDefault(ps => ps.Name == systemName);

                var allRanges = new List<TemperatureRange>();
                if (energySystem != null) allRanges.AddRange(energySystem.Ranges);
                if (powerSystem != null) allRanges.AddRange(powerSystem.Ranges);

                var dbRanges = new List<TemperatureRangeDb>();
                int rangeNumber = 1;
                foreach (var range in allRanges.OrderBy(r => r.From))
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

                MessageBox.Show($"Коэффициенты успешно сохранены!\n\n" +
                               $"Система: {systemName}\n" +
                               $"Набор: {setName}\n" +
                               $"Диапазонов: {dbRanges.Count}\n" +
                               $"Дата загрузки: {loadDate:dd.MM.yyyy HH:mm}",
                               "Успешно",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в базу данных:\n\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}