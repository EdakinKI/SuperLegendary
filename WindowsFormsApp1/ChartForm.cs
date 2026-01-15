using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ClassLibrary1;

namespace WindowsFormsApp1
{
    public partial class ChartForm : Form
    {
        private List<DataPeriod> dataPeriods;
        private bool isFourierProcessed;
        private List<RegressionResult> regressionResults;

        public ChartForm(List<DataPeriod> periods, bool fourierProcessed = true)
        {
            dataPeriods = periods ?? new List<DataPeriod>();
            isFourierProcessed = fourierProcessed;
            regressionResults = new List<RegressionResult>();

            InitializeComponent();
            ConfigureForm();
            CreateChart();
            UpdateStats();
        }

        private void ConfigureForm()
        {
            string title = "График зависимости мощности от температуры";
            if (dataPeriods.Count > 1)
            {
                title += $"\n{dataPeriods.Count} периода(ов)";
            }

            this.Text = title;
            lblTitle.Text = title;
        }

        private void CreateChart()
        {
            try
            {
                chartDependency.Series.Clear();
                chartDependency.ChartAreas.Clear();

                if (dataPeriods == null || dataPeriods.Count == 0)
                {
                    MessageBox.Show("Нет данных для построения графиков", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ChartArea chartArea = new ChartArea("MainArea");
                chartArea.AxisX.Title = "Температура, °C";
                chartArea.AxisY.Title = "Мощность, МВт";
                chartArea.AxisX.IsStartedFromZero = false;
                chartArea.AxisY.IsStartedFromZero = false;
                chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
                chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;

                chartDependency.ChartAreas.Add(chartArea);

                Color[] periodColors = new Color[]
                {
                    Color.Red,
                    Color.Blue,
                    Color.Green,
                    Color.Purple,
                    Color.Orange,
                    Color.Brown,
                    Color.DarkCyan,
                    Color.Magenta,
                    Color.DarkGreen,
                    Color.DarkRed
                };

                chartDependency.Legends.Clear();
                Legend legend = new Legend
                {
                    Name = "MainLegend",
                    Title = "Осенне-зимние периоды",
                    Docking = Docking.Top,
                    Alignment = StringAlignment.Center,
                    TitleAlignment = StringAlignment.Center,
                    TitleFont = new Font("Arial", 10, FontStyle.Bold),
                    Font = new Font("Arial", 9),
                    BorderColor = Color.Gray,
                    BorderWidth = 1,
                    BorderDashStyle = ChartDashStyle.Solid
                };
                chartDependency.Legends.Add(legend);

                int periodIndex = 0;
                int totalPoints = 0;

                foreach (var period in dataPeriods)
                {
                    var validData = period.Data.Where(d => d.PowerValue.HasValue).ToList();

                    if (validData.Count < 4)
                        continue;

                    Color seriesColor = periodColors[periodIndex % periodColors.Length];
                    string seriesName = period.Name;

                    Series dataSeries = new Series(seriesName)
                    {
                        ChartType = SeriesChartType.Point,
                        Color = seriesColor,
                        MarkerStyle = MarkerStyle.Circle,
                        MarkerSize = 4,
                        BorderWidth = 0,
                        IsVisibleInLegend = true,
                        LegendText = seriesName
                    };

                    int maxPoints = 1000;
                    int step = Math.Max(1, validData.Count / maxPoints);

                    for (int i = 0; i < validData.Count; i += step)
                    {
                        var data = validData[i];
                        dataSeries.Points.AddXY(data.Value, data.PowerValue.Value);
                        totalPoints++;

                        if (dataSeries.Points.Count >= maxPoints)
                            break;
                    }

                    if (dataSeries.Points.Count > 0)
                    {
                        chartDependency.Series.Add(dataSeries);
                        periodIndex++;
                    }
                }

                if (periodIndex == 0)
                {
                    MessageBox.Show("Нет достаточного количества данных для построения графиков",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                UpdateStats();
                lblInfo.Text = $"Построено {periodIndex} периодов, {totalPoints} точек";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении графика:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApproximate_Click(object sender, EventArgs e)
        {
            try
            {
                lblInfo.Text = "Выполняется аппроксимация...";
                btnApproximate.Enabled = false;
                Application.DoEvents();

                regressionResults.Clear();

                foreach (var period in dataPeriods)
                {
                    var validData = period.Data.Where(d => d.PowerValue.HasValue).ToList();

                    if (validData.Count < 10)
                        continue;

                    var temperatures = validData.Select(d => d.Value).ToList();
                    var powers = validData.Select(d => d.PowerValue.Value).ToList();

                    var result = RegressionCalculator.CalculateRegressions(period.Name, temperatures, powers);

                    if (result.Temperatures.Count > 0)
                    {
                        regressionResults.Add(result);
                        System.Diagnostics.Debug.WriteLine($"Рассчитана регрессия для {period.Name}:");
                        System.Diagnostics.Debug.WriteLine($"  k={result.LinearSlope:F4}, b={result.LinearIntercept:F2}");
                        System.Diagnostics.Debug.WriteLine($"  L={result.ExponentialIntensity:F4}, T_cross={result.CrossoverPoint:F2}°C");
                        System.Diagnostics.Debug.WriteLine($"  RMSE линейной: {result.LinearRMSE:F2}, RMSE экспоненц.: {result.ExponentialRMSE:F2}");
                    }
                }

                if (regressionResults.Count > 0)
                {
                    btnShowRegression.Enabled = true;
                    lblInfo.Text = $"Аппроксимация завершена!\n" +
                                  $"Рассчитано {regressionResults.Count} регрессионных моделей.\n" +
                                  $"Нажмите 'Показать регрессии' для визуализации.";
                }
                else
                {
                    lblInfo.Text = "Не удалось рассчитать регрессии.\n" +
                                  "Проверьте, что данные содержат достаточное количество точек.";
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text = $"Ошибка при аппроксимации: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка BtnApproximate_Click: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                btnApproximate.Enabled = true;
            }
        }

        private void BtnShowRegression_Click(object sender, EventArgs e)
        {
            try
            {
                if (regressionResults == null || regressionResults.Count == 0)
                {
                    MessageBox.Show("Сначала выполните аппроксимацию данных", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Используем новую форму с сохранением в БД
                var regressionForm = new RegressionFormDB(regressionResults);
                regressionForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна регрессий:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnShowFormulas_Click(object sender, EventArgs e)
        {
            string formulas = @"ФОРМУЛЫ ЛИНЕЙНОЙ РЕГРЕССИИ:

k = [1+(m-n)]·Σ(P·T) - ΣP·ΣT
    -----------------------------------
    [1+(m-n)]·Σ(T²) - (ΣT)²

b = ΣP - ΣT
    -----------------------------------
    [1+(m-n)]·Σ(T²) - (ΣT)²

где:
• Σ = Σ_{i=n}^{m} - сумма от i=n до i=m
• n - первый индекс линейной регрессии
• m - последний индекс линейной регрессии
• P_i - мощность в точке i
• T_i - температура в точке i

ФОРМУЛА ЭКСПОНЕНЦИАЛЬНОЙ РЕГРЕССИИ:

P(T) = f(T₀) - k/L + (k/L)·exp[L·(T - T₀)]

где:
• f(T₀) = k·T₀ + b
• L оптимизируется методом Ньютона

RMSE (среднеквадратичное отклонение):
RMSE = √[ Σ(ŷ_i - y_i)² / N ]";

            MessageBox.Show(formulas, "Формулы регрессии",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateStats()
        {
            try
            {
                if (dataPeriods == null)
                {
                    lblStats.Text = "Нет данных для построения графика";
                    return;
                }

                var validData = dataPeriods
                    .SelectMany(p => p.Data?.Where(d => d.PowerValue.HasValue) ?? new List<HourlyData>())
                    .ToList();

                if (validData.Count > 0)
                {
                    double minTemp = validData.Min(d => d.Value);
                    double maxTemp = validData.Max(d => d.Value);
                    double minPower = validData.Min(d => d.PowerValue.Value);
                    double maxPower = validData.Max(d => d.PowerValue.Value);
                    double avgTemp = validData.Average(d => d.Value);
                    double avgPower = validData.Average(d => d.PowerValue.Value);

                    string fourierInfo = isFourierProcessed
                        ? "Данные обработаны преобразованием Фурье\n(оставлена только основная гармоника)\n"
                        : "";

                    lblStats.Text = $"{fourierInfo}" +
                                   $"Периодов: {dataPeriods.Count}\n" +
                                   $"Точек на графике: {validData.Count}\n" +
                                   $"Температура: {minTemp:F1}...{maxTemp:F1}°C (среднее: {avgTemp:F1}°C)\n" +
                                   $"Мощность: {minPower:F0}...{maxPower:F0} МВт (среднее: {avgPower:F0} МВт)";
                }
                else
                {
                    lblStats.Text = "Нет данных для построения графика";
                }
            }
            catch (Exception ex)
            {
                lblStats.Text = $"Ошибка при обновлении статистики: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка UpdateStats: {ex.Message}");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}