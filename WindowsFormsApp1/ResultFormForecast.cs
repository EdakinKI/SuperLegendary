using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClassLibrary1;

namespace WindowsFormsApp1
{
    public partial class ResultForm : Form
    {
        public ResultForm()
        {
            InitializeComponent();
        }

        public ResultForm(
            string systemName,
            double t1,
            double t2,
            double p1,
            double e1,
            DateTime forecastDate,
            double finalPowerEnergy,
            double finalPowerPower,
            List<(double Value, string Description)> intermediateResultsEnergy,
            List<(double Value, string Description)> intermediateResultsPower,
            List<TemperatureRange> energyRanges,
            List<TemperatureRange> powerRanges,
            string calculationType) : this()
        {
            InitializeFormContent(
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
                energyRanges,
                powerRanges,
                calculationType
            );
        }

        private void InitializeFormContent(
            string systemName,
            double t1,
            double t2,
            double p1,
            double e1,
            DateTime forecastDate,
            double finalPowerEnergy,
            double finalPowerPower,
            List<(double Value, string Description)> intermediateResultsEnergy,
            List<(double Value, string Description)> intermediateResultsPower,
            List<TemperatureRange> energyRanges,
            List<TemperatureRange> powerRanges,
            string calculationType)
        {
            // Настройка формы
            this.Text = "Результаты расчета";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Заполнение исходных данных
            FillInputData(systemName, t1, t2, p1, e1, forecastDate, calculationType);

            // Заполнение результатов расчета
            FillResultsData(finalPowerEnergy, finalPowerPower, calculationType);

            // Настройка таблиц в зависимости от типа расчета
            int currentY = 140; // Начальная позиция после gbInput

            if (calculationType == "По мощности и электроэнергии")
            {
                // Показываем все 4 таблицы
                ConfigurePowerTables(intermediateResultsPower, powerRanges, ref currentY);
                ConfigureEnergyTables(intermediateResultsEnergy, energyRanges, ref currentY);

                // Показываем все группы
                gbPowerSteps.Visible = true;
                gbPowerRanges.Visible = true;
                gbEnergySteps.Visible = true;
                gbEnergyRanges.Visible = true;
                gbResults.Location = new Point(18, currentY);
                currentY += 120;
            }
            else if (calculationType == "По электроэнергии")
            {
                // Показываем только таблицы для электроэнергии
                gbPowerSteps.Visible = false;
                gbPowerRanges.Visible = false;

                ConfigureEnergyTables(intermediateResultsEnergy, energyRanges, ref currentY, true);

                // Меняем заголовки
                gbEnergySteps.Text = "Шаги расчета (Электроэнергия)";
                gbEnergyRanges.Text = "Диапазоны температур и коэффициенты (Электроэнергия)";

                gbResults.Location = new Point(18, currentY);
                currentY += 120;
            }
            else if (calculationType == "По мощности")
            {
                // Показываем только таблицы для мощности
                gbEnergySteps.Visible = false;
                gbEnergyRanges.Visible = false;

                ConfigurePowerTables(intermediateResultsPower, powerRanges, ref currentY, true);

                // Меняем заголовки
                gbPowerSteps.Text = "Шаги расчета (Мощность)";
                gbPowerRanges.Text = "Диапазоны температур и коэффициенты (Мощность)";

                gbResults.Location = new Point(18, currentY);
                currentY += 120;
            }

            // Настройка кнопки закрыть
            btnClose.Click += (s, e) => this.Close();

            // Устанавливаем высоту contentPanel
            contentPanel.Height = currentY + 100;
        }

        private void FillInputData(string systemName, double t1, double t2, double p1, double e1,
                                 DateTime forecastDate, string calculationType)
        {
            inputTable.Controls.Clear();

            // Строка 1
            inputTable.Controls.Add(CreateBoldLabel("Тип расчета:"), 0, 0);
            inputTable.Controls.Add(CreateValueLabel(calculationType), 1, 0);

            inputTable.Controls.Add(CreateBoldLabel("Энергосистема:"), 2, 0);
            inputTable.Controls.Add(CreateValueLabel(systemName), 3, 0);

            inputTable.Controls.Add(CreateBoldLabel("Дата прогноза:"), 4, 0);
            inputTable.Controls.Add(CreateValueLabel(forecastDate.ToString("dd.MM.yyyy"), Color.DarkRed), 5, 0);

            // Строка 2
            inputTable.Controls.Add(CreateBoldLabel("Т1 (°C):"), 0, 1);
            inputTable.Controls.Add(CreateValueLabel(t1.ToString("F1")), 1, 1);

            inputTable.Controls.Add(CreateBoldLabel("Т2 (°C):"), 2, 1);
            inputTable.Controls.Add(CreateValueLabel(t2.ToString("F1")), 3, 1);

            // Строка 3: Входные значения
            if (calculationType == "По электроэнергии")
            {
                inputTable.Controls.Add(CreateBoldLabel("Е1 (млн кВт·ч):"), 0, 2);
                inputTable.Controls.Add(CreateValueLabel(e1.ToString("F2")), 1, 2);
            }
            else if (calculationType == "По мощности")
            {
                inputTable.Controls.Add(CreateBoldLabel("Р1 (МВт):"), 0, 2);
                inputTable.Controls.Add(CreateValueLabel(p1.ToString("F2")), 1, 2);
            }
            else if (calculationType == "По мощности и электроэнергии")
            {
                inputTable.Controls.Add(CreateBoldLabel("Р1 (МВт):"), 0, 2);
                inputTable.Controls.Add(CreateValueLabel(p1.ToString("F2")), 1, 2);

                inputTable.Controls.Add(CreateBoldLabel("Е1 (млн кВт·ч):"), 2, 2);
                inputTable.Controls.Add(CreateValueLabel(e1.ToString("F2")), 3, 2);
            }
        }

        private void FillResultsData(double finalPowerEnergy, double finalPowerPower, string calculationType)
        {
            resultsTable.Controls.Clear();

            if (calculationType == "По электроэнергии" || calculationType == "По мощности и электроэнергии")
            {
                resultsTable.Controls.Add(CreateBoldLabel("Е2 (млн кВт·ч):"), 0, 0);
                resultsTable.Controls.Add(CreateValueLabel(finalPowerEnergy.ToString("F2"), Color.DarkGreen, 11), 1, 0);

                if (calculationType == "По мощности и электроэнергии")
                {
                    resultsTable.Controls.Add(CreateBoldLabel("Р2 (МВт):"), 2, 0);
                    resultsTable.Controls.Add(CreateValueLabel(finalPowerPower.ToString("F2"), Color.DarkBlue, 11), 3, 0);
                }
            }
            else if (calculationType == "По мощности")
            {
                resultsTable.Controls.Add(CreateBoldLabel("Р2 (МВт):"), 0, 0);
                resultsTable.Controls.Add(CreateValueLabel(finalPowerPower.ToString("F2"), Color.DarkBlue, 11), 1, 0);
            }
        }

        private void ConfigurePowerTables(
            List<(double Value, string Description)> intermediateResults,
            List<TemperatureRange> ranges, ref int currentY, bool singleMode = false)
        {
            // Таблица шагов расчета мощности
            gbPowerSteps.Location = new Point(18, currentY);
            currentY += 160;
            ConfigureStepsDataGridView(dgvPowerSteps, intermediateResults, "Мощность, МВт");

            // Таблица диапазонов мощности в формате Excel
            gbPowerRanges.Location = new Point(18, currentY);
            currentY += 160;
            ConfigureExcelFormatRangesDataGridView(dgvPowerRangesGrid, ranges);
        }

        private void ConfigureEnergyTables(
            List<(double Value, string Description)> intermediateResults,
            List<TemperatureRange> ranges, ref int currentY, bool singleMode = false)
        {
            // Таблица шагов расчета электроэнергии
            gbEnergySteps.Location = new Point(18, currentY);
            currentY += 160;
            ConfigureStepsDataGridView(dgvEnergySteps, intermediateResults, "Энергия, млн кВт·ч");

            // Таблица диапазонов электроэнергии в формате Excel
            gbEnergyRanges.Location = new Point(18, currentY);
            currentY += 160;
            ConfigureExcelFormatRangesDataGridView(dgvEnergyRangesGrid, ranges);
        }

        // ... остальные методы без изменений ...

        private void ConfigureStepsDataGridView(DataGridView dgv,
            List<(double Value, string Description)> results, string valueColumnName)
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            dgv.Columns.Add("Step", "Шаг");
            dgv.Columns.Add("Value", valueColumnName);
            dgv.Columns.Add("Description", "Описание");

            dgv.Columns["Step"].Width = 60;
            dgv.Columns["Value"].Width = 120;
            dgv.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            if (results != null && results.Count > 0)
            {
                // Показываем минимум 4 строки
                int rowsToShow = Math.Max(results.Count, 4);

                for (int i = 0; i < rowsToShow; i++)
                {
                    if (i < results.Count)
                    {
                        dgv.Rows.Add(
                            i + 1,
                            results[i].Value.ToString("F2"),
                            results[i].Description
                        );
                    }
                    else
                    {
                        // Заполняем пустыми строками до 4
                        dgv.Rows.Add("", "", "");
                    }
                }
            }
            else
            {
                // Минимум 4 строки
                for (int i = 0; i < 4; i++)
                {
                    dgv.Rows.Add("", "", "");
                }
            }

            // Устанавливаем высоту строк для отображения 4 строк
            dgv.RowTemplate.Height = 25;
        }

        private void ConfigureExcelFormatRangesDataGridView(DataGridView dgv, List<TemperatureRange> ranges)
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            dgv.Columns.Add("RangeNum", "№ диапазона");
            dgv.Columns.Add("UpperBound", "Верхняя граница (°C)");
            dgv.Columns.Add("LowerBound", "Нижняя граница (°C)");
            dgv.Columns.Add("Coefficient", "Коэффициент влияния");

            if (ranges != null && ranges.Count > 0)
            {
                for (int i = 0; i < ranges.Count; i++)
                {
                    dgv.Rows.Add(
                        i + 1,
                        ranges[i].To.ToString("F1"),
                        ranges[i].From.ToString("F1"),
                        ranges[i].Coefficient.ToString("F3")
                    );
                }
            }

            // Настройка автоматического заполнения пространства
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;

            // Настройка высоты строк для лучшего отображения
            dgv.RowTemplate.Height = 25;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private Label CreateBoldLabel(string text)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Dock = DockStyle.Fill
            };
        }

        private Label CreateValueLabel(string text, Color? color = null, float? fontSize = null)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", fontSize ?? 9, FontStyle.Regular),
                ForeColor = color ?? Color.Black,
                Dock = DockStyle.Fill
            };
        }
    }
}