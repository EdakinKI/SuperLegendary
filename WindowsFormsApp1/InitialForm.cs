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
        private DbForecastParams _selectedParams;
        private List<EnergySystem> _energySystems = new List<EnergySystem>();

        public InitialFormDb()
        {
            InitializeComponent();
            InitializeEventHandlers();
            LoadDataFromDatabase();
            ConfigureVisibility(); // Только управление видимостью
        }

        private void ConfigureVisibility()
        {
            // Изначально скрываем таблицы
            gbPowerRanges.Visible = false;
            gbEnergyRanges.Visible = false;

            // Устанавливаем правильные названия
            this.Text = "Расчет температурных зависимостей (БД)";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
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
                // Загружаем параметры из БД - метод возвращает DbForecastParams
                _selectedParams = _dbService.GetForecastParams(systemName, paramSetName);

                if (_selectedParams != null)
                {
                    // Преобразуем в формат EnergySystem для отображения
                    _energySystems.Clear();
                    var energySystem = new EnergySystem // ← ИСПОЛЬЗУЙТЕ ClassLibrary1.EnergySystem
                    {
                        Name = _selectedParams.SystemName, // ← DbForecastParams имеет SystemName
                        Ranges = _selectedParams.Ranges.Select(r => new TemperatureRange // ← ClassLibrary1.TemperatureRange
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
            // Проверяем _selectedParams вместо _energySystems
            if (_selectedParams == null || _selectedParams.Ranges == null) return;

            string selectedType = cmbCalculationType.SelectedItem?.ToString();

            // Скрываем все таблицы сначала
            gbPowerRanges.Visible = false;
            gbEnergyRanges.Visible = false;

            int currentY = btnCalculate.Location.Y + btnCalculate.Height + 20;

            if (selectedType == "По электроэнергии" || selectedType == "По мощности и электроэнергии")
            {
                DisplayRangesInGrid(_energySystems.FirstOrDefault(), dgvEnergyRanges, "Электроэнергия");
                gbEnergyRanges.Visible = true;
                gbEnergyRanges.Location = new Point(10, currentY);
                gbEnergyRanges.Text = "Коэффициенты влияния для ЭЛЕКТРОЭНЕРГИИ";
                currentY += gbEnergyRanges.Height + 10;
            }

            if (selectedType == "По мощности" || selectedType == "По мощности и электроэнергии")
            {
                DisplayRangesInGrid(_energySystems.FirstOrDefault(), dgvPowerRanges, "Мощность");
                gbPowerRanges.Visible = true;

                // Если уже есть таблица для электроэнергии, позиционируем ниже
                if (gbEnergyRanges.Visible)
                {
                    gbPowerRanges.Location = new Point(10, gbEnergyRanges.Location.Y + gbEnergyRanges.Height + 10);
                }
                else
                {
                    gbPowerRanges.Location = new Point(10, currentY);
                }

                gbPowerRanges.Text = "Коэффициенты влияния для МОЩНОСТИ";
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
                // Определяем правильный тип расчета на основе выбранного в интерфейсе
                string dbCalculationType;
                switch (calculationType)
                {
                    case "По электроэнергии":
                        dbCalculationType = "Прогноз потребления по электроэнергии";
                        break;
                    case "По мощности":
                        dbCalculationType = "Прогноз потребления по мощности";
                        break;
                    case "По мощности и электроэнергии":
                        dbCalculationType = "Прогноз потребления по мощности и электроэнергии";
                        break;
                    default:
                        dbCalculationType = "Прогноз потребления по мощности"; // fallback
                        break;
                }

                // Получаем ID типа расчета
                int typeId = _dbService.GetOrCreateCalculationType(dbCalculationType);

                // Получаем ID энергосистемы
                int systemId = _dbService.GetOrCreateEnergySystem(systemName);

                // Получаем ID набора параметров
                var paramsDb = _dbService.GetForecastParams(systemName, paramSetName);
                if (paramsDb == null) return;

                // Создаем расчет - ИСПОЛЬЗУЕМ DbForecastCalculation
                var forecastCalc = new DbForecastCalculation
                {
                    TypeId = typeId,
                    CalculationName = $"{calculationType} - {systemName} - {forecastDate:dd.MM.yyyy}",
                    CalculationDate = DateTime.Now,
                    TargetDate = forecastDate,
                    SystemId = systemId,
                    TOriginal = t1,
                    TResult = t2,
                    ParamSetId = paramsDb.ParamSetId // ← paramsDb это DbForecastParams
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
                    System.Diagnostics.Debug.WriteLine($"Расчет прогноза сохранен в БД с ID: {forecastId}, Тип: {dbCalculationType}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения расчета в БД: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (cmbSystems.SelectedItem == null || cmbCalculationType.SelectedItem == null)
                return false;

            if (string.IsNullOrEmpty(txtT1.Text) || string.IsNullOrEmpty(txtT2.Text))
                return false;

            if (!double.TryParse(txtT1.Text, out _) || !double.TryParse(txtT2.Text, out _))
                return false;

            string calculationType = cmbCalculationType.SelectedItem.ToString();

            if (calculationType == "По электроэнергии")
            {
                if (string.IsNullOrEmpty(txtE1.Text) || !double.TryParse(txtE1.Text, out _))
                    return false;
            }
            else if (calculationType == "По мощности")
            {
                if (string.IsNullOrEmpty(txtP1.Text) || !double.TryParse(txtP1.Text, out _))
                    return false;
            }
            else if (calculationType == "По мощности и электроэнергии")
            {
                if (string.IsNullOrEmpty(txtP1.Text) || !double.TryParse(txtP1.Text, out _) ||
                    string.IsNullOrEmpty(txtE1.Text) || !double.TryParse(txtE1.Text, out _))
                    return false;
            }

            if (dtpForecastDate.Value < new DateTime(2000, 1, 1))
            {
                MessageBox.Show("Пожалуйста, выберите корректную дату.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private (double FinalValue, List<(double Value, string Description)> IntermediateResults)
            Calculate(double initialValue, double t1, double t2, List<TemperatureRange> ranges, string calculationType)
        {
            List<(double Value, string Description)> intermediateResults = new List<(double, string)>();
            double currentValue = initialValue;
            double currentTemp = t1;

            var sortedRanges = ranges.OrderBy(r => r.From).ToList();
            bool movingToWarmer = t2 > t1;

            int startIndex = FindRangeIndex(sortedRanges, t1, movingToWarmer);
            if (startIndex == -1) return (initialValue, intermediateResults);

            string unit = calculationType == "Электроэнергия" ? "млн кВт·ч" : "МВт";
            string prefix = calculationType == "Электроэнергия" ? "E" : "P";

            intermediateResults.Add((currentValue,
                $"Начальное значение: {prefix}1 = {initialValue} {unit} при T={t1}°C"));

            int step = 1;

            if (movingToWarmer)
            {
                for (int i = startIndex; i < sortedRanges.Count; i++)
                {
                    var currentRange = sortedRanges[i];
                    double targetTemp = Math.Min(currentRange.To, t2);
                    double deltaTemp = targetTemp - currentTemp;
                    double exponent = (currentRange.Coefficient / 100.0) * deltaTemp;
                    currentValue = currentValue * Math.Exp(exponent);
                    currentTemp = targetTemp;

                    intermediateResults.Add((
                        Math.Round(currentValue, 2),
                        $"Шаг {step}: {prefix} = {Math.Round(currentValue, 2)} {unit} при T={targetTemp}°C " +
                        $"(k={currentRange.Coefficient}, ΔT={deltaTemp:F1}°C)"
                    ));

                    step++;
                    if (Math.Abs(currentTemp - t2) < 0.001) break;
                }
            }
            else
            {
                for (int i = startIndex; i >= 0; i--)
                {
                    var currentRange = sortedRanges[i];
                    double targetTemp = Math.Max(currentRange.From, t2);
                    double deltaTemp = targetTemp - currentTemp;
                    double exponent = (currentRange.Coefficient / 100.0) * deltaTemp;
                    currentValue = currentValue * Math.Exp(exponent);
                    currentTemp = targetTemp;

                    intermediateResults.Add((
                        Math.Round(currentValue, 2),
                        $"Шаг {step}: {prefix} = {Math.Round(currentValue, 2)} {unit} при T={targetTemp}°C " +
                        $"(k={currentRange.Coefficient}, ΔT={deltaTemp:F1}°C)"
                    ));

                    step++;
                    if (Math.Abs(currentTemp - t2) < 0.001) break;
                }
            }

            return (Math.Round(currentValue, 2), intermediateResults);
        }

        private int FindRangeIndex(List<TemperatureRange> ranges, double temperature, bool movingToWarmer)
        {
            for (int i = 0; i < ranges.Count; i++)
            {
                if (temperature >= ranges[i].From && temperature <= ranges[i].To)
                    return i;
            }

            if (temperature < ranges.First().From)
                return 0;
            else if (temperature > ranges.Last().To)
                return ranges.Count - 1;
            else
            {
                for (int i = 0; i < ranges.Count - 1; i++)
                {
                    if (temperature > ranges[i].To && temperature < ranges[i + 1].From)
                        return movingToWarmer ? i + 1 : i;
                }
            }

            return -1;
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != '-' && e.KeyChar != '.' && e.KeyChar != ',')
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '-' && textBox.Text.IndexOf('-') > -1)
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar == '.' || e.KeyChar == ',') &&
                (textBox.Text.IndexOf('.') > -1 || textBox.Text.IndexOf(',') > -1))
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '-' && textBox.SelectionStart != 0)
            {
                e.Handled = true;
            }
        }
    }
}