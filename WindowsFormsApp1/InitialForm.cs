using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClassLibrary1;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1
{
    public partial class InitialFormDb : Form
    {
        private DatabaseService _dbService = new DatabaseService();
        private ForecastParamsDb _selectedParams;
        private List<EnergySystem> _energySystems = new List<EnergySystem>();

        public InitialFormDb()
        {
            InitializeComponent();
            ConfigureForm();
            InitializeEventHandlers();
            LoadDataFromDatabase();
        }

        private void ConfigureForm()
        {
            this.Text = "Расчет температурных зависимостей (БД)";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(1000, 700);

            // Изначально скрываем таблицы
            gbPowerRanges.Visible = false;
            gbEnergyRanges.Visible = false;
        }

        private void InitializeEventHandlers()
        {
            cmbCalculationType.SelectedIndexChanged += CmbCalculationType_SelectedIndexChanged;
            cmbSystems.SelectedIndexChanged += CmbSystems_SelectedIndexChanged;
            cmbParamSets.SelectedIndexChanged += CmbParamSets_SelectedIndexChanged;
            btnCalculate.Click += BtnCalculate_Click;

            // Валидация ввода
            txtT1.KeyPress += NumericTextBox_KeyPress;
            txtT2.KeyPress += NumericTextBox_KeyPress;
            txtP1.KeyPress += NumericTextBox_KeyPress;
            txtE1.KeyPress += NumericTextBox_KeyPress;
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                // Загружаем типы расчетов
                cmbCalculationType.Items.Clear();
                cmbCalculationType.Items.Add("По электроэнергии");
                cmbCalculationType.Items.Add("По мощности");
                cmbCalculationType.Items.Add("По мощности и электроэнергии");

                // Загружаем энергосистемы
                var systems = _dbService.GetAllEnergySystems();
                cmbSystems.Items.Clear();
                foreach (var system in systems)
                {
                    cmbSystems.Items.Add(system);
                }

                EnableCalculationType(cmbSystems.Items.Count > 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных из БД:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnableCalculationType(bool enabled)
        {
            lblCalculationType.Enabled = enabled;
            lblCalculationType.ForeColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            cmbCalculationType.Enabled = enabled;
        }

        private void EnableSystemSelection(bool enabled)
        {
            lblSystem.Enabled = enabled;
            lblSystem.ForeColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            cmbSystems.Enabled = enabled;
        }

        private void EnableParamSetSelection(bool enabled)
        {
            lblParamSet.Enabled = enabled;
            lblParamSet.ForeColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            cmbParamSets.Enabled = enabled;
        }

        private void EnableInputControls(bool enabled)
        {
            // Температуры
            lblT1.Enabled = enabled;
            lblT1.ForeColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            txtT1.Enabled = enabled;

            lblT2.Enabled = enabled;
            lblT2.ForeColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            txtT2.Enabled = enabled;

            // Мощность/Энергия
            lblP1.Enabled = enabled;
            lblP1.ForeColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            txtP1.Enabled = enabled;

            lblE1.Enabled = enabled;
            lblE1.ForeColor = enabled ? System.Drawing.SystemColors.ControlText : System.Drawing.SystemColors.GrayText;
            txtE1.Enabled = enabled;

            // Дата
            lblForecastDate.Enabled = enabled;
            lblForecastDate.ForeColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            dtpForecastDate.Enabled = enabled;

            // Кнопка расчета
            btnCalculate.Enabled = enabled && (cmbSystems.SelectedItem != null) && (cmbParamSets.SelectedItem != null);
        }

        private void CmbCalculationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCalculationType.SelectedItem == null) return;

            string selectedType = cmbCalculationType.SelectedItem.ToString();
            ConfigureInputFields(selectedType);
            EnableSystemSelection(true);
        }

        private void ConfigureInputFields(string calculationType)
        {
            // Скрываем/показываем поля в зависимости от типа расчета
            switch (calculationType)
            {
                case "По электроэнергии":
                    lblP1.Visible = false;
                    txtP1.Visible = false;
                    txtP1.Text = "";

                    lblE1.Visible = true;
                    txtE1.Visible = true;
                    txtE1.Text = "";
                    break;

                case "По мощности":
                    lblP1.Visible = true;
                    txtP1.Visible = true;
                    txtP1.Text = "";

                    lblE1.Visible = false;
                    txtE1.Visible = false;
                    txtE1.Text = "";
                    break;

                case "По мощности и электроэнергии":
                    lblP1.Visible = true;
                    txtP1.Visible = true;
                    txtP1.Text = "";

                    lblE1.Visible = true;
                    txtE1.Visible = true;
                    txtE1.Text = "";
                    break;
            }

            EnableInputControls(true);
        }

        private void CmbSystems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSystems.SelectedItem == null) return;

            string systemName = cmbSystems.SelectedItem.ToString();
            LoadParamSetsForSystem(systemName);
        }

        private void LoadParamSetsForSystem(string systemName)
        {
            try
            {
                cmbParamSets.Items.Clear();
                var paramSets = _dbService.GetAvailableParamSets(systemName);

                foreach (var paramSet in paramSets)
                {
                    cmbParamSets.Items.Add(paramSet);
                }

                EnableParamSetSelection(paramSets.Count > 0);

                if (paramSets.Count > 0)
                {
                    cmbParamSets.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки наборов параметров:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbParamSets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSystems.SelectedItem == null || cmbParamSets.SelectedItem == null) return;

            string systemName = cmbSystems.SelectedItem.ToString();
            string paramSetName = cmbParamSets.SelectedItem.ToString();

            try
            {
                // Загружаем параметры из БД
                _selectedParams = _dbService.GetForecastParams(systemName, paramSetName);

                if (_selectedParams != null)
                {
                    // Преобразуем в формат EnergySystem для отображения
                    _energySystems.Clear();
                    var energySystem = new EnergySystem
                    {
                        Name = _selectedParams.SystemName,
                        Ranges = _selectedParams.Ranges.Select(r => new TemperatureRange
                        {
                            From = r.TempLower ?? 0,
                            To = r.TempUpper ?? 0,
                            Coefficient = r.Coefficient ?? 0
                        }).ToList()
                    };
                    _energySystems.Add(energySystem);

                    // Отображаем диапазоны
                    DisplayRangesFromDatabase();
                    EnableInputControls(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки коэффициентов:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayRangesFromDatabase()
        {
            if (_selectedParams == null || _selectedParams.Ranges == null) return;

            string selectedType = cmbCalculationType.SelectedItem?.ToString();

            if (selectedType == "По электроэнергии" || selectedType == "По мощности и электроэнергии")
            {
                DisplayRangesInGrid(_energySystems.FirstOrDefault(), dgvEnergyRanges, "Электроэнергия");
                gbEnergyRanges.Visible = true;
                gbEnergyRanges.Location = new Point(0, 0);
            }

            if (selectedType == "По мощности" || selectedType == "По мощности и электроэнергии")
            {
                DisplayRangesInGrid(_energySystems.FirstOrDefault(), dgvPowerRanges, "Мощность");
                gbPowerRanges.Visible = true;
                gbPowerRanges.Location = selectedType == "По мощности и электроэнергии" ?
                    new Point(0, 250) : new Point(0, 0);
            }
        }

        private void DisplayRangesInGrid(EnergySystem system, DataGridView grid, string title)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();

            if (system == null || system.Ranges == null || system.Ranges.Count == 0)
            {
                grid.Columns.Add("Message", "Сообщение");
                grid.Rows.Add($"Для энергосистемы нет данных по {title.ToLower()}");
                grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                return;
            }

            grid.Columns.Add("Column1", "№");
            grid.Columns.Add("Column2", "Верхняя граница (°C)");
            grid.Columns.Add("Column3", "Нижняя граница (°C)");
            grid.Columns.Add("Column4", "Коэффициент влияния");

            int rowIndex = 0;
            foreach (var range in system.Ranges.OrderBy(r => r.From))
            {
                grid.Rows.Add(
                    rowIndex + 1,
                    range.To.ToString("F1"),
                    range.From.ToString("F1"),
                    range.Coefficient.ToString("F4")
                );
                rowIndex++;
            }

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    double t1 = double.Parse(txtT1.Text);
                    double t2 = double.Parse(txtT2.Text);
                    string systemName = cmbSystems.SelectedItem.ToString();
                    string paramSetName = cmbParamSets.SelectedItem.ToString();
                    string calculationType = cmbCalculationType.SelectedItem.ToString();
                    DateTime forecastDate = dtpForecastDate.Value;

                    double p1 = 0;
                    double e1 = 0;
                    double finalPowerEnergy = 0;
                    double finalPowerPower = 0;
                    List<(double Value, string Description)> intermediateResultsEnergy = new List<(double, string)>();
                    List<(double Value, string Description)> intermediateResultsPower = new List<(double, string)>();

                    // Получаем значения в зависимости от типа расчета
                    if (calculationType == "По электроэнергии")
                    {
                        e1 = double.Parse(txtE1.Text);
                    }
                    else if (calculationType == "По мощности")
                    {
                        p1 = double.Parse(txtP1.Text);
                    }
                    else if (calculationType == "По мощности и электроэнергии")
                    {
                        p1 = double.Parse(txtP1.Text);
                        e1 = double.Parse(txtE1.Text);
                    }

                    // Выполняем расчеты
                    if (calculationType == "По электроэнергии" || calculationType == "По мощности и электроэнергии")
                    {
                        var result = Calculate(e1, t1, t2, _energySystems.FirstOrDefault()?.Ranges, "Электроэнергия");
                        finalPowerEnergy = result.FinalValue;
                        intermediateResultsEnergy = result.IntermediateResults;
                    }

                    if (calculationType == "По мощности" || calculationType == "По мощности и электроэнергии")
                    {
                        var result = Calculate(p1, t1, t2, _energySystems.FirstOrDefault()?.Ranges, "Мощность");
                        finalPowerPower = result.FinalValue;
                        intermediateResultsPower = result.IntermediateResults;
                    }

                    // Сохраняем в базу данных (АВТОМАТИЧЕСКОЕ СОХРАНЕНИЕ)
                    SaveCalculationToDatabase(
                        systemName, paramSetName, calculationType,
                        t1, t2, p1, e1, forecastDate,
                        finalPowerPower, finalPowerEnergy,
                        intermediateResultsPower, intermediateResultsEnergy
                    );

                    // Открываем форму результатов
                    ResultForm resultForm = new ResultForm(
                        systemName,
                        t1,
                        t2,
                        p1,
                        e1,
                        forecastDate,
                        finalPowerEnergy,
                        finalPowerPower,
                        intermediateResultsEnergy,
                        intermediateResultsPower,
                        _energySystems.FirstOrDefault()?.Ranges,
                        _energySystems.FirstOrDefault()?.Ranges,
                        calculationType
                    );
                    resultForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при расчете: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, проверьте введенные данные.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCalculationToDatabase(
            string systemName, string paramSetName, string calculationType,
            double t1, double t2, double p1, double e1, DateTime forecastDate,
            double finalPower, double finalEnergy,
            List<(double Value, string Description)> intermediateResultsPower,
            List<(double Value, string Description)> intermediateResultsEnergy)
        {
            try
            {
                // Получаем ID типа расчета
                int typeId = _dbService.GetOrCreateCalculationType("Прогноз потребления мощности");

                // Получаем ID энергосистемы
                int systemId = _dbService.GetOrCreateEnergySystem(systemName);

                // Получаем ID набора параметров
                var paramsDb = _dbService.GetForecastParams(systemName, paramSetName);
                if (paramsDb == null) return;

                // Создаем расчет
                var forecastCalc = new ForecastCalculationDb
                {
                    TypeId = typeId,
                    CalculationName = $"{calculationType} - {systemName} - {forecastDate:dd.MM.yyyy}",
                    CalculationDate = DateTime.Now,
                    TargetDate = forecastDate,
                    SystemId = systemId,
                    TOriginal = t1,
                    TResult = t2,
                    ParamSetId = paramsDb.ParamSetId
                };

                // Заполняем значения в зависимости от типа расчета
                switch (calculationType)
                {
                    case "По электроэнергии":
                        forecastCalc.EOriginal = e1;
                        forecastCalc.EResult = finalEnergy;
                        break;
                    case "По мощности":
                        forecastCalc.POriginal = p1;
                        forecastCalc.PResult = finalPower;
                        break;
                    case "По мощности и электроэнергии":
                        forecastCalc.POriginal = p1;
                        forecastCalc.EOriginal = e1;
                        forecastCalc.PResult = finalPower;
                        forecastCalc.EResult = finalEnergy;
                        break;
                }

                // Сохраняем в БД
                int forecastId = _dbService.SaveForecastCalculation(forecastCalc);

                if (forecastId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Расчет прогноза сохранен в БД с ID: {forecastId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения расчета в БД: {ex.Message}");
            }
        }

        // Остальные методы (Calculate, ValidateInput, FindRangeIndex, NumericTextBox_KeyPress)
        // остаются такими же, как в оригинальном InitialForm
        // ...

        private (double FinalValue, List<(double Value, string Description)> IntermediateResults)
            Calculate(double initialValue, double t1, double t2, List<TemperatureRange> ranges, string calculationType)
        {
            // Реализация метода Calculate из оригинального InitialForm
            // ...
            return (initialValue, new List<(double, string)>());
        }

        private bool ValidateInput()
        {
            // Реализация метода ValidateInput из оригинального InitialForm
            // ...
            return true;
        }

        private int FindRangeIndex(List<TemperatureRange> ranges, double temperature, bool movingToWarmer)
        {
            // Реализация метода FindRangeIndex из оригинального InitialForm
            // ...
            return 0;
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Реализация метода NumericTextBox_KeyPress из оригинального InitialForm
            // ...
        }
    }
}