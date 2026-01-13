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
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class InitialFormStatic : Form
    {
        private List<HourlyData> powerData;
        private List<HourlyData> allTemperatureData; // Все интерполированные температуры
        private List<HourlyData> matchedData; // Сопоставленные данные

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

        private void ConfigureForm()
        {
            this.Text = "Статический анализ зависимостей";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(1000, 700);

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

        private void UpdateUIState()
        {
            bool hasPowerData = powerData != null && powerData.Count > 0;
            bool hasTempData = allTemperatureData != null && allTemperatureData.Count > 0;

            // Шаг 1: Разрешаем загрузку температуры только после загрузки мощности
            lblTemperatureFile.Enabled = hasPowerData;
            txtTemperatureFile.Enabled = hasPowerData;
            btnBrowseTemperature.Enabled = hasPowerData;

            // Шаг 2: Разрешаем построение графика только после загрузки обоих файлов
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
                lblPowerPreview.Text = $"Сопоставленные данные (по наименьшему количеству):";
            }
        }

        private void InitializeEventHandlers()
        {
            btnBrowsePower.Click += BtnBrowsePower_Click;
            btnBrowseTemperature.Click += BtnBrowseTemperature_Click;
            btnAnalyze.Click += BtnAnalyze_Click;
        }

        #region Загрузка данных мощности

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

        #endregion

        #region Загрузка данных температуры (все листы)

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
                        allTemperatureData = new List<HourlyData>();

                        // Обрабатываем все листы
                        foreach (DataTable sheet in result.Tables)
                        {
                            var sheetTemperatures = ParseTemperatureData(sheet);
                            if (sheetTemperatures != null && sheetTemperatures.Count > 0)
                            {
                                allTemperatureData.AddRange(sheetTemperatures);
                            }
                        }

                        if (allTemperatureData.Count > 0)
                        {
                            // Сортируем по дате
                            allTemperatureData = allTemperatureData.OrderBy(t => t.DateTime).ToList();

                            UpdateStatus($"Загружено {allTemperatureData.Count} интерполированных температур", Color.Green);

                            // Автоматически сопоставляем данные
                            MatchDataByMinCount();
                            UpdateUIState();
                            DisplayDataPreview();
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

        private List<HourlyData> ParseTemperatureData(DataTable data)
        {
            var temperatures = new List<HourlyData>();

            try
            {
                // Текущие значения года, месяца, дня
                int currentYear = 2019;
                int currentMonth = 1;
                int currentDay = 1;

                // Парсим построчно
                for (int rowIndex = 0; rowIndex < data.Rows.Count; rowIndex++)
                {
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

                                        // Логируем для отладки
                                        Debug.WriteLine($"Температура: {dateTime.Value:dd.MM.yyyy HH:mm} = {temperature}°C");
                                    }
                                }
                            }
                        }
                    }
                }

                Debug.WriteLine($"=== Найдено {temperatures.Count} температур по часам ===");

                // Теперь для каждого дня, где есть температуры, интерполируем недостающие часы
                return InterpolateMissingHours(temperatures);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка парсинга температуры:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return temperatures;
            }
        }

        // Метод для интерполяции недостающих часов
        private List<HourlyData> InterpolateMissingHours(List<HourlyData> hourlyTemperatures)
        {
            var allTemperatures = new List<HourlyData>();

            if (hourlyTemperatures.Count == 0)
                return allTemperatures;

            // Группируем по дням
            var dailyGroups = hourlyTemperatures
                .GroupBy(t => t.DateTime.Date)
                .OrderBy(g => g.Key);

            foreach (var dayGroup in dailyGroups)
            {
                var dayDate = dayGroup.Key;
                var dayTemps = dayGroup.OrderBy(t => t.Hour).ToList();

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

        #endregion

        #region Сопоставление данных

        #endregion

        #region Анализ зависимости

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                // ОТЛАДКА: проверяем данные перед обработкой
                System.Diagnostics.Debug.WriteLine("=== НАЧАЛО АНАЛИЗА ===");
                System.Diagnostics.Debug.WriteLine($"matchedData: {(matchedData == null ? "null" : matchedData.Count.ToString())} точек");

                if (matchedData != null && matchedData.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Диапазон дат: {matchedData.Min(d => d.DateTime):dd.MM.yyyy HH:mm} - {matchedData.Max(d => d.DateTime):dd.MM.yyyy HH:mm}");

                    // Проверяем наличие PowerValue
                    int withPower = matchedData.Count(d => d.PowerValue.HasValue);
                    int withoutPower = matchedData.Count(d => !d.PowerValue.HasValue);
                    System.Diagnostics.Debug.WriteLine($"С PowerValue: {withPower}, без PowerValue: {withoutPower}");

                    if (withPower == 0)
                    {
                        MessageBox.Show("Нет данных с мощностью для анализа", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (matchedData == null || matchedData.Count < 4)
                {
                    MessageBox.Show($"Недостаточно данных для анализа. Нужно минимум 4 точки, а есть {matchedData?.Count ?? 0}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Показываем диалог прогресса
                var progressForm = new ProcessingForm();
                progressForm.Show();
                Application.DoEvents();

                try
                {
                    progressForm.UpdateProgress("Разделение данных на периоды...", 10);

                    // 1. Сначала убедимся, что у нас есть PowerValue
                    var validData = matchedData.Where(d => d.PowerValue.HasValue).ToList();
                    System.Diagnostics.Debug.WriteLine($"Данных с мощностью: {validData.Count}");

                    if (validData.Count < 4)
                    {
                        progressForm.Close();
                        MessageBox.Show($"Недостаточно данных с мощностью. Только {validData.Count} точек",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 2. Разделяем данные на осенне-зимние периоды
                    var dataPeriods = SplitIntoAutumnWinterPeriods(validData);

                    if (dataPeriods.Count == 0)
                    {
                        // Если не удалось создать периоды, используем все данные как один период
                        System.Diagnostics.Debug.WriteLine("Не удалось создать периоды, используем все данные как один период");

                        var fallbackPeriod = new DataPeriod
                        {
                            Name = "Все данные",
                            StartYear = validData.Min(d => d.DateTime.Year),
                            EndYear = validData.Max(d => d.DateTime.Year),
                            PeriodStart = validData.Min(d => d.DateTime),
                            PeriodEnd = validData.Max(d => d.DateTime),
                            Data = validData
                        };

                        dataPeriods.Add(fallbackPeriod);
                    }

                    progressForm.UpdateProgress($"Найдено {dataPeriods.Count} периода(ов)", 20);
                    System.Diagnostics.Debug.WriteLine($"Создано периодов: {dataPeriods.Count}");

                    // 3. Обрабатываем каждый период отдельно
                    var processedPeriods = new List<DataPeriod>();

                    for (int i = 0; i < dataPeriods.Count; i++)
                    {
                        var period = dataPeriods[i];
                        int progress = 20 + (i * 70 / Math.Max(1, dataPeriods.Count));

                        progressForm.UpdateProgress($"Обработка периода: {period.Name}...", progress);
                        System.Diagnostics.Debug.WriteLine($"Обработка периода {period.Name}: {period.Data.Count} точек");

                        // Проверяем, что в периоде достаточно данных
                        if (period.Data.Count < 4)
                        {
                            System.Diagnostics.Debug.WriteLine($"Пропущен период '{period.Name}': недостаточно данных ({period.Data.Count})");
                            continue;
                        }

                        // Применяем Фурье (опционально, можно закомментировать для отладки)
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

                            // Можно заменить на это, чтобы без Фурье, тогда будут точки класс
                            // var filteredPowerData = periodPowerData;
                            // var filteredTempData = periodTempData;
                            // Применяем Фурье
                            // cutoffRatio - сила сглаживание, чем меньше, тем сильнее
                            double cutoffRatio = 0.1;
                            var filteredPowerData = FourierProcessor.ProcessPowerData(periodPowerData, cutoffRatio);
                            var filteredTempData = FourierProcessor.ProcessTemperatureData(periodTempData, cutoffRatio);

                            // Проверяем длины
                            if (filteredPowerData.Count != periodPowerData.Count ||
                                filteredTempData.Count != periodTempData.Count)
                            {
                                System.Diagnostics.Debug.WriteLine($"ОШИБКА Фурье: длины не совпадают. Используем оригинальные данные.");
                                processedPeriods.Add(period);
                                continue;
                            }

                            // Сопоставляем отфильтрованные данные
                            var filteredMatchedData = new List<HourlyData>();
                            for (int j = 0; j < periodPowerData.Count; j++)
                            {
                                filteredMatchedData.Add(new HourlyData
                                {
                                    DateTime = periodPowerData[j].DateTime,
                                    Hour = periodPowerData[j].Hour,
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
                        progressForm.Close();
                        MessageBox.Show("Нет данных для построения графиков",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    progressForm.UpdateProgress("Построение графика...", 95);

                    // 4. Показываем форму с графиками
                    System.Diagnostics.Debug.WriteLine($"Открываем ChartForm с {processedPeriods.Count} периодами");

                    var chartForm = new ChartForm(processedPeriods, true);
                    progressForm.UpdateProgress("Готово!", 100);
                    System.Threading.Thread.Sleep(300);

                    progressForm.Close();
                    chartForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    progressForm.Close();
                    System.Diagnostics.Debug.WriteLine($"Ошибка BtnAnalyze_Click (внутренняя): {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show($"Ошибка при обработке данных:\n{ex.Message}\n\nПодробности в Debug Output", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка BtnAnalyze_Click (внешняя): {ex.Message}\n{ex.StackTrace}");
                
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== КОНЕЦ АНАЛИЗА ===");
            }
        }

        private List<DataPeriod> SplitIntoAutumnWinterPeriods(List<HourlyData> allData)
        {
            var periods = new List<DataPeriod>();

            if (allData == null || allData.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("SplitIntoAutumnWinterPeriods: allData пуст или null");
                return periods;
            }

            System.Diagnostics.Debug.WriteLine($"SplitIntoAutumnWinterPeriods: всего {allData.Count} точек");

            // Находим диапазон дат
            DateTime minDate = allData.Min(d => d.DateTime);
            DateTime maxDate = allData.Max(d => d.DateTime);
            System.Diagnostics.Debug.WriteLine($"Диапазон дат: {minDate:dd.MM.yyyy} - {maxDate:dd.MM.yyyy}");

            // Группируем данные по годам и месяцам
            var monthlyGroups = allData
                .GroupBy(d => new { d.DateTime.Year, d.DateTime.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Найдено месяцев: {monthlyGroups.Count}");

            // Для каждого осенне-зимнего периода (сентябрь-май)
            // Ищем года, где есть данные с сентября по май
            var years = allData.Select(d => d.DateTime.Year).Distinct().OrderBy(y => y).ToList();

            foreach (int year in years)
            {
                int nextYear = year + 1;

                // Собираем данные для периода: сентябрь-декабрь текущего года + январь-май следующего
                List<HourlyData> periodData = new List<HourlyData>();

                // Текущий год: сентябрь-декабрь
                for (int month = 9; month <= 12; month++)
                {
                    var monthData = allData.Where(d =>
                        d.DateTime.Year == year &&
                        d.DateTime.Month == month).ToList();

                    if (monthData.Count > 0)
                    {
                        periodData.AddRange(monthData);
                        System.Diagnostics.Debug.WriteLine($"Год {year}, месяц {month}: {monthData.Count} точек");
                    }
                }

                // Следующий год: январь-май
                for (int month = 1; month <= 5; month++)
                {
                    var monthData = allData.Where(d =>
                        d.DateTime.Year == nextYear &&
                        d.DateTime.Month == month).ToList();

                    if (monthData.Count > 0)
                    {
                        periodData.AddRange(monthData);
                        System.Diagnostics.Debug.WriteLine($"Год {nextYear}, месяц {month}: {monthData.Count} точек");
                    }
                }

                // Если в периоде достаточно данных
                if (periodData.Count >= 24) // Минимум 1 день данных (24 часа)
                {
                    // Сортируем по дате
                    periodData = periodData.OrderBy(d => d.DateTime).ToList();

                    // Определяем фактические границы периода
                    DateTime actualStart = periodData.Min(d => d.DateTime);
                    DateTime actualEnd = periodData.Max(d => d.DateTime);

                    var period = new DataPeriod
                    {
                        Name = $"Период {year}-{nextYear}",
                        StartYear = year,
                        EndYear = nextYear,
                        PeriodStart = actualStart,
                        PeriodEnd = actualEnd,
                        Data = periodData
                    };

                    periods.Add(period);

                    System.Diagnostics.Debug.WriteLine($"Создан период {period.Name}: " +
                        $"{periodData.Count} точек, " +
                        $"{actualStart:dd.MM.yyyy} - {actualEnd:dd.MM.yyyy}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Период {year}-{nextYear} пропущен: только {periodData.Count} точек");
                }
            }

            // Если не нашли полных периодов, создаем периоды по годам
            if (periods.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Создаем упрощенные периоды по годам...");

                foreach (int year in years)
                {
                    var yearData = allData.Where(d => d.DateTime.Year == year).ToList();

                    if (yearData.Count >= 24) // Минимум 1 день
                    {
                        // Находим фактические даты начала и конца
                        DateTime startDate = yearData.Min(d => d.DateTime);
                        DateTime endDate = yearData.Max(d => d.DateTime);

                        // Оставляем только данные с сентября по май
                        var filteredData = yearData.Where(d =>
                            d.DateTime.Month >= 9 || d.DateTime.Month <= 5).ToList();

                        if (filteredData.Count >= 24)
                        {
                            var period = new DataPeriod
                            {
                                Name = $"Год {year}",
                                StartYear = year,
                                EndYear = year,
                                PeriodStart = startDate,
                                PeriodEnd = endDate,
                                Data = filteredData.OrderBy(d => d.DateTime).ToList()
                            };

                            periods.Add(period);
                            System.Diagnostics.Debug.WriteLine($"Создан упрощенный период {period.Name}: {filteredData.Count} точек");
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Всего создано периодов: {periods.Count}");

            // Логируем информацию о периодах
            foreach (var period in periods)
            {
                System.Diagnostics.Debug.WriteLine($"Период '{period.Name}': {period.Data.Count} точек, " +
                    $"{period.PeriodStart:dd.MM.yyyy} - {period.PeriodEnd:dd.MM.yyyy}");
            }

            return periods;
        }

        #endregion

        #region Вспомогательные методы

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

        #endregion
    }
}