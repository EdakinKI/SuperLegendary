using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ExcelDataReader;
using MathNet.Numerics;
using ClassLibrary1;

using System.Diagnostics;

namespace WindowsFormsApp1
{
    public partial class InitialFormStatic : Form
    {
        private List<HourlyData> powerData;
        private List<HourlyData> allTemperatureData;
        private List<HourlyData> matchedData;
        private ComboBox cmbFourierPeriod;
        private NumericUpDown nudCutoffRatio;
        private Button btnCancelProcessing;
        private bool processingCancelled = false;
        private ProcessingFormWithCancel progressForm;

        public InitialFormStatic()
        {
            InitializeComponent();

            powerData = new List<HourlyData>();
            allTemperatureData = new List<HourlyData>();
            matchedData = new List<HourlyData>();

            ConfigureForm();
            InitializeEventHandlers();
            AddFourierSettingsControls();
            UpdateUIState();
        }

        private void ConfigureForm()
        {
            this.Text = "Статический анализ зависимостей";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(1100, 800); // Увеличили высоту

            // Перемещаем кнопку "Построение графика" ниже
            btnAnalyze.Location = new Point(20, 280);

            // Увеличиваем высоту предпросмотра
            dgvPowerPreview.Size = new Size(1050, 300);
            lblPowerPreview.Location = new Point(20, 330);
            dgvPowerPreview.Location = new Point(20, 360);
        }

        private void AddFourierSettingsControls()
        {
            // Метка для периода Фурье
            var lblFourierPeriod = new Label
            {
                Text = "Период Фурье:",
                Location = new Point(20, 190),
                Size = new Size(120, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            // Выпадающий список периода
            cmbFourierPeriod = new ComboBox
            {
                Location = new Point(140, 187),
                Size = new Size(150, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            cmbFourierPeriod.Items.AddRange(new string[] {
                "Весь период",
                "Месяц",
                "Неделя",
                "День"
            });
            cmbFourierPeriod.SelectedIndex = 0;

            // Метка для cutoff ratio
            var lblCutoffRatio = new Label
            {
                Text = "Процент низких частот:",
                Location = new Point(300, 190),
                Size = new Size(150, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            // NumericUpDown для cutoff ratio
            nudCutoffRatio = new NumericUpDown
            {
                Location = new Point(450, 187),
                Size = new Size(80, 22),
                Minimum = 0,
                Maximum = 1,
                DecimalPlaces = 2,
                Increment = 0.05M,
                Value = 0.10M,
                Enabled = false
            };

            // Подпись для cutoff ratio
            var lblCutoffInfo = new Label
            {
                Text = "(0-1, по умолчанию 0.1)",
                Location = new Point(540, 190),
                Size = new Size(120, 20),
                Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            // Кнопка отмены обработки
            btnCancelProcessing = new Button
            {
                Text = "Прервать обработку",
                Location = new Point(680, 185),
                Size = new Size(150, 30),
                BackColor = Color.LightCoral,
                Visible = false,
                Enabled = false
            };

            btnCancelProcessing.Click += (s, e) =>
            {
                processingCancelled = true;
                btnCancelProcessing.Enabled = false;
                btnCancelProcessing.Text = "Прерывание...";
                if (progressForm != null)
                    progressForm.RequestCancel();
            };

            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblFourierPeriod, cmbFourierPeriod,
                lblCutoffRatio, nudCutoffRatio, lblCutoffInfo,
                btnCancelProcessing
            });
        }

        private void AddCancelButton()
        {
            btnCancelProcessing = new Button
            {
                Text = "Прервать обработку",
                Location = new Point(650, 187),
                Size = new Size(150, 30),
                BackColor = Color.LightCoral,
                Visible = false,
                Enabled = false
            };

            btnCancelProcessing.Click += (s, e) =>
            {
                processingCancelled = true;
                btnCancelProcessing.Enabled = false;
                btnCancelProcessing.Text = "Прерывание...";
            };

            this.Controls.Add(btnCancelProcessing);
        }

        private void UpdateUIState()
        {
            bool hasPowerData = powerData != null && powerData.Count > 0;
            bool hasTempData = allTemperatureData != null && allTemperatureData.Count > 0;

            // Разрешаем настройки Фурье только после загрузки обоих файлов
            cmbFourierPeriod.Enabled = hasPowerData && hasTempData;
            nudCutoffRatio.Enabled = hasPowerData && hasTempData;

            lblTemperatureFile.Enabled = hasPowerData;
            txtTemperatureFile.Enabled = hasPowerData;
            btnBrowseTemperature.Enabled = hasPowerData;

            btnAnalyze.Enabled = hasPowerData && hasTempData && matchedData != null && matchedData.Count > 0;
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

                        // Показываем прогресс
                        var progressForm = new ProcessingFormWithCancel();
                        progressForm.Show();
                        progressForm.UpdateProgress("Загрузка данных температуры...", 10);

                        // Обрабатываем все листы
                        for (int i = 0; i < result.Tables.Count; i++)
                        {
                            if (progressForm.IsCancellationRequested)
                                break;

                            var sheet = result.Tables[i];
                            progressForm.UpdateProgress($"Обработка листа {sheet.TableName}...",
                                10 + (i * 30 / Math.Max(1, result.Tables.Count)));

                            var sheetTemperatures = ParseTemperatureData(sheet, progressForm);
                            if (sheetTemperatures != null && sheetTemperatures.Count > 0)
                            {
                                allTemperatureData.AddRange(sheetTemperatures);
                            }
                        }

                        if (!progressForm.IsCancellationRequested && allTemperatureData.Count > 0)
                        {
                            allTemperatureData = allTemperatureData.OrderBy(t => t.DateTime).ToList();

                            progressForm.UpdateProgress("Интерполяция часов...", 60);
                            // Вызываем интерполяцию с ProgressBar
                            allTemperatureData = InterpolateMissingHoursWithProgress(allTemperatureData,
                                progressForm, "Интерполяция температурных данных");

                            if (!progressForm.IsCancellationRequested)
                            {
                                UpdateStatus($"Загружено {allTemperatureData.Count} интерполированных температур", Color.Green);
                                MatchDataByMinCount();
                                UpdateUIState();
                                DisplayDataPreview();
                                progressForm.UpdateProgress("Готово!", 100);
                                System.Threading.Thread.Sleep(300);
                            }
                        }

                        progressForm.Close();

                        if (progressForm.IsCancellationRequested)
                        {
                            allTemperatureData.Clear();
                            txtTemperatureFile.Text = "";
                            UpdateStatus("Загрузка отменена", Color.Orange);
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

        private List<HourlyData> ParseTemperatureData(DataTable data, ProcessingFormWithCancel progressForm = null)
        {
            var temperatures = new List<HourlyData>();

            try
            {
                // Текущие значения года, месяца, дня
                int currentYear = 2019;
                int currentMonth = 1;
                int currentDay = 1;
                int totalRows = data.Rows.Count;

                for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                {
                    if (progressForm?.IsCancellationRequested == true)
                        return temperatures;

                    var row = data.Rows[rowIndex];

                    // Обновляем прогресс каждые 100 строк
                    if (rowIndex % 100 == 0 && progressForm != null)
                    {
                        int progress = 40 + (rowIndex * 20 / Math.Max(1, totalRows));
                        progressForm.UpdateProgress($"Парсинг строк {rowIndex}/{totalRows}...", progress);
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

                System.Diagnostics.Debug.WriteLine($"=== Найдено {temperatures.Count} температур по часам ===");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка парсинга температуры:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return temperatures;
        }

        // Метод для интерполяции недостающих часов
        private List<HourlyData> InterpolateMissingHoursWithProgress(List<HourlyData> hourlyTemperatures,
            ProcessingFormWithCancel progressForm = null, string progressMessage = "Интерполяция температур...")
        {
            var allTemperatures = new List<HourlyData>();

            if (hourlyTemperatures.Count == 0)
                return allTemperatures;

            // Показываем начальный прогресс
            if (progressForm != null)
                progressForm.UpdateProgress($"{progressMessage} 0%", 0);

            // Группируем по дням
            var dailyGroups = hourlyTemperatures
                .GroupBy(t => t.DateTime.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int totalDays = dailyGroups.Count;
            int processedDays = 0;

            foreach (var dayGroup in dailyGroups)
            {
                if (processingCancelled || (progressForm?.IsCancellationRequested == true))
                {
                    System.Diagnostics.Debug.WriteLine("Интерполяция прервана");
                    return allTemperatures;
                }

                var dayDate = dayGroup.Key;
                var dayTemps = dayGroup.OrderBy(t => t.Hour).ToList();

                // Обновляем прогресс
                if (progressForm != null)
                {
                    processedDays++;
                    int progress = 10 + (processedDays * 80 / Math.Max(1, totalDays));
                    progressForm.UpdateProgress($"{progressMessage} {processedDays}/{totalDays} дней", progress);
                }

                if (dayTemps.Count >= 2)
                {
                    try
                    {
                        var x = dayTemps.Select(t => (double)t.Hour).ToArray();
                        var y = dayTemps.Select(t => t.Value).ToArray();

                        // Выполняем кубическую сплайн-интерполяцию
                        var spline = Interpolate.CubicSpline(x, y);

                        // Интерполируем все 24 часа
                        for (int hour = 0; hour < 24; hour++)
                        {
                            double temperature = hour >= x[0] && hour <= x[x.Length - 1]
                                ? spline.Interpolate(hour)
                                : (hour < x[0] ? y[0] : y[y.Length - 1]);

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
                        System.Diagnostics.Debug.WriteLine($"Ошибка интерполяции для {dayDate:dd.MM.yyyy}: {ex.Message}");

                        // Резервный вариант: среднее значение
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
                    // Только одно измерение
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

                // Небольшая задержка для плавности прогресса
                if (progressForm != null && totalDays > 10)
                    System.Threading.Thread.Sleep(10);
            }

            if (progressForm != null)
                progressForm.UpdateProgress($"{progressMessage} завершено", 95);

            return allTemperatures.OrderBy(t => t.DateTime).ToList();
        }

        private List<DataPeriod> ProcessPeriodsWithFourier(List<DataPeriod> periods, string fourierPeriodType,
            double cutoffRatio, ProcessingFormWithCancel progressForm)
        {
            var processedPeriods = new List<DataPeriod>();
            int totalPeriods = periods.Count;

            for (int periodIndex = 0; periodIndex < totalPeriods; periodIndex++)
            {
                if (processingCancelled || progressForm.IsCancellationRequested)
                    break;

                var period = periods[periodIndex];
                int progress = 20 + (periodIndex * 60 / Math.Max(1, totalPeriods));

                progressForm.UpdateProgress($"Обработка {period.Name} ({fourierPeriodType})...", progress);

                if (period.Data.Count < 4)
                    continue;

                // Разделяем период согласно выбранному типу Фурье
                var subPeriods = SplitPeriodByType(period, fourierPeriodType);
                int totalSubPeriods = subPeriods.Count;

                for (int subIndex = 0; subIndex < totalSubPeriods; subIndex++)
                {
                    if (processingCancelled || progressForm.IsCancellationRequested)
                        break;

                    var subPeriod = subPeriods[subIndex];

                    try
                    {
                        // Обновляем прогресс для подпериода
                        int subProgress = progress + ((subIndex + 1) * 10 / Math.Max(1, totalSubPeriods));
                        progressForm.UpdateProgress($"Обработка {subPeriod.Name}...", subProgress);

                        var processedData = ProcessSubPeriodWithFourier(subPeriod, fourierPeriodType, cutoffRatio);

                        if (processedData != null && processedData.Count > 0)
                        {
                            processedPeriods.Add(new DataPeriod
                            {
                                Name = $"{period.Name} - {subPeriod.Name}",
                                StartYear = period.StartYear,
                                EndYear = period.EndYear,
                                PeriodStart = subPeriod.PeriodStart,
                                PeriodEnd = subPeriod.PeriodEnd,
                                Data = processedData
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка обработки подпериода: {ex.Message}");
                    }
                }
            }

            return processedPeriods;
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
            try
            {
                if (matchedData == null || matchedData.Count < 4)
                {
                    MessageBox.Show($"Недостаточно данных для анализа. Нужно минимум 4 точки, а есть {matchedData?.Count ?? 0}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Сбрасываем флаг прерывания
                processingCancelled = false;
                btnCancelProcessing.Visible = true;
                btnCancelProcessing.Enabled = true;
                btnCancelProcessing.Text = "Прервать обработку";

                // Показываем диалог прогресса
                progressForm = new ProcessingFormWithCancel();
                progressForm.CancelRequested += (s, args) => processingCancelled = true;
                progressForm.Show();
                Application.DoEvents();

                try
                {
                    progressForm.UpdateProgress("Разделение данных на периоды...", 10);

                    var validData = matchedData.Where(d => d.PowerValue.HasValue).ToList();

                    if (validData.Count < 4)
                    {
                        progressForm.Close();
                        MessageBox.Show($"Недостаточно данных с мощностью. Только {validData.Count} точек",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 1. Разделяем данные на отопительные периоды
                    var heatingPeriods = SplitIntoHeatingPeriods(validData);

                    if (heatingPeriods.Count == 0)
                    {
                        var fallbackPeriod = new DataPeriod
                        {
                            Name = "Все данные",
                            StartYear = validData.Min(d => d.DateTime.Year),
                            EndYear = validData.Max(d => d.DateTime.Year),
                            PeriodStart = validData.Min(d => d.DateTime),
                            PeriodEnd = validData.Max(d => d.DateTime),
                            Data = validData
                        };
                        heatingPeriods.Add(fallbackPeriod);
                    }

                    progressForm.UpdateProgress($"Найдено {heatingPeriods.Count} отопительных периода(ов)", 20);

                    // 2. Получаем настройки Фурье
                    string fourierPeriodType = cmbFourierPeriod.SelectedItem?.ToString() ?? "Весь период";
                    double cutoffRatio = (double)nudCutoffRatio.Value;

                    // 3. Обрабатываем каждый период
                    var processedPeriods = ProcessPeriodsWithFourier(heatingPeriods, fourierPeriodType, cutoffRatio, progressForm);

                    if (processingCancelled)
                    {
                        progressForm.Close();
                        MessageBox.Show("Обработка прервана пользователем.", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnCancelProcessing.Visible = false;
                        return;
                    }

                    // Проверяем, что есть обработанные периоды
                    if (processedPeriods.Count == 0)
                    {
                        progressForm.Close();
                        MessageBox.Show("Нет данных для построения графиков",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnCancelProcessing.Visible = false;
                        return;
                    }

                    progressForm.UpdateProgress("Построение графика...", 95);

                    // 4. Показываем форму с графиками
                    var chartForm = new ChartForm(processedPeriods, true);
                    progressForm.UpdateProgress("Готово!", 100);
                    System.Threading.Thread.Sleep(300);

                    progressForm.Close();
                    progressForm = null;
                    btnCancelProcessing.Visible = false;
                    chartForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    progressForm?.Close();
                    System.Diagnostics.Debug.WriteLine($"Ошибка BtnAnalyze_Click: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show($"Ошибка при обработке данных:\n{ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnCancelProcessing.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка BtnAnalyze_Click (внешняя): {ex.Message}\n{ex.StackTrace}");
                btnCancelProcessing.Visible = false;
            }
        }

        private List<DataPeriod> SplitIntoHeatingPeriods(List<HourlyData> allData)
        {
            var periods = new List<DataPeriod>();

            if (allData == null || allData.Count == 0)
                return periods;

            // Находим все годы в данных
            var years = allData.Select(d => d.DateTime.Year).Distinct().OrderBy(y => y).ToList();

            foreach (int year in years)
            {
                int nextYear = year + 1;

                // Собираем данные для отопительного периода: 
                // Октябрь-Декабрь текущего года + Январь-Апрель следующего
                List<HourlyData> periodData = new List<HourlyData>();

                // Текущий год: октябрь-декабрь
                for (int month = 10; month <= 12; month++)
                {
                    var monthData = allData.Where(d =>
                        d.DateTime.Year == year &&
                        d.DateTime.Month == month).ToList();
                    periodData.AddRange(monthData);
                }

                // Следующий год: январь-апрель
                for (int month = 1; month <= 4; month++)
                {
                    var monthData = allData.Where(d =>
                        d.DateTime.Year == nextYear &&
                        d.DateTime.Month == month).ToList();
                    periodData.AddRange(monthData);
                }

                // Если в периоде достаточно данных
                if (periodData.Count >= 24 * 7) // Минимум неделя данных (24*7 часов)
                {
                    periodData = periodData.OrderBy(d => d.DateTime).ToList();

                    DateTime actualStart = periodData.Min(d => d.DateTime);
                    DateTime actualEnd = periodData.Max(d => d.DateTime);

                    var period = new DataPeriod
                    {
                        Name = $"Отопительный период {year}-{nextYear}",
                        StartYear = year,
                        EndYear = nextYear,
                        PeriodStart = actualStart,
                        PeriodEnd = actualEnd,
                        Data = periodData
                    };

                    periods.Add(period);
                }
            }

            // Если не нашли полных отопительных периодов, создаем по годам
            if (periods.Count == 0)
            {
                foreach (int year in years)
                {
                    var yearData = allData.Where(d => d.DateTime.Year == year).ToList();

                    if (yearData.Count >= 24 * 30) // Минимум месяц
                    {
                        // Берем только холодные месяцы: октябрь-апрель
                        var filteredData = yearData.Where(d =>
                            d.DateTime.Month >= 10 || d.DateTime.Month <= 4).ToList();

                        if (filteredData.Count >= 24 * 7) // Минимум неделя
                        {
                            var period = new DataPeriod
                            {
                                Name = $"Год {year} (холодные месяцы)",
                                StartYear = year,
                                EndYear = year,
                                PeriodStart = filteredData.Min(d => d.DateTime),
                                PeriodEnd = filteredData.Max(d => d.DateTime),
                                Data = filteredData.OrderBy(d => d.DateTime).ToList()
                            };

                            periods.Add(period);
                        }
                    }
                }
            }

            return periods;
        }

        private List<DataPeriod> SplitPeriodByType(DataPeriod period, string periodType)
        {
            var subPeriods = new List<DataPeriod>();

            switch (periodType)
            {
                case "День":
                    var dailyGroups = period.Data
                        .GroupBy(d => d.DateTime.Date)
                        .Where(g => g.Count() >= 4)
                        .OrderBy(g => g.Key);

                    foreach (var dayGroup in dailyGroups)
                    {
                        subPeriods.Add(new DataPeriod
                        {
                            Name = $"День {dayGroup.Key:dd.MM.yyyy}",
                            PeriodStart = dayGroup.Key,
                            PeriodEnd = dayGroup.Key.AddHours(23).AddMinutes(59),
                            Data = dayGroup.ToList()
                        });
                    }
                    break;

                case "Неделя":
                    var weeklyGroups = period.Data
                        .GroupBy(d =>
                        {
                            var diff = (d.DateTime - new DateTime(d.DateTime.Year, 1, 1)).Days;
                            return diff / 7;
                        })
                        .Where(g => g.Count() >= 4 * 7)
                        .OrderBy(g => g.Key);

                    int weekNum = 1;
                    foreach (var weekGroup in weeklyGroups)
                    {
                        var weekData = weekGroup.OrderBy(d => d.DateTime).ToList();
                        subPeriods.Add(new DataPeriod
                        {
                            Name = $"Неделя {weekNum}",
                            PeriodStart = weekData.Min(d => d.DateTime),
                            PeriodEnd = weekData.Max(d => d.DateTime),
                            Data = weekData
                        });
                        weekNum++;
                    }
                    break;

                case "Месяц":
                    var monthlyGroups = period.Data
                        .GroupBy(d => new { d.DateTime.Year, d.DateTime.Month })
                        .Where(g => g.Count() >= 4 * 15)
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Month);

                    foreach (var monthGroup in monthlyGroups)
                    {
                        var monthData = monthGroup.OrderBy(d => d.DateTime).ToList();
                        subPeriods.Add(new DataPeriod
                        {
                            Name = $"{monthGroup.Key.Year}-{monthGroup.Key.Month:00}",
                            PeriodStart = monthData.Min(d => d.DateTime),
                            PeriodEnd = monthData.Max(d => d.DateTime),
                            Data = monthData
                        });
                    }
                    break;

                case "Весь период":
                default:
                    subPeriods.Add(period);
                    break;
            }

            return subPeriods;
        }

        private List<HourlyData> ProcessSubPeriodWithFourier(DataPeriod subPeriod, string periodType, double cutoffRatio)
        {
            var periodPowerData = subPeriod.Data.Select(d => new HourlyData
            {
                DateTime = d.DateTime,
                Hour = d.Hour,
                Value = d.PowerValue.Value
            }).ToList();

            var periodTempData = subPeriod.Data.Select(d => new HourlyData
            {
                DateTime = d.DateTime,
                Hour = d.Hour,
                Value = d.Value
            }).ToList();

            // Применяем Фурье-фильтрацию
            var filteredPowerData = FourierProcessor.ProcessPowerData(periodPowerData, cutoffRatio);
            var filteredTempData = FourierProcessor.ProcessTemperatureData(periodTempData, cutoffRatio);

            // Сопоставляем данные
            var filteredMatchedData = new List<HourlyData>();
            for (int i = 0; i < periodPowerData.Count; i++)
            {
                filteredMatchedData.Add(new HourlyData
                {
                    DateTime = periodPowerData[i].DateTime,
                    Hour = periodPowerData[i].Hour,
                    Value = filteredTempData[i].Value,
                    PowerValue = filteredPowerData[i].Value
                });
            }

            return filteredMatchedData;
        }

        private double GetCutoffRatioForPeriodType(string periodType)
        {
            switch (periodType)
            {
                case "День": return 0.3;      // Более сильная фильтрация для дня
                case "Неделя": return 0.2;    // Средняя фильтрация для недели
                case "Месяц": return 0.1;     // Меньшая фильтрация для месяца
                case "Весь период": return 0.05; // Минимальная фильтрация для всего периода
                default: return 0.1;
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