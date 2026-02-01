using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WindowsFormsApp1.Services;
using static WindowsFormsApp1.Services.DatabaseService;

namespace WindowsFormsApp1
{
    public partial class RegressionFormDB : Form
    {
        private List<RegressionResult> regressionResults;
        private DatabaseService _dbService = new DatabaseService();
        private Button btnSaveToDb;

        public RegressionFormDB(List<RegressionResult> results)
        {
            InitializeComponent();

            regressionResults = results ?? new List<RegressionResult>();

            if (regressionResults.Count > 0)
            {
                CreateRegressionChart();
                CreateCoefficientsTable();
                CreateFormulasText();
                AddSaveButton();
            }
            else
            {
                MessageBox.Show("Нет данных для отображения", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddSaveButton()
        {
            // Создаем панель для кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = SystemColors.Control
            };

            // Кнопка сохранения
            btnSaveToDb = new Button
            {
                Text = "Сохранить в БД",
                Size = new Size(150, 35),
                Location = new Point(this.Width - 320, 10),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            btnSaveToDb.Click += BtnSaveToDb_Click;

            // Кнопка закрытия
            var btnClose = new Button
            {
                Text = "Закрыть",
                Size = new Size(150, 35),
                Location = new Point(this.Width - 160, 10)
            };
            btnClose.Click += (s, e) => this.Close();

            buttonPanel.Controls.Add(btnSaveToDb);
            buttonPanel.Controls.Add(btnClose);
            this.Controls.Add(buttonPanel);
        }

        private void BtnSaveToDb_Click(object sender, EventArgs e)
        {
            var dialogResult = MessageBox.Show(
                "Сохранить результаты статического анализа в базу данных?\n\n" +
                "Будут сохранены коэффициенты линейной и экспоненциальной регрессии для всех периодов.",
                "Подтверждение сохранения",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int savedCount = 0;
                int totalCount = regressionResults.Count;

                // Показываем прогресс
                var progressForm = new ProcessingFormWithCancel();
                progressForm.Show();
                progressForm.UpdateProgress("Начинаю сохранение...", 0);

                // Получаем ID типа расчета
                int typeId = _dbService.GetOrCreateCalculationType("Расчет статических зависимостей");

                var staticCalc = new DbStaticCalculation
                {
                    TypeId = typeId,
                    CalculationName = $"Статический анализ {DateTime.Now:dd.MM.yyyy HH:mm}",
                    CalculationDate = DateTime.Now
                };

                // Сохраняем ВСЕ регрессии в одной записи
                int staticId = _dbService.SaveStaticCalculation(staticCalc, regressionResults);

                if (staticId > 0)
                {
                    MessageBox.Show(
                        $"Успешно сохранен статический анализ с {regressionResults.Count} периодами регрессии.\n" +
                        $"ID записи: {staticId}\n\n" +
                        "Все регрессии сохранены как часть одного расчета.",
                        "Успешно",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // После успешного сохранения обновляем кнопку
                    if (btnSaveToDb != null)
                    {
                        btnSaveToDb.Enabled = false;
                        btnSaveToDb.BackColor = Color.LightGray;
                        btnSaveToDb.Text = "✓ Уже сохранено";
                        btnSaveToDb.Font = new Font(btnSaveToDb.Font, FontStyle.Bold);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Не удалось сохранить результаты.\nПроверьте подключение к базе данных.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                progressForm.UpdateProgress("Сохранение завершено!", 100);
                System.Threading.Thread.Sleep(500);
                progressForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка при сохранении в БД:\n\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateRegressionChart()
        {
            try
            {
                chartRegression.Series.Clear();
                chartRegression.ChartAreas.Clear();
                chartRegression.Titles.Clear();
                chartRegression.Legends.Clear();

                if (regressionResults.Count == 0)
                    return;

                ChartArea chartArea = new ChartArea("RegressionArea");
                chartArea.AxisX.Title = "Температура, °C";
                chartArea.AxisY.Title = "Мощность, МВт";
                chartArea.AxisX.IsStartedFromZero = false;
                chartArea.AxisY.IsStartedFromZero = false;
                chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
                chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;

                chartRegression.ChartAreas.Add(chartArea);

                Legend legend = new Legend
                {
                    Docking = Docking.Top,
                    Title = "Периоды и типы регрессий",
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    IsTextAutoFit = true
                };
                chartRegression.Legends.Add(legend);

                Color[] colors = new Color[]
                {
                    Color.Red, Color.Blue, Color.Green, Color.Purple,
                    Color.Orange, Color.Brown, Color.DarkCyan, Color.Magenta,
                    Color.DarkOrange, Color.DarkBlue
                };

                for (int i = 0; i < regressionResults.Count; i++)
                {
                    var result = regressionResults[i];
                    Color periodColor = colors[i % colors.Length];

                    // 1. Исходные данные (точки)
                    Series dataSeries = new Series($"{result.PeriodName} - данные")
                    {
                        ChartType = SeriesChartType.Point,
                        Color = Color.FromArgb(50, periodColor), // Полупрозрачные точки
                        MarkerStyle = MarkerStyle.Circle,
                        MarkerSize = 4,
                        BorderWidth = 0,
                        IsVisibleInLegend = true,
                        LegendText = $"{result.PeriodName} - данные"
                    };

                    // 2. Объединенная регрессия
                    Series combinedRegressionSeries = new Series($"{result.PeriodName} - регрессия")
                    {
                        ChartType = SeriesChartType.Line,
                        Color = periodColor,
                        BorderWidth = 3,
                        IsVisibleInLegend = true,
                        LegendText = $"{result.PeriodName} - регрессия"
                    };

                    // 3. Точка перехода
                    Series crossoverSeries = new Series($"{result.PeriodName} - точка перехода")
                    {
                        ChartType = SeriesChartType.Point,
                        Color = Color.Black,
                        MarkerStyle = MarkerStyle.Diamond,
                        MarkerSize = 10,
                        BorderWidth = 2,
                        IsVisibleInLegend = false
                    };

                    // Добавляем исходные данные (каждую 10-ю точку для производительности)
                    int step = Math.Max(1, result.Temperatures.Count / 500);
                    for (int j = 0; j < result.Temperatures.Count; j += step)
                    {
                        double temp = result.Temperatures[j];
                        dataSeries.Points.AddXY(temp, result.Powers[j]);
                    }

                    // Добавляем объединенную регрессию
                    // Для гладкой кривой берем больше точек
                    int regressionPoints = 200;
                    double minTemp = result.Temperatures.Min();
                    double maxTemp = result.Temperatures.Max();

                    for (int j = 0; j <= regressionPoints; j++)
                    {
                        double temp = minTemp + (maxTemp - minTemp) * j / regressionPoints;
                        double regressionValue;

                        if (temp > result.CrossoverPoint)
                        {
                            // ЛИНЕЙНАЯ часть (выше точки перехода)
                            regressionValue = result.LinearSlope * temp + result.LinearIntercept;
                        }
                        else
                        {
                            // ЭКСПОНЕНЦИАЛЬНАЯ часть (ниже или равно точке перехода)
                            double expTerm = Math.Exp(result.ExponentialIntensity * (temp - result.TransitionTemperature));
                            regressionValue = result.TransitionPower - (result.LinearSlope / result.ExponentialIntensity)
                                            + (result.LinearSlope / result.ExponentialIntensity) * expTerm;
                        }

                        combinedRegressionSeries.Points.AddXY(temp, regressionValue);
                    }

                    // Добавляем точку перехода
                    crossoverSeries.Points.AddXY(result.CrossoverPoint,
                        result.LinearSlope * result.CrossoverPoint + result.LinearIntercept);

                    chartRegression.Series.Add(dataSeries);
                    chartRegression.Series.Add(combinedRegressionSeries);
                    chartRegression.Series.Add(crossoverSeries);
                }

                Title title = new Title
                {
                    Text = "Аппроксимация данных: линейная и экспоненциальная регрессии",
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    ForeColor = Color.DarkBlue,
                    Alignment = ContentAlignment.TopCenter
                };
                chartRegression.Titles.Add(title);

                // Настраиваем масштабирование
                chartArea.CursorX.IsUserEnabled = true;
                chartArea.CursorX.IsUserSelectionEnabled = true;
                chartArea.AxisX.ScaleView.Zoomable = true;
                chartArea.AxisY.ScaleView.Zoomable = true;
                chartArea.CursorY.IsUserEnabled = true;
                chartArea.CursorY.IsUserSelectionEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании графика:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateCoefficientsTable()
        {
            try
            {
                dgvCoefficients.Columns.Clear();
                dgvCoefficients.Rows.Clear();
                dgvCoefficients.AutoGenerateColumns = false;

                // Настраиваем столбцы
                DataGridViewTextBoxColumn[] columns = {
                    new DataGridViewTextBoxColumn { Name = "Period", HeaderText = "Период", Width = 150 },
                    new DataGridViewTextBoxColumn { Name = "Points", HeaderText = "Точек", Width = 80 },
                    new DataGridViewTextBoxColumn { Name = "k", HeaderText = "k (наклон)", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "b", HeaderText = "b (intercept)", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "L", HeaderText = "L (интенсивность)", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "T_cross", HeaderText = "T перехода (°C)", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "RMSE_lin", HeaderText = "RMSE линейной", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "RMSE_exp", HeaderText = "RMSE экспоненц.", Width = 120 }
                };

                dgvCoefficients.Columns.AddRange(columns);

                // Заполняем данными
                foreach (var result in regressionResults)
                {
                    dgvCoefficients.Rows.Add(
                        result.PeriodName,
                        result.Temperatures.Count,
                        $"{result.LinearSlope:F4}",
                        $"{result.LinearIntercept:F2}",
                        $"{result.ExponentialIntensity:F4}",
                        $"{result.CrossoverPoint:F2}",
                        $"{result.LinearRMSE:F2}",
                        $"{result.ExponentialRMSE:F2}"
                    );
                }

                // Настраиваем внешний вид
                dgvCoefficients.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvCoefficients.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvCoefficients.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
                dgvCoefficients.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvCoefficients.AllowUserToResizeRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании таблицы коэффициентов:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateFormulasText()
        {
            try
            {
                string formulasText = @"ФОРМУЛЫ РЕГРЕССИОННОГО АНАЛИЗА

1. ЛИНЕЙНАЯ РЕГРЕССИЯ (для средних и высоких температур):
   P = k·T + b
   
   где:
   • P - мощность, МВт
   • T - температура, °C
   • k - коэффициент наклона (МВт/°C)
   • b - свободный член (МВт)

2. ЭКСПОНЕНЦИАЛЬНАЯ РЕГРЕССИЯ (для низких температур):
   P = f(T₀) - (k/L) + (k/L)·exp[L·(T - T₀)]
   
   где:
   • f(T₀) = k·T₀ + b - мощность в точке перехода
   • L - интенсивность затухания (1/°C)
   • T₀ - точка перехода между регрессиями (°C)

3. ТОЧКА ПЕРЕХОДА (Crossover Point):
   Температура, при которой происходит переход от экспоненциальной 
   к линейной зависимости. Определяется аналитически по данным.

4. RMSE (СРЕДНЕКВАДРАТИЧНОЕ ОТКЛОНЕНИЕ):
   RMSE = √[ Σ(ŷ_i - y_i)² / N ]
   
   где:
   • ŷ_i - предсказанное значение
   • y_i - фактическое значение
   • N - количество точек

ИНТЕРПРЕТАЦИЯ ПАРАМЕТРОВ:

• k < 0: мощность уменьшается при росте температуры
• |k| большое: сильная зависимость мощности от температуры
• L > 0: экспоненциальный рост/спад мощности при изменении температуры
• RMSE: чем меньше, тем точнее модель

ТИПИЧНЫЕ ЗНАЧЕНИЯ ДЛЯ ЭНЕРГОСИСТЕМ:

• k: от -5 до -30 МВт/°C
• L: от 0.05 до 0.3 1/°C
• T₀ (точка перехода): от -15°C до +5°C
• RMSE: 
   - < 50 МВт - отличная модель
   - 50-100 МВт - хорошая модель
   - > 100 МВт - требуется улучшение

МЕТОДИКА РАСЧЕТА:

1. Данные сортируются по температуре
2. Выполняется линейная регрессия для всех данных
3. Определяется точка перехода аналитическим методом
4. Для температур ниже точки перехода строится экспоненциальная регрессия
5. Параметр L оптимизируется методом Ньютона
6. Рассчитываются ошибки RMSE для обеих моделей";

                rtbFormulas.Text = formulasText;
                rtbFormulas.Font = new Font("Arial", 10);
                rtbFormulas.ForeColor = Color.DarkBlue;
                rtbFormulas.BackColor = Color.AliceBlue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании текста формул:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}