using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WindowsFormsApp1;

namespace ClassLibrary1
{
    public static class FourierProcessor
    {
        private static void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[Фурье] {DateTime.Now:HH:mm:ss.fff}: {message}");
        }

        /// <summary>
        /// Фильтр низких частот: оставляет постоянную составляющую и малые гармоники
        /// cutoffRatio - какая часть гармоник сохраняется (0.1 = 10%)
        /// </summary>
        public static List<double> KeepLowFrequencies(List<double> data, double cutoffRatio = 0.1)
        {
            if (data == null || data.Count < 4)
            {
                Log($"Слишком мало данных: {data?.Count ?? 0}");
                return data ?? new List<double>();
            }

            try
            {
                Log($"Начало фильтрации НЧ: {data.Count} точек, cutoffRatio={cutoffRatio}");

                // 1. Нормализуем данные (убираем среднее)
                double mean = data.Average();
                var normalizedData = data.Select(d => d - mean).ToList();

                Log($"Среднее значение: {mean:F2}");

                // 2. Находим ближайшую степень двойки
                int n = normalizedData.Count;
                int size = 1;
                while (size < n)
                    size <<= 1;

                Log($"Оригинальная длина: {n}, Размер для FFT: {size}");

                // 3. Дополняем нулями
                var paddedData = new List<double>(normalizedData);
                if (size > n)
                {
                    paddedData.AddRange(Enumerable.Repeat(0.0, size - n));
                }

                // 4. Преобразуем в комплексные числа
                var complexData = paddedData.Select(d => new System.Numerics.Complex(d, 0)).ToArray();

                // 5. Прямое преобразование Фурье
                Fourier.Forward(complexData, FourierOptions.Default);

                // 6. Определяем сколько гармоник оставить
                int totalHarmonics = size / 2; // Действительные гармоники (без комплексно-сопряженных)
                int harmonicsToKeep = Math.Max(1, (int)(totalHarmonics * cutoffRatio));

                Log($"Всего гармоник: {totalHarmonics}, Оставляем: {harmonicsToKeep}");

                // 7. Фильтрация: обнуляем высокие частоты
                // Сохраняем:
                // - постоянную составляющую (индекс 0)
                // - низкие гармоники (индексы 1 до harmonicsToKeep)
                // - их комплексно-сопряженные пары (индексы size-harmonicsToKeep до size-1)

                int preservedIndex = harmonicsToKeep + 1; // +1 для индекса 0

                for (int i = preservedIndex; i < size - harmonicsToKeep; i++)
                {
                    complexData[i] = System.Numerics.Complex.Zero;
                }

                Log($"Обнулено {size - 2 * harmonicsToKeep - 1} из {size} частот");

                // 8. Обратное преобразование Фурье
                Fourier.Inverse(complexData, FourierOptions.Default);

                // 9. Восстанавливаем среднее значение
                var result = new List<double>();

                // БЕЗОПАСНЫЙ ДОСТУП: берем минимум из n и complexData.Length
                int safeLength = Math.Min(n, complexData.Length);
                for (int i = 0; i < safeLength; i++)
                {
                    result.Add(complexData[i].Real + mean);
                }

                // Если что-то пошло не так, дополняем оставшиеся точки
                if (result.Count < n)
                {
                    Log($"Предупреждение: результат короче. Дополняем...");
                    for (int i = result.Count; i < n; i++)
                    {
                        result.Add(data[i]); // Берем из оригинальных данных
                    }
                }

                Log($"Фильтрация завершена: {result.Count} точек");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Ошибка фильтрации НЧ: {ex.Message}");
                return data;
            }
        }

        /// <summary>
        /// Обработка мощности с фильтром низких частот
        /// </summary>
        public static List<HourlyData> ProcessPowerData(List<HourlyData> powerData, double cutoffRatio = 0.1)
        {
            if (powerData == null || powerData.Count < 4)
            {
                Log($"Недостаточно данных мощности: {powerData?.Count ?? 0}");
                return powerData ?? new List<HourlyData>();
            }

            try
            {
                Log($"Обработка мощности НЧ-фильтром: {powerData.Count} точек");

                // Извлекаем значения
                var powerValues = powerData.Select(d => d.Value).ToList();

                // Применяем фильтр
                var filteredValues = KeepLowFrequencies(powerValues, cutoffRatio);

                // Создаем результат с ГАРАНТИЕЙ одинаковой длины
                var result = new List<HourlyData>();

                // Берем минимум из двух длин
                int minCount = Math.Min(powerData.Count, filteredValues.Count);

                for (int i = 0; i < minCount; i++)
                {
                    result.Add(new HourlyData
                    {
                        DateTime = powerData[i].DateTime,
                        Hour = powerData[i].Hour,
                        Value = Math.Round(filteredValues[i], 2)
                    });
                }

                // Если отфильтрованных данных меньше, дополняем
                if (result.Count < powerData.Count)
                {
                    Log($"Дополняем результат мощности: {result.Count} -> {powerData.Count}");
                    for (int i = result.Count; i < powerData.Count; i++)
                    {
                        result.Add(new HourlyData
                        {
                            DateTime = powerData[i].DateTime,
                            Hour = powerData[i].Hour,
                            Value = Math.Round(powerData[i].Value, 2)
                        });
                    }
                }

                Log($"Обработка мощности завершена: {result.Count} точек");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Ошибка обработки мощности: {ex.Message}");
                return powerData;
            }
        }

        /// <summary>
        /// Обработка температуры с фильтром низких частот
        /// </summary>
        public static List<HourlyData> ProcessTemperatureData(List<HourlyData> tempData, double cutoffRatio = 0.1)
        {
            if (tempData == null || tempData.Count < 4)
            {
                Log($"Недостаточно данных температуры: {tempData?.Count ?? 0}");
                return tempData ?? new List<HourlyData>();
            }

            try
            {
                Log($"Обработка температуры НЧ-фильтром: {tempData.Count} точек");

                // Извлекаем значения
                var tempValues = tempData.Select(d => d.Value).ToList();

                // Применяем фильтр
                var filteredValues = KeepLowFrequencies(tempValues, cutoffRatio);

                // Создаем результат с ГАРАНТИЕЙ одинаковой длины
                var result = new List<HourlyData>();

                // Берем минимум из двух длин
                int minCount = Math.Min(tempData.Count, filteredValues.Count);

                for (int i = 0; i < minCount; i++)
                {
                    result.Add(new HourlyData
                    {
                        DateTime = tempData[i].DateTime,
                        Hour = tempData[i].Hour,
                        Value = Math.Round(filteredValues[i], 1)
                    });
                }

                // Если отфильтрованных данных меньше, дополняем
                if (result.Count < tempData.Count)
                {
                    Log($"Дополняем результат температуры: {result.Count} -> {tempData.Count}");
                    for (int i = result.Count; i < tempData.Count; i++)
                    {
                        result.Add(new HourlyData
                        {
                            DateTime = tempData[i].DateTime,
                            Hour = tempData[i].Hour,
                            Value = Math.Round(tempData[i].Value, 1)
                        });
                    }
                }

                Log($"Обработка температуры завершена: {result.Count} точек");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Ошибка обработки температуры: {ex.Message}");
                return tempData;
            }
        }

        // НОВЫЕ МЕТОДЫ С ПОДДЕРЖКОЙ ПРОГРЕССА

        /// <summary>
        /// Обработка мощности с фильтром низких частот с поддержкой прогресса
        /// </summary>
        public static List<HourlyData> ProcessPowerDataWithProgress(
            List<HourlyData> powerData,
            double cutoffRatio,
            IProgressReporter progressReporter,
            CancellationTokenSource cts)
        {
            if (powerData == null || powerData.Count < 4)
            {
                Log($"Недостаточно данных мощности: {powerData?.Count ?? 0}");
                return powerData ?? new List<HourlyData>();
            }

            try
            {
                Log($"Обработка мощности НЧ-фильтром с прогрессом: {powerData.Count} точек");

                progressReporter?.UpdateProgress($"Обработка мощности: извлечение данных...", 10);

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                {
                    Log("Обработка отменена пользователем");
                    return null;
                }

                // Извлекаем значения
                var powerValues = powerData.Select(d => d.Value).ToList();

                progressReporter?.UpdateProgress($"Обработка мощности: Фурье-преобразование...", 30);

                // Применяем фильтр с поддержкой прогресса
                var filteredValues = KeepLowFrequenciesWithProgress(powerValues, cutoffRatio,
                    progressReporter, cts, "мощности");

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested || filteredValues == null)
                {
                    return null;
                }

                progressReporter?.UpdateProgress($"Обработка мощности: создание результата...", 90);

                // Создаем результат с ГАРАНТИЕЙ одинаковой длины
                var result = new List<HourlyData>();

                // Берем минимум из двух длин
                int minCount = Math.Min(powerData.Count, filteredValues.Count);

                for (int i = 0; i < minCount; i++)
                {
                    result.Add(new HourlyData
                    {
                        DateTime = powerData[i].DateTime,
                        Hour = powerData[i].Hour,
                        Value = Math.Round(filteredValues[i], 2)
                    });

                    // Обновляем прогресс каждые 1000 точек
                    if (i % 1000 == 0 && progressReporter != null)
                    {
                        int progress = 90 + (i * 10) / Math.Max(1, minCount);
                        progressReporter.UpdateProgress($"Обработка мощности: точка {i + 1} из {minCount}",
                            Math.Min(progress, 99));
                    }
                }

                // Если отфильтрованных данных меньше, дополняем
                if (result.Count < powerData.Count)
                {
                    Log($"Дополняем результат мощности: {result.Count} -> {powerData.Count}");
                    for (int i = result.Count; i < powerData.Count; i++)
                    {
                        result.Add(new HourlyData
                        {
                            DateTime = powerData[i].DateTime,
                            Hour = powerData[i].Hour,
                            Value = Math.Round(powerData[i].Value, 2)
                        });
                    }
                }

                progressReporter?.UpdateProgress($"Обработка мощности завершена", 100);

                Log($"Обработка мощности завершена: {result.Count} точек");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Ошибка обработки мощности: {ex.Message}");
                return powerData;
            }
        }

        /// <summary>
        /// Обработка температуры с фильтром низких частот с поддержкой прогресса
        /// </summary>
        public static List<HourlyData> ProcessTemperatureDataWithProgress(
            List<HourlyData> tempData,
            double cutoffRatio,
            IProgressReporter progressReporter,
            CancellationTokenSource cts)
        {
            if (tempData == null || tempData.Count < 4)
            {
                Log($"Недостаточно данных температуры: {tempData?.Count ?? 0}");
                return tempData ?? new List<HourlyData>();
            }

            try
            {
                Log($"Обработка температуры НЧ-фильтром с прогрессом: {tempData.Count} точек");

                progressReporter?.UpdateProgress($"Обработка температуры: извлечение данных...", 10);

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                {
                    Log("Обработка отменена пользователем");
                    return null;
                }

                // Извлекаем значения
                var tempValues = tempData.Select(d => d.Value).ToList();

                progressReporter?.UpdateProgress($"Обработка температуры: Фурье-преобразование...", 30);

                // Применяем фильтр с поддержкой прогресса
                var filteredValues = KeepLowFrequenciesWithProgress(tempValues, cutoffRatio,
                    progressReporter, cts, "температуры");

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested || filteredValues == null)
                {
                    return null;
                }

                progressReporter?.UpdateProgress($"Обработка температуры: создание результата...", 90);

                // Создаем результат с ГАРАНТИЕЙ одинаковой длины
                var result = new List<HourlyData>();

                // Берем минимум из двух длин
                int minCount = Math.Min(tempData.Count, filteredValues.Count);

                for (int i = 0; i < minCount; i++)
                {
                    result.Add(new HourlyData
                    {
                        DateTime = tempData[i].DateTime,
                        Hour = tempData[i].Hour,
                        Value = Math.Round(filteredValues[i], 1)
                    });

                    // Обновляем прогресс каждые 1000 точек
                    if (i % 1000 == 0 && progressReporter != null)
                    {
                        int progress = 90 + (i * 10) / Math.Max(1, minCount);
                        progressReporter.UpdateProgress($"Обработка температуры: точка {i + 1} из {minCount}",
                            Math.Min(progress, 99));
                    }
                }

                // Если отфильтрованных данных меньше, дополняем
                if (result.Count < tempData.Count)
                {
                    Log($"Дополняем результат температуры: {result.Count} -> {tempData.Count}");
                    for (int i = result.Count; i < tempData.Count; i++)
                    {
                        result.Add(new HourlyData
                        {
                            DateTime = tempData[i].DateTime,
                            Hour = tempData[i].Hour,
                            Value = Math.Round(tempData[i].Value, 1)
                        });
                    }
                }

                progressReporter?.UpdateProgress($"Обработка температуры завершена", 100);

                Log($"Обработка температуры завершена: {result.Count} точек");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Ошибка обработки температуры: {ex.Message}");
                return tempData;
            }
        }

        /// <summary>
        /// Фильтр низких частот с поддержкой прогресса
        /// </summary>
        private static List<double> KeepLowFrequenciesWithProgress(
            List<double> data,
            double cutoffRatio,
            IProgressReporter progressReporter,
            CancellationTokenSource cts,
            string dataType = "данных")
        {
            if (data == null || data.Count < 4)
            {
                Log($"Слишком мало данных: {data?.Count ?? 0}");
                return data ?? new List<double>();
            }

            try
            {
                Log($"Начало фильтрации НЧ с прогрессом: {data.Count} точек, тип: {dataType}");

                progressReporter?.UpdateProgress($"Фильтрация {dataType}: нормализация...", 10);

                // 1. Нормализуем данные (убираем среднее)
                double mean = data.Average();
                var normalizedData = data.Select(d => d - mean).ToList();

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                    return null;

                progressReporter?.UpdateProgress($"Фильтрация {dataType}: подготовка FFT...", 20);

                // 2. Находим ближайшую степень двойки
                int n = normalizedData.Count;
                int size = 1;
                while (size < n)
                    size <<= 1;

                // 3. Дополняем нулями
                var paddedData = new List<double>(normalizedData);
                if (size > n)
                {
                    paddedData.AddRange(Enumerable.Repeat(0.0, size - n));
                }

                // 4. Преобразуем в комплексные числа
                var complexData = paddedData.Select(d => new System.Numerics.Complex(d, 0)).ToArray();

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                    return null;

                progressReporter?.UpdateProgress($"Фильтрация {dataType}: прямое Фурье-преобразование...", 40);

                // 5. Прямое преобразование Фурье
                Fourier.Forward(complexData, FourierOptions.Default);

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                    return null;

                progressReporter?.UpdateProgress($"Фильтрация {dataType}: фильтрация частот...", 60);

                // 6. Определяем сколько гармоник оставить
                int totalHarmonics = size / 2;
                int harmonicsToKeep = Math.Max(1, (int)(totalHarmonics * cutoffRatio));

                // 7. Фильтрация: обнуляем высокие частоты
                int preservedIndex = harmonicsToKeep + 1;

                for (int i = preservedIndex; i < size - harmonicsToKeep; i++)
                {
                    complexData[i] = System.Numerics.Complex.Zero;
                }

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                    return null;

                progressReporter?.UpdateProgress($"Фильтрация {dataType}: обратное Фурье-преобразование...", 80);

                // 8. Обратное преобразование Фурье
                Fourier.Inverse(complexData, FourierOptions.Default);

                // Проверяем отмену
                if (cts != null && cts.Token.IsCancellationRequested)
                    return null;

                progressReporter?.UpdateProgress($"Фильтрация {dataType}: восстановление данных...", 90);

                // 9. Восстанавливаем среднее значение
                var result = new List<double>();

                // БЕЗОПАСНЫЙ ДОСТУП: берем минимум из n и complexData.Length
                int safeLength = Math.Min(n, complexData.Length);
                for (int i = 0; i < safeLength; i++)
                {
                    result.Add(complexData[i].Real + mean);
                }

                // Если что-то пошло не так, дополняем оставшиеся точки
                if (result.Count < n)
                {
                    Log($"Предупреждение: результат короче. Дополняем...");
                    for (int i = result.Count; i < n; i++)
                    {
                        result.Add(data[i]); // Берем из оригинальных данных
                    }
                }

                progressReporter?.UpdateProgress($"Фильтрация {dataType} завершена", 100);

                Log($"Фильтрация завершена: {result.Count} точек");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Ошибка фильтрации НЧ: {ex.Message}");
                return data;
            }
        }
    }
}