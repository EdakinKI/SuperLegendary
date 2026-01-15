using ClassLibrary1;
using ExcelDataReader;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class InitialFormStatic : Form
    {
        private List<HourlyData> powerData;
        private List<HourlyData> allTemperatureData; // Все интерполированные температуры
        private List<HourlyData> matchedData; // Сопоставленные данные
        
        // Добавьте эти переменные в класс
        private double _cutoffRatio = 0.1; // По умолчанию 10%
        private string _fourierPeriodType = "Осенне-зимние периоды (Сен-Май)";

        public InitialFormStatic()
        {
            InitializeComponent();

            // ИНИЦИАЛИЗИРУЕМ списки
            powerData = new List<HourlyData>();
            allTemperatureData = new List<HourlyData>();
            matchedData = new List<HourlyData>();

            ConfigureForm();
            InitializeEventHandlers();
            UpdateUIState(); // Новый метод для управления состоянием UI
        }

        // Метод для обработки изменения cutoff ratio
        private void NumCutoffRatio_ValueChanged(object sender, EventArgs e)
        {
            _cutoffRatio = (double)numCutoffRatio.Value / 100.0; // Конвертируем проценты в десятичную дробь
            UpdateStatus($"Cutoff ratio: {_cutoffRatio:P0}", Color.Blue);
        }

        // Метод для обработки выбора типа периода Фурье
        private void CmbFourierPeriodType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFourierPeriodType.SelectedItem != null)
            {
                _fourierPeriodType = cmbFourierPeriodType.SelectedItem.ToString(); // ← Используйте _fourierPeriodType
                UpdateStatus($"Выбран период Фурье: {_fourierPeriodType}", Color.Blue);
            }
        }

        /// <summary>
        /// Применяет Фурье-фильтрацию к данным с выбранным типом периода
        /// </summary>
        private List<HourlyData> ApplyFourierFilterByPeriod(
            List<HourlyData> data,
            bool isPowerData,
            ProcessingFormWithCancel progressForm,
            CancellationTokenSource cts)
        {
            var result = new List<HourlyData>();

            try
            {
                if (data == null || data.Count == 0)
                    return data;

                // Разделяем данные на периоды в зависимости от выбранного типа
                var periods = SplitDataIntoFourierPeriods(data, _fourierPeriodType);

                progressForm.UpdateProgress($"Фурье-фильтрация: найдено {periods.Count} периодов", 30);

                // Обрабатываем каждый период отдельно
                for (int i = 0; i < periods.Count; i++)
                {
                    // Проверяем отмену
                    if (cts.Token.IsCancellationRequested)
                        return null;

                    var period = periods[i];
                    int progress = 30 + (i * 40 / Math.Max(1, periods.Count));

                    progressForm.UpdateProgress($"Фурье-фильтрация периода {i + 1} из {periods.Count}...", progress);

                    // Применяем Фурье-фильтр к периоду
                    List<HourlyData> filteredPeriodData;

                    if (isPowerData)
                    {
                        filteredPeriodData = ClassLibrary1.FourierProcessor.ProcessPowerDataWithProgress(
                            period.Data, _cutoffRatio, progressForm, cts);
                    }
                    else
                    {
                        filteredPeriodData = ClassLibrary1.FourierProcessor.ProcessTemperatureDataWithProgress(
                            period.Data, _cutoffRatio, progressForm, cts);
                    }

                    // Проверяем отмену
                    if (cts.Token.IsCancellationRequested || filteredPeriodData == null)
                        return null;

                    // Добавляем отфильтрованные данные в результат
                    result.AddRange(filteredPeriodData);
                }

                // Сортируем по дате
                result = result.OrderBy(d => d.DateTime).ToList();

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка Фурье-фильтрации: {ex.Message}");
                return data; // Возвращаем оригинальные данные в случае ошибки
            }
        }

        /// <summary>
        /// Разделяет данные на периоды для Фурье-преобразования
        /// </summary>
        private List<DataPeriod> SplitDataIntoFourierPeriods(List<HourlyData> allData, string periodType)
        {
            switch (periodType)
            {
                case "Еженедельные периоды":
                    return SplitIntoWeeklyFourierPeriods(allData);
                case "Ежемесячные периоды":
                    return SplitIntoMonthlyFourierPeriods(allData);
                case "Осенне-зимние периоды (Сен-Май)":
                default:
                    return SplitIntoAutumnWinterFourierPeriods(allData);
            }
        }

        private void ConfigureForm()
        {
            this.Text = "Статический анализ зависимостей";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(1000, 700);

            // Установите значения по умолчанию для новых контролов
            numCutoffRatio.Value = 10; // 10%
            cmbFourierPeriodType.SelectedIndex = 0; // Осенне-зимние периоды

            // Кнопка для отладки
            var btnDebug = new Button
            {
                Text = "Отладка",
                Location = new Point(650, 20),
                Size = new Size(100, 30)
            };
            btnDebug.Click += (s, ev) => DebugData();
            this.Controls.Add(btnDebug);
            ;
        }

        private void DebugData()
        {
            if (matchedData != null)
            {
                System.Diagnostics.Debug.WriteLine("=== ОТЛАДКА ДАННЫХ ===");
                System.Diagnostics.Debug.WriteLine($"Всего точек: {matchedData.Count}");

                if (matchedData.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Диапазон: {matchedData.Min(d => d.DateTime):dd.MM.yyyy HH:mm} - {matchedData.Max(d => d.DateTime):dd.MM.yyyy HH:mm}");

                    var years = matchedData.Select(d => d.DateTime.Year).Distinct().OrderBy(y => y);
                    System.Diagnostics.Debug.WriteLine($"Года: {string.Join(", ", years)}");

                    foreach (var year in years)
                    {
                        var yearData = matchedData.Where(d => d.DateTime.Year == year).ToList();
                        var months = yearData.Select(d => d.DateTime.Month).Distinct().OrderBy(m => m);
                        System.Diagnostics.Debug.WriteLine($"  {year}: {yearData.Count} точек, месяцы: {string.Join(", ", months)}");
                    }

                    int withPower = matchedData.Count(d => d.PowerValue.HasValue);
                    int withoutPower = matchedData.Count(d => !d.PowerValue.HasValue);
                    System.Diagnostics.Debug.WriteLine($"С мощностью: {withPower}, без мощности: {withoutPower}");

                    // Покажем первые 5 записей
                    System.Diagnostics.Debug.WriteLine("Первые 5 записей:");
                    for (int i = 0; i < Math.Min(5, matchedData.Count); i++)
                    {
                        var d = matchedData[i];
                        System.Diagnostics.Debug.WriteLine($"  {d.DateTime:dd.MM.yyyy HH:mm}: T={d.Value:F1}°C, P={d.PowerValue?.ToString("F2") ?? "N/A"} МВт");
                    }
                }

                System.Diagnostics.Debug.WriteLine("=== КОНЕЦ ОТЛАДКИ ===");

                MessageBox.Show($"Данные загружены: {matchedData.Count} точек\n" +
                               $"С мощностью: {matchedData.Count(d => d.PowerValue.HasValue)}\n" +
                               $"Года: {string.Join(", ", matchedData.Select(d => d.DateTime.Year).Distinct().OrderBy(y => y))}",
                               "Отладка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("matchedData is null!", "Отладка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Разделяет данные на еженедельные периоды для Фурье
        /// </summary>
        private List<DataPeriod> SplitIntoWeeklyFourierPeriods(List<HourlyData> allData)
        {
            var periods = new List<DataPeriod>();

            if (allData == null || allData.Count == 0)
                return periods;

            // Сортируем по дате
            var sortedData = allData.OrderBy(d => d.DateTime).ToList();

            // Группируем по неделям (ISO 8601)
            var weeklyGroups = sortedData
                .GroupBy(d =>
                {
                    var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
                    return calendar.GetWeekOfYear(
                        d.DateTime,
                        System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday);
                })
                .OrderBy(g => g.Min(d => d.DateTime))
                .ToList();

            int weekNumber = 1;
            foreach (var weekGroup in weeklyGroups)
            {
                var weekData = weekGroup.OrderBy(d => d.DateTime).ToList();

                if (weekData.Count >= 24) // Минимум 1 день данных
                {
                    DateTime weekStart = weekData.Min(d => d.DateTime);
                    DateTime weekEnd = weekData.Max(d => d.DateTime);

                    periods.Add(new DataPeriod
                    {
                        Name = $"Неделя {weekNumber}",
                        StartYear = weekStart.Year,
                        EndYear = weekEnd.Year,
                        PeriodStart = weekStart,
                        PeriodEnd = weekEnd,
                        Data = weekData
                    });

                    weekNumber++;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Создано {periods.Count} недельных периодов для Фурье");
            return periods;
        }

        /// <summary>
        /// Разделяет данные на ежемесячные периоды для Фурье
        /// </summary>
        private List<DataPeriod> SplitIntoMonthlyFourierPeriods(List<HourlyData> allData)
        {
            var periods = new List<DataPeriod>();

            if (allData == null || allData.Count == 0)
                return periods;

            // Сортируем по дате
            var sortedData = allData.OrderBy(d => d.DateTime).ToList();

            // Группируем по году и месяцу
            var monthlyGroups = sortedData
                .GroupBy(d => new { d.DateTime.Year, d.DateTime.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToList();

            foreach (var monthGroup in monthlyGroups)
            {
                var monthData = monthGroup.OrderBy(d => d.DateTime).ToList();

                if (monthData.Count >= 24) // Минимум 1 день данных
                {
                    DateTime monthStart = monthData.Min(d => d.DateTime);
                    DateTime monthEnd = monthData.Max(d => d.DateTime);

                    string monthName = GetMonthName(monthStart.Month);

                    periods.Add(new DataPeriod
                    {
                        Name = $"{monthName} {monthStart.Year}",
                        StartYear = monthStart.Year,
                        EndYear = monthStart.Year,
                        PeriodStart = monthStart,
                        PeriodEnd = monthEnd,
                        Data = monthData
                    });
                }
            }

            System.Diagnostics.Debug.WriteLine($"Создано {periods.Count} месячных периодов для Фурье");
            return periods;
        }

        /// <summary>
        /// Получает название месяца
        /// </summary>
        private string GetMonthName(int month)
        {
            switch (month)
            {
                case 1: return "Январь";
                case 2: return "Февраль";
                case 3: return "Март";
                case 4: return "Апрель";
                case 5: return "Май";
                case 6: return "Июнь";
                case 7: return "Июль";
                case 8: return "Август";
                case 9: return "Сентябрь";
                case 10: return "Октябрь";
                case 11: return "Ноябрь";
                case 12: return "Декабрь";
                default: return $"Месяц {month}";
            }
        }

        private void UpdateUIState()
        {
            bool hasPowerData = powerData != null && powerData.Count > 0;
            bool hasTempData = allTemperatureData != null && allTemperatureData.Count > 0;

            // Шаг 1: Разрешаем загрузку температуры только после загрузки мощности
            lblTemperatureFile.Enabled = hasPowerData;
            txtTemperatureFile.Enabled = hasPowerData;
            btnBrowseTemperature.Enabled = hasPowerData;

            // Шаг 2: Разрешаем настройки Фурье только после загрузки обоих файлов
            lblCutoffRatio.Enabled = hasPowerData && hasTempData;
            numCutoffRatio.Enabled = hasPowerData && hasTempData;
            lblFourierPeriodType.Enabled = hasPowerData && hasTempData;
            cmbFourierPeriodType.Enabled = hasPowerData && hasTempData;

            // Шаг 3: Разрешаем построение графика только после загрузки обоих файлов
            btnAnalyze.Enabled = hasPowerData && hasTempData && matchedData != null && matchedData.Count > 0;

            // Обновляем статус
            if (!hasPowerData)
            {
                lblStatus.Text = "Статус: Загрузите файл мощности";
                lblPowerPreview.Text = "Сначала загрузите файл мощности";
            }
            else if (!hasTempData)
            {
                lblStatus.Text = $"Загружено {powerData.Count} записей мощности. Теперь загрузите файл температуры";
                lblPowerPreview.Text = "Теперь загрузите файл температуры";
            }
            else if (matchedData != null && matchedData.Count > 0)
            {
                lblStatus.Text = $"Сопоставлено {matchedData.Count} записей. Можно строить график";
                lblPowerPreview.Text = $"Сопоставленные данные:";

                // Показываем текущие настройки Фурье
                lblStatus.Text += $"\nНастройки Фурье: Cutoff Ratio = {_cutoffRatio:P0}, Период = {_fourierPeriodType}";
            }
            else if (matchedData != null && matchedData.Count > 0)
            {
                lblStatus.Text = $"Сопоставлено {matchedData.Count} записей. Можно строить график";
                lblPowerPreview.Text = $"Сопоставленные данные:";

                // Показываем текущие настройки Фурье
                lblStatus.Text += $"\nНастройки Фурье: Cutoff Ratio = {_cutoffRatio:P0}, Период = {_fourierPeriodType}"; // ← Используйте _fourierPeriodType
            }
        }

        private void InitializeEventHandlers()
        {
            btnBrowsePower.Click += BtnBrowsePower_Click;
            btnBrowseTemperature.Click += BtnBrowseTemperature_Click;
            btnAnalyze.Click += BtnAnalyze_Click;
        }

        private void BtnBrowsePower_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtPowerFile.Text = openFileDialog.FileName;
                    LoadPowerData(openFileDialog.FileName);
                }
            }
        }

        private void LoadPowerData(string filePath)
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
                        }
                    });

                    if (result.Tables.Count > 0)
                    {
                        var dataTable = result.Tables[0];
                        powerData = ParsePowerData(dataTable);

                        if (powerData.Count > 0)
                        {
                            UpdateStatus($"Загружено {powerData.Count} записей по мощности", Color.Green);

                            // Сбрасываем предыдущие данные температуры
                            allTemperatureData = new List<HourlyData>();
                            matchedData = new List<HourlyData>();
                            txtTemperatureFile.Text = "";

                            UpdateUIState();
                            DisplayDataPreview();
                        }
                        else
                        {
                            UpdateStatus("Не удалось загрузить данные по мощности", Color.Red);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла мощности:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<HourlyData> ParsePowerData(DataTable data)
        {
            var powerList = new List<HourlyData>();

            try
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    var row = data.Rows[i];

                    if (row.ItemArray.Length < 2) continue;

                    string dateStr = row[0]?.ToString();
                    string powerStr = row[1]?.ToString();

                    if (string.IsNullOrWhiteSpace(dateStr) || string.IsNullOrWhiteSpace(powerStr))
                        continue;

                    // Пробуем разные форматы даты
                    DateTime dateTime;
                    if (!DateTime.TryParse(dateStr, out dateTime))
                    {
                        string[] formats = {
                            "dd.MM.yy HH:mm:ss",
                            "dd.MM.yyyy HH:mm:ss",
                            "dd.MM.yy H:mm:ss",
                            "dd.MM.yyyy H:mm:ss",
                            "yyyy-MM-dd HH:mm:ss",
                            "dd/MM/yyyy HH:mm:ss"
                        };

                        if (!DateTime.TryParseExact(dateStr.Trim(), formats, null,
                            System.Globalization.DateTimeStyles.None, out dateTime))
                            continue;
                    }

                    powerStr = powerStr.Replace(',', '.');
                    if (!double.TryParse(powerStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double powerValue))
                        continue;

                    powerList.Add(new HourlyData
                    {
                        DateTime = dateTime,
                        Hour = dateTime.Hour,
                        Value = Math.Round(powerValue, 2)
                    });
                }

                // Сортируем по дате
                powerList = powerList.OrderBy(p => p.DateTime).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при парсинге данных мощности:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return powerList;
        }

        private void BtnBrowseTemperature_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtTemperatureFile.Text = openFileDialog.FileName;
                    LoadTemperatureData(openFileDialog.FileName);
                }
            }
        }

        private void LoadTemperatureData(string filePath)
        {
            CancellationTokenSource cts = null;
            ProcessingFormWithCancel progressForm = null;

            try
            {
                // Создаем форму прогресса
                cts = new CancellationTokenSource();
                progressForm = new ProcessingFormWithCancel();
                progressForm.Show();
                Application.DoEvents();

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                progressForm.UpdateProgress("Чтение файла температуры...", 10);

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = false
                        }
                    });

                    if (result.Tables.Count > 0)
                    {
                        progressForm.UpdateProgress("Парсинг данных температуры...", 30);

                        if (cts.Token.IsCancellationRequested)
                        {
                            progressForm.SetProcessing(false);
                            return;
                        }

                        allTemperatureData = new List<HourlyData>();

                        // Обрабатываем все листы
                        for (int sheetIndex = 0; sheetIndex < result.Tables.Count; sheetIndex++)
                        {
                            if (cts.Token.IsCancellationRequested)
                            {
                                progressForm.SetProcessing(false);
                                return;
                            }

                            int progress = 30 + (sheetIndex * 20) / Math.Max(1, result.Tables.Count);
                            progressForm.UpdateProgress($"Обработка листа {sheetIndex + 1} из {result.Tables.Count}...", progress);

                            DataTable sheet = result.Tables[sheetIndex];
                            var sheetTemperatures = ParseTemperatureDataWithProgress(sheet, progressForm, cts);

                            if (sheetTemperatures != null && sheetTemperatures.Count > 0)
                            {
                                allTemperatureData.AddRange(sheetTemperatures);
                            }
                        }

                        if (allTemperatureData.Count > 0)
                        {
                            progressForm.UpdateProgress("Сортировка данных...", 60);

                            if (cts.Token.IsCancellationRequested)
                            {
                                progressForm.SetProcessing(false);
                                return;
                            }

                            // Сортируем по дате
                            allTemperatureData = allTemperatureData.OrderBy(t => t.DateTime).ToList();

                            UpdateStatus($"Загружено {allTemperatureData.Count} интерполированных температур", Color.Green);

                            // Автоматически сопоставляем данные
                            MatchDataByMinCount();
                            UpdateUIState();
                            DisplayDataPreview();

                            progressForm.UpdateProgress("Готово!", 100);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            UpdateStatus("Не удалось загрузить данные температуры", Color.Red);
                            UpdateUIState();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла температуры:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateUIState();
            }
            finally
            {
                if (progressForm != null)
                {
                    progressForm.SetProcessing(false);
                }
            }
        }

        private void MatchDataByMinCount()
        {
            matchedData = new List<HourlyData>();

            if (powerData == null || allTemperatureData == null ||
                powerData.Count == 0 || allTemperatureData.Count == 0)
            {
                return;
            }

            // Находим минимальное количество записей
            int minCount = Math.Min(powerData.Count, allTemperatureData.Count);

            // Находим общий диапазон дат
            var powerStart = powerData.First().DateTime;
            var powerEnd = powerData.Last().DateTime;
            var tempStart = allTemperatureData.First().DateTime;
            var tempEnd = allTemperatureData.Last().DateTime;

            // Находим пересечение диапазонов
            DateTime commonStart = powerStart > tempStart ? powerStart : tempStart;
            DateTime commonEnd = powerEnd < tempEnd ? powerEnd : tempEnd;

            if (commonStart > commonEnd)
            {
                MessageBox.Show("Диапазоны дат мощности и температуры не пересекаются", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Фильтруем данные по общему диапазону
            var filteredPower = powerData
                .Where(p => p.DateTime >= commonStart && p.DateTime <= commonEnd)
                .OrderBy(p => p.DateTime)
                .ToList();

            var filteredTemp = allTemperatureData
                .Where(t => t.DateTime >= commonStart && t.DateTime <= commonEnd)
                .OrderBy(t => t.DateTime)
                .ToList();

            // Берем минимальное количество из отфильтрованных данных
            int filteredMinCount = Math.Min(filteredPower.Count, filteredTemp.Count);

            // Сопоставляем данные
            for (int i = 0; i < filteredMinCount; i++)
            {
                matchedData.Add(new HourlyData
                {
                    DateTime = filteredPower[i].DateTime,
                    Hour = filteredPower[i].Hour,
                    Value = filteredTemp[i].Value, // Температура
                    PowerValue = filteredPower[i].Value // Мощность
                });
            }

            Debug.WriteLine($"Сопоставлено {matchedData.Count} записей (по наименьшему количеству)");
            Debug.WriteLine($"Диапазон: {commonStart:dd.MM.yyyy HH:mm} - {commonEnd:dd.MM.yyyy HH:mm}");
        }

        private List<HourlyData> ParseTemperatureDataWithProgress(DataTable data,
                                                         ProcessingFormWithCancel progressForm,
                                                         CancellationTokenSource cts)
        {
            var temperatures = new List<HourlyData>();

            try
            {
                // Текущие значения года, месяца, дня
                int currentYear = 2019;
                int currentMonth = 1;
                int currentDay = 1;

                int totalRows = data.Rows.Count;

                // Парсим построчно
                for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                {
                    // Проверяем отмену каждые 100 строк
                    if (rowIndex % 100 == 0 && cts.Token.IsCancellationRequested)
                    {
                        return new List<HourlyData>();
                    }

                    // Обновляем прогресс каждые 1000 строк
                    if (rowIndex % 1000 == 0)
                    {
                        int progress = (rowIndex * 100) / Math.Max(1, totalRows);
                        progressForm.UpdateProgress($"Обработка строк {rowIndex + 1} из {totalRows}...", progress);
                    }

                    var row = data.Rows[rowIndex];

                    // 1. Проверяем столбец A - ГОД
                    if (row.ItemArray.Length > 0)
                    {
                        string yearStr = row[0]?.ToString().Trim();
                        if (!string.IsNullOrEmpty(yearStr) && int.TryParse(yearStr, out int year))
                        {
                            if (year >= 2000 && year <= 2100)
                            {
                                currentYear = year;
                            }
                        }
                    }

                    // 2. Проверяем столбец B - МЕСЯЦ
                    if (row.ItemArray.Length > 1)
                    {
                        string monthStr = row[1]?.ToString().Trim();
                        if (!string.IsNullOrEmpty(monthStr))
                        {
                            int month = ParseMonthNameToNumber(monthStr);
                            if (month > 0)
                            {
                                currentMonth = month;
                            }
                        }
                    }

                    // 3. Проверяем столбец C - ДЕНЬ (формат "01 вт")
                    if (row.ItemArray.Length > 2)
                    {
                        string dayStr = row[2]?.ToString().Trim();
                        if (!string.IsNullOrEmpty(dayStr))
                        {
                            // Извлекаем число из начала строки
                            string dayNumber = new string(dayStr.TakeWhile(char.IsDigit).ToArray());
                            if (int.TryParse(dayNumber, out int day) && day >= 1 && day <= 31)
                            {
                                currentDay = day;
                            }
                        }
                    }

                    // 4. ГЛАВНОЕ: Проверяем столбцы D и E - ЧАС и ТЕМПЕРАТУРА
                    if (row.ItemArray.Length > 4) // Должны быть столбцы A-E
                    {
                        string hourStr = row[3]?.ToString().Trim(); // Столбец D
                        string tempStr = row[4]?.ToString().Trim(); // Столбец E

                        // НАС ИНТЕРЕСУЮТ ТОЛЬКО СТРОКИ С ЧАСОМ!
                        if (!string.IsNullOrEmpty(hourStr) && !string.IsNullOrEmpty(tempStr))
                        {
                            // Проверяем, что hourStr - это число (0-23)
                            if (int.TryParse(hourStr, out int hour) && hour >= 0 && hour <= 23)
                            {
                                // Преобразуем температуру (заменяем запятую на точку)
                                tempStr = tempStr.Replace(',', '.');
                                if (double.TryParse(tempStr,
                                    System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out double temperature))
                                {
                                    // Создаем дату с проверкой
                                    DateTime? dateTime = CreateSafeDateTime(currentYear, currentMonth, currentDay, hour);

                                    if (dateTime.HasValue)
                                    {
                                        temperatures.Add(new HourlyData
                                        {
                                            DateTime = dateTime.Value,
                                            Hour = hour,
                                            Value = Math.Round(temperature, 1)
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                Debug.WriteLine($"=== Найдено {temperatures.Count} температур по часам ===");

                // Теперь для каждого дня, где есть температуры, интерполируем недостающие часы
                return InterpolateMissingHoursWithProgress(temperatures, progressForm, cts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка парсинга температуры:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return temperatures;
            }
        }

        // Метод для интерполяции недостающих часов
        private List<HourlyData> InterpolateMissingHoursWithProgress(List<HourlyData> hourlyTemperatures,
                                                             ProcessingFormWithCancel progressForm,
                                                             CancellationTokenSource cts)
        {
            var allTemperatures = new List<HourlyData>();

            if (hourlyTemperatures.Count == 0)
                return allTemperatures;

            // Группируем по дням
            var dailyGroups = hourlyTemperatures
                .GroupBy(t => t.DateTime.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int totalDays = dailyGroups.Count;

            for (int dayIndex = 0; dayIndex < totalDays; dayIndex++)
            {
                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                {
                    progressForm.UpdateProgress("Операция прервана пользователем", 0);
                    return new List<HourlyData>();
                }

                var dayGroup = dailyGroups[dayIndex];
                var dayDate = dayGroup.Key;
                var dayTemps = dayGroup.OrderBy(t => t.Hour).ToList();

                // Обновляем прогресс
                int progress = (dayIndex * 100) / Math.Max(1, totalDays);
                progressForm.UpdateProgress($"Интерполяция температуры: день {dayIndex + 1} из {totalDays}", progress);

                Debug.WriteLine($"День {dayDate:dd.MM.yyyy}: {dayTemps.Count} измерений");

                if (dayTemps.Count >= 2)
                {
                    // Есть минимум 2 точки - интерполируем
                    var x = dayTemps.Select(t => (double)t.Hour).ToArray();
                    var y = dayTemps.Select(t => t.Value).ToArray();

                    try
                    {
                        var spline = Interpolate.CubicSpline(x, y);

                        // Интерполируем все 24 часа
                        for (int hour = 0; hour < 24; hour++)
                        {
                            double temperature;

                            if (hour >= x[0] && hour <= x[x.Length - 1])
                            {
                                // Интерполяция внутри диапазона
                                temperature = spline.Interpolate(hour);
                            }
                            else if (hour < x[0])
                            {
                                // Экстраполяция влево - берем первое значение
                                temperature = y[0];
                            }
                            else
                            {
                                // Экстраполяция вправо - берем последнее значение
                                temperature = y[y.Length - 1];
                            }

                            allTemperatures.Add(new HourlyData
                            {
                                DateTime = dayDate.AddHours(hour),
                                Hour = hour,
                                Value = Math.Round(temperature, 1)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка интерполяции для {dayDate:dd.MM.yyyy}: {ex.Message}");
                        // Если не удалось интерполировать, используем среднее
                        double avgTemp = dayTemps.Average(t => t.Value);
                        for (int hour = 0; hour < 24; hour++)
                        {
                            allTemperatures.Add(new HourlyData
                            {
                                DateTime = dayDate.AddHours(hour),
                                Hour = hour,
                                Value = Math.Round(avgTemp, 1)
                            });
                        }
                    }
                }
                else if (dayTemps.Count == 1)
                {
                    // Только одно измерение - используем его для всех часов
                    double singleTemp = dayTemps.First().Value;
                    for (int hour = 0; hour < 24; hour++)
                    {
                        allTemperatures.Add(new HourlyData
                        {
                            DateTime = dayDate.AddHours(hour),
                            Hour = hour,
                            Value = Math.Round(singleTemp, 1)
                        });
                    }
                }

                // Небольшая пауза для отображения прогресса
                Thread.Sleep(10);
            }

            return allTemperatures.OrderBy(t => t.DateTime).ToList();
        }

        // Метод для безопасного создания DateTime
        private DateTime? CreateSafeDateTime(int year, int month, int day, int hour)
        {
            try
            {
                // Базовые проверки
                if (year < 2000 || year > 2100) return null;
                if (month < 1 || month > 12) return null;
                if (day < 1 || day > 31) return null;
                if (hour < 0 || hour > 23) return null;

                // Проверяем количество дней в месяце
                int daysInMonth = DateTime.DaysInMonth(year, month);
                if (day > daysInMonth)
                {
                    // Корректируем день, если он превышает количество дней в месяце
                    day = daysInMonth;
                }

                return new DateTime(year, month, day, hour, 0, 0);
            }
            catch
            {
                return null;
            }
        }

        // Метод для парсинга названия месяца
        private int ParseMonthNameToNumber(string monthName)
        {
            if (string.IsNullOrEmpty(monthName))
                return 0;

            string lower = monthName.ToLower();

            if (lower.Contains("янв")) return 1;
            if (lower.Contains("фев")) return 2;
            if (lower.Contains("мар")) return 3;
            if (lower.Contains("апр")) return 4;
            if (lower.Contains("май")) return 5;
            if (lower.Contains("июн")) return 6;
            if (lower.Contains("июл")) return 7;
            if (lower.Contains("авг")) return 8;
            if (lower.Contains("сен")) return 9;
            if (lower.Contains("окт")) return 10;
            if (lower.Contains("ноя")) return 11;
            if (lower.Contains("дек")) return 12;

            // Пробуем распарсить как число
            if (int.TryParse(monthName, out int monthNum) && monthNum >= 1 && monthNum <= 12)
                return monthNum;

            return 0;
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            CancellationTokenSource cts = null;
            ProcessingFormWithCancel progressForm = null;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== НАЧАЛО АНАЛИЗА ===");
                System.Diagnostics.Debug.WriteLine($"Настройки Фурье: Cutoff Ratio = {_cutoffRatio:P0}, Период = {_fourierPeriodType}");

                if (matchedData == null || matchedData.Count < 4)
                {
                    MessageBox.Show($"Недостаточно данных для анализа. Нужно минимум 4 точки, а есть {matchedData?.Count ?? 0}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Создаем форму прогресса
                cts = new CancellationTokenSource();
                progressForm = new ProcessingFormWithCancel();
                progressForm.Show();
                Application.DoEvents();

                try
                {
                    progressForm.UpdateProgress("Подготовка данных...", 10);

                    // Проверяем отмену
                    if (cts.Token.IsCancellationRequested)
                    {
                        progressForm.SetProcessing(false);
                        return;
                    }

                    // 1. Сначала убедимся, что у нас есть PowerValue
                    var validData = matchedData.Where(d => d.PowerValue.HasValue).ToList();

                    if (validData.Count < 4)
                    {
                        MessageBox.Show($"Недостаточно данных с мощностью. Только {validData.Count} точек",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        progressForm.SetProcessing(false);
                        return;
                    }

                    progressForm.UpdateProgress("Разделение данных на осенне-зимние периоды...", 20);

                    // 2. Разделяем данные на осенне-зимние периоды (как и раньше, для регрессии)
                    var autumnWinterPeriods = SplitIntoAutumnWinterFourierPeriods(validData);

                    progressForm.UpdateProgress($"Найдено {autumnWinterPeriods.Count} осенне-зимних периода(ов)", 25);

                    // Проверяем отмену
                    if (cts.Token.IsCancellationRequested)
                    {
                        progressForm.SetProcessing(false);
                        return;
                    }

                    // 3. Обрабатываем каждый осенне-зимний период отдельно с Фурье-фильтрацией
                    var processedPeriods = new List<DataPeriod>();

                    for (int i = 0; i < autumnWinterPeriods.Count; i++)
                    {
                        // Проверяем отмену
                        if (cts.Token.IsCancellationRequested)
                        {
                            progressForm.SetProcessing(false);
                            return;
                        }

                        var period = autumnWinterPeriods[i];
                        int progress = 25 + (i * 50 / Math.Max(1, autumnWinterPeriods.Count));

                        progressForm.UpdateProgress($"Обработка периода: {period.Name} ({i + 1}/{autumnWinterPeriods.Count})...", progress);

                        // Проверяем, что в периоде достаточно данных
                        if (period.Data.Count < 4)
                        {
                            System.Diagnostics.Debug.WriteLine($"Пропущен период '{period.Name}': недостаточно данных ({period.Data.Count})");
                            continue;
                        }

                        try
                        {
                            // Разделяем данные периода
                            var periodPowerData = period.Data.Select(d => new HourlyData
                            {
                                DateTime = d.DateTime,
                                Hour = d.Hour,
                                Value = d.PowerValue.Value
                            }).ToList();

                            var periodTempData = period.Data.Select(d => new HourlyData
                            {
                                DateTime = d.DateTime,
                                Hour = d.Hour,
                                Value = d.Value
                            }).ToList();

                            // ПРИМЕНЯЕМ ФУРЬЕ С НАСТРОЙКАМИ ПОЛЬЗОВАТЕЛЯ
                            progressForm.UpdateProgress($"Применение Фурье-фильтра (мощность)...", progress + 5);

                            var filteredPowerData = ApplyFourierFilterByPeriod(
                                periodPowerData, true, progressForm, cts);

                            // Проверяем отмену
                            if (cts.Token.IsCancellationRequested || filteredPowerData == null)
                            {
                                progressForm.SetProcessing(false);
                                return;
                            }

                            progressForm.UpdateProgress($"Применение Фурье-фильтра (температура)...", progress + 10);

                            var filteredTempData = ApplyFourierFilterByPeriod(
                                periodTempData, false, progressForm, cts);

                            // Проверяем отмену
                            if (cts.Token.IsCancellationRequested || filteredTempData == null)
                            {
                                progressForm.SetProcessing(false);
                                return;
                            }

                            // Сопоставляем отфильтрованные данные
                            var filteredMatchedData = new List<HourlyData>();
                            int minCount = Math.Min(filteredPowerData.Count, filteredTempData.Count);

                            for (int j = 0; j < minCount; j++)
                            {
                                filteredMatchedData.Add(new HourlyData
                                {
                                    DateTime = filteredPowerData[j].DateTime,
                                    Hour = filteredPowerData[j].Hour,
                                    Value = filteredTempData[j].Value,
                                    PowerValue = filteredPowerData[j].Value
                                });
                            }

                            // Добавляем обработанный период
                            processedPeriods.Add(new DataPeriod
                            {
                                Name = period.Name,
                                StartYear = period.StartYear,
                                EndYear = period.EndYear,
                                PeriodStart = period.PeriodStart,
                                PeriodEnd = period.PeriodEnd,
                                Data = filteredMatchedData
                            });

                            System.Diagnostics.Debug.WriteLine($"Обработан период '{period.Name}': {filteredMatchedData.Count} точек");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка при обработке периода '{period.Name}': {ex.Message}");
                            // В случае ошибки используем оригинальные данные
                            processedPeriods.Add(period);
                        }
                    }

                    // Проверяем, что есть обработанные периоды
                    if (processedPeriods.Count == 0)
                    {
                        MessageBox.Show("Нет данных для построения графиков",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        progressForm.SetProcessing(false);
                        return;
                    }

                    progressForm.UpdateProgress("Открытие окна с маркерами...", 80);

                    // 4. Показываем форму с маркерами (ChartForm)
                    // Здесь нужно создать ChartForm, которая покажет маркеры после Фурье
                    // и будет иметь кнопку "Аппроксимировать"

                    progressForm.SetProcessing(false);

                    // Создаем и показываем форму с маркерами
                    var chartForm = new ChartForm(processedPeriods, true);
                    chartForm.ShowDialog();

                    // Когда пользователь нажмет "Аппроксимировать" в ChartForm,
                    // откроется RegressionFormDB с результатами регрессии

                }
                catch (Exception ex)
                {
                    progressForm.SetProcessing(false);
                    MessageBox.Show($"Ошибка при обработке данных:\n{ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка BtnAnalyze_Click: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== КОНЕЦ АНАЛИЗА ===");
            }
        }

        /// <summary>
        /// Разделяет данные на осенне-зимние периоды для Фурье
        /// </summary>
        private List<DataPeriod> SplitIntoAutumnWinterFourierPeriods(List<HourlyData> allData)
        {
            var periods = new List<DataPeriod>();

            if (allData == null || allData.Count == 0)
                return periods;

            // Просто возвращаем все данные как один период
            periods.Add(new DataPeriod
            {
                Name = "Весь период",
                StartYear = allData.Min(d => d.DateTime).Year,
                EndYear = allData.Max(d => d.DateTime).Year,
                PeriodStart = allData.Min(d => d.DateTime),
                PeriodEnd = allData.Max(d => d.DateTime),
                Data = allData.OrderBy(d => d.DateTime).ToList()
            });

            System.Diagnostics.Debug.WriteLine($"Создан 1 общий период для Фурье");
            return periods;
        }

        private void UpdateStatus(string message, Color color)
        {
            lblStatus.Text = $"Статус: {message}";
            lblStatus.ForeColor = color;
        }

        private void DisplayDataPreview()
        {
            dgvPowerPreview.Rows.Clear();
            dgvPowerPreview.Columns.Clear();

            // Настраиваем колонки
            dgvPowerPreview.Columns.Add("DateTime", "Дата и время");
            dgvPowerPreview.Columns.Add("Hour", "Час");
            dgvPowerPreview.Columns.Add("Temperature", "Температура, °C");
            dgvPowerPreview.Columns.Add("Power", "Мощность, МВт");

            dgvPowerPreview.Columns["DateTime"].Width = 180;
            dgvPowerPreview.Columns["Hour"].Width = 80;
            dgvPowerPreview.Columns["Temperature"].Width = 120;
            dgvPowerPreview.Columns["Power"].Width = 120;

            // Показываем данные только если они сопоставлены
            if (matchedData != null && matchedData.Count > 0)
            {
                // Показываем первые 5 записей
                int showCount = Math.Min(5, matchedData.Count);

                // Первые записи
                for (int i = 0; i < showCount; i++)
                {
                    var item = matchedData[i];
                    dgvPowerPreview.Rows.Add(
                        item.DateTime.ToString("dd.MM.yyyy HH:mm"),
                        $"{item.Hour:00}:00",
                        item.Value.ToString("F1"),
                        item.PowerValue?.ToString("F2") ?? "N/A"
                    );
                }

                // Разделитель, если есть более 10 записей
                if (matchedData.Count > 10)
                {
                    dgvPowerPreview.Rows.Add("...", "...", "...", "...");

                    // Последние записи
                    for (int i = matchedData.Count - showCount; i < matchedData.Count; i++)
                    {
                        var item = matchedData[i];
                        dgvPowerPreview.Rows.Add(
                            item.DateTime.ToString("dd.MM.yyyy HH:mm"),
                            $"{item.Hour:00}:00",
                            item.Value.ToString("F1"),
                            item.PowerValue?.ToString("F2") ?? "N/A"
                        );
                    }
                }

                // Обновляем заголовок
                UpdatePreviewTitle();
            }
            else
            {
                // Показываем информацию о загрузке
                if (powerData != null && powerData.Count > 0 &&
                    allTemperatureData != null && allTemperatureData.Count > 0)
                {
                    dgvPowerPreview.Rows.Add("Данные загружены", "", "", "");
                    dgvPowerPreview.Rows.Add($"Мощность: {powerData.Count} записей", "", "", "");
                    dgvPowerPreview.Rows.Add($"Температура: {allTemperatureData.Count} записей", "", "", "");
                    dgvPowerPreview.Rows.Add("Сопоставление...", "", "", "");
                }
            }
        }

        private void UpdatePreviewTitle()
        {
            if (matchedData != null && matchedData.Count > 0)
            {
                int powerCount = powerData?.Count ?? 0;
                int tempCount = allTemperatureData?.Count ?? 0;
                int matchedCount = matchedData.Count;

                lblPowerPreview.Text = $"Сопоставленные данные: {matchedCount} записей " +
                                      $"(из {powerCount} мощности и {tempCount} температуры)";
            }
        }
    }
}