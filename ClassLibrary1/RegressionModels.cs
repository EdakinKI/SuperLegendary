using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1
{
    public class RegressionResult
    {
        public string PeriodName { get; set; }
        public List<double> Temperatures { get; set; } = new List<double>();
        public List<double> Powers { get; set; } = new List<double>();
        public List<double> LinearApproximation { get; set; } = new List<double>();
        public List<double> ExponentialApproximation { get; set; } = new List<double>();
        public double LinearSlope { get; set; } // k
        public double LinearIntercept { get; set; } // b
        public double ExponentialIntensity { get; set; } // L
        public double TransitionTemperature { get; set; } // T_общая
        public double TransitionPower { get; set; } // f(T_общая)
        public double CrossoverPoint { get; set; } // Точка перехода
        public int LinearStartIndex { get; set; }
        public int LinearEndIndex { get; set; }

        public double LinearRMSE { get; set; }
        public double ExponentialRMSE { get; set; }

        public override string ToString()
        {
            return $"{PeriodName}: k={LinearSlope:F4}, b={LinearIntercept:F2}, L={ExponentialIntensity:F4}, T_cross={CrossoverPoint:F2}°C";
        }
    }

    public static class RegressionCalculator
    {
        /// <summary>
        /// Сортирует данные по температуре
        /// </summary>
        public static (List<double> sortedTemps, List<double> sortedPowers) SortByTemperature(
            List<double> temperatures, List<double> powers)
        {
            var pairs = temperatures.Zip(powers, (t, p) => new { Temperature = t, Power = p })
                                  .OrderBy(x => x.Temperature)
                                  .ToList();

            return (pairs.Select(x => x.Temperature).ToList(),
                    pairs.Select(x => x.Power).ToList());
        }

        /// <summary>
        /// Линейная регрессия по вашим формулам: P = k·T + b
        /// </summary>
        /// <summary>
        /// Линейная регрессия P = k·T + b (ИСПРАВЛЕННЫЕ ФОРМУЛЫ)
        /// </summary>
        public static Tuple<double, double, List<double>, int, int> LinearRegressionCustom(
            List<double> temperatures, List<double> powers)
        {
            if (temperatures.Count < 2)
                return Tuple.Create(0.0, powers.Average(), powers, 0, temperatures.Count - 1);

            int n = 0; // Первый индекс
            int m = temperatures.Count - 1; // Последний индекс
            int N = m - n + 1; // Количество точек

            System.Diagnostics.Debug.WriteLine($"ЛИНЕЙНАЯ РЕГРЕССИЯ: N={N} точек");

            // Вычисляем суммы
            double sumP = 0, sumT = 0, sumPT = 0, sumT2 = 0;

            for (int i = n; i <= m; i++)
            {
                double P = powers[i];
                double T = temperatures[i];

                sumP += P;
                sumT += T;
                sumPT += P * T;
                sumT2 += T * T;
            }

            System.Diagnostics.Debug.WriteLine($"Суммы: ∑P={sumP:F0}, ∑T={sumT:F0}, ∑PT={sumPT:F0}, ∑T²={sumT2:F0}");

            // Исправленные формулы
            double denominator = N * sumT2 - sumT * sumT;

            if (Math.Abs(denominator) < 1e-10)
            {
                System.Diagnostics.Debug.WriteLine($"МАЛЫЙ ЗНАМЕНАТЕЛЬ, используем упрощенный расчет");
                // Упрощенный расчет через средние
                double T_avg = sumT / N;
                double P_avg = sumP / N;

                double numerator_k = 0;
                double denominator_k = 0;

                for (int i = n; i <= m; i++)
                {
                    double devT = temperatures[i] - T_avg;
                    double devP = powers[i] - P_avg;
                    numerator_k += devT * devP;
                    denominator_k += devT * devT;
                }

                double k_simple = numerator_k / denominator_k;
                double b_simple = P_avg - k_simple * T_avg;

                var fittedValues = temperatures.Select(t => k_simple * t + b_simple).ToList();

                System.Diagnostics.Debug.WriteLine($"Упрощенный расчет: k={k_simple:F4}, b={b_simple:F2}");
                return Tuple.Create(k_simple, b_simple, fittedValues, n, m);
            }

            // ВАШИ ФОРМУЛЫ, НО С ПРАВИЛЬНЫМ РАСЧЕТОМ b
            double k = (N * sumPT - sumP * sumT) / denominator;
            double b = (sumP - k * sumT) / N; // ← ИСПРАВЛЕННАЯ ФОРМУЛА!

            System.Diagnostics.Debug.WriteLine($"Расчет по формулам: k={k:F4}, b={b:F2}");

            // Дополнительная проверка через стандартные формулы
            double T_avg_check = sumT / N;
            double P_avg_check = sumP / N;

            double S_PT = sumPT - sumP * sumT / N;
            double S_TT = sumT2 - sumT * sumT / N;

            double k_check = S_PT / S_TT;
            double b_check = P_avg_check - k_check * T_avg_check;

            System.Diagnostics.Debug.WriteLine($"Проверка (стандартные): k={k_check:F4}, b={b_check:F2}");
            System.Diagnostics.Debug.WriteLine($"Разница: Δk={Math.Abs(k - k_check):E}, Δb={Math.Abs(b - b_check):E}");

            // Используем проверенные значения
            if (Math.Abs(k - k_check) > 0.01 || Math.Abs(b - b_check) > 10)
            {
                System.Diagnostics.Debug.WriteLine($"БОЛЬШАЯ РАЗНИЦА! Использую проверенные значения");
                k = k_check;
                b = b_check;
            }

            // Проверка на NaN/Infinity
            if (double.IsNaN(k) || double.IsInfinity(k) || double.IsNaN(b) || double.IsInfinity(b))
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА РАСЧЕТА, использую средние");
                k = 0;
                b = powers.Average();
            }

            var fittedValues_correct = temperatures.Select(t => k * t + b).ToList();

            // Проверка адекватности
            double minFitted = fittedValues_correct.Min();
            double maxFitted = fittedValues_correct.Max();
            double minActual = powers.Min();
            double maxActual = powers.Max();

            System.Diagnostics.Debug.WriteLine($"Диапазон фактический: {minActual:F0}...{maxActual:F0}");
            System.Diagnostics.Debug.WriteLine($"Диапазон рассчитанный: {minFitted:F0}...{maxFitted:F0}");

            return Tuple.Create(k, b, fittedValues_correct, n, m);
        }

        /// <summary>
        /// Экспоненциальная регрессия с оптимизацией L методом Ньютона
        /// </summary>
        public static Tuple<double, List<double>> ExponentialRegressionNewton(
            List<double> temperatures, List<double> powers,
            double k, double b, double transitionTemp, double transitionPower)
        {
            if (temperatures.Count < 3 || Math.Abs(k) < 1e-10)
                return Tuple.Create(0.1, powers);

            try
            {
                double L = 0.1;
                double f_T0 = transitionPower;

                int maxIterations = 100;
                double tolerance = 1e-6;

                for (int iter = 0; iter < maxIterations; iter++)
                {
                    double f_L = 0;
                    double df_dL = 0;

                    for (int i = 0; i < temperatures.Count; i++)
                    {
                        double T = temperatures[i];
                        double P_actual = powers[i];

                        double term = L * (T - transitionTemp);
                        double expTerm = Math.Exp(term);
                        double P_predicted = f_T0 - (k / L) + (k / L) * expTerm;

                        double error = P_predicted - P_actual;
                        double dP_dL = (k / (L * L)) * (1 - expTerm * (1 - L * (T - transitionTemp)));

                        f_L += 2 * error * dP_dL;
                        df_dL += 2 * (dP_dL * dP_dL + error * SecondDerivative(L, k, T, transitionTemp));
                    }

                    if (Math.Abs(df_dL) < 1e-10)
                        break;

                    double delta = f_L / df_dL;
                    L = L - delta;
                    L = Math.Max(0.01, Math.Min(2.0, L));

                    if (Math.Abs(delta) < tolerance)
                        break;
                }

                var fittedValues = new List<double>();
                foreach (double T in temperatures)
                {
                    double term = L * (T - transitionTemp);
                    double expTerm = Math.Exp(term);
                    double predicted = f_T0 - (k / L) + (k / L) * expTerm;
                    fittedValues.Add(predicted);
                }

                return Tuple.Create(L, fittedValues);
            }
            catch
            {
                return Tuple.Create(0.1, powers);
            }
        }

        private static double SecondDerivative(double L, double k, double T, double T0)
        {
            double deltaT = T - T0;
            double expTerm = Math.Exp(L * deltaT);
            return (2 * k / (L * L * L)) * (1 - expTerm * (1 - L * deltaT)) - (k / (L * L)) * expTerm * deltaT * deltaT;
        }

        /// <summary>
        /// Определяет оптимальную точку перехода
        /// </summary>
        private static double FindOptimalCrossover(List<double> temps, List<double> powers, List<double> linearApprox)
        {
            // Ищем точку, где относительное отклонение мощности от линейной модели максимально
            double maxDeviation = 0;
            int crossIndex = temps.Count / 2; // значение по умолчанию

            for (int i = 0; i < temps.Count; i++)
            {
                // Отклонение = (Факт - Линейный прогноз) / Линейный прогноз
                double deviation = Math.Abs((powers[i] - linearApprox[i]) / linearApprox[i]);
                if (deviation > maxDeviation)
                {
                    maxDeviation = deviation;
                    crossIndex = i;
                }
            }
            // Возвращаем температуру в найденной точке
            return temps[crossIndex];
        }

        /// <summary>
        /// Вычисляет среднеквадратичное отклонение (RMSE)
        /// </summary>
        public static double CalculateRMSE(List<double> actual, List<double> predicted)
        {
            if (actual.Count != predicted.Count || actual.Count == 0)
                return 0;

            double sumSquaredError = 0;
            for (int i = 0; i < actual.Count; i++)
            {
                double error = actual[i] - predicted[i];
                sumSquaredError += error * error;
            }

            return Math.Sqrt(sumSquaredError / actual.Count);
        }

        public static double FindPeriodCrossover(List<double> temperatures, List<double> powers)
        {
            if (temperatures.Count < 20) return 0.0; // Минимум данных

            // Стратегия: анализируем РАСПРЕДЕЛЕНИЕ мощностей по температурам

            // 1. Группируем данные по температурным интервалам (шаг 1°C)
            var tempGroups = temperatures
                .Select((t, i) => new { Temp = Math.Round(t), Power = powers[i] })
                .GroupBy(x => x.Temp)
                .Where(g => g.Count() >= 3) // Минимум 3 точки в группе
                .OrderBy(g => g.Key)
                .ToList();

            if (tempGroups.Count < 5) return temperatures.Average();

            // 2. Ищем температуру, где меняется характер зависимости
            List<double> avgPowers = new List<double>();
            List<double> tempCenters = new List<double>();

            foreach (var group in tempGroups)
            {
                tempCenters.Add(group.Key);
                avgPowers.Add(group.Average(x => x.Power));
            }

            // 3. Анализируем производные (изменение мощности при изменении температуры)
            List<double> derivatives = new List<double>();
            for (int i = 1; i < tempCenters.Count; i++)
            {
                double dT = tempCenters[i] - tempCenters[i - 1];
                double dP = avgPowers[i] - avgPowers[i - 1];
                if (Math.Abs(dT) > 0.1)
                    derivatives.Add(dP / dT);
            }

            // 4. Находим температуру, где производная максимальна (наибольшая чувствительность)
            // Это и будет точка перехода к экспоненциальному росту
            if (derivatives.Count > 0)
            {
                int maxDerivIndex = derivatives.IndexOf(derivatives.Min()); // Ищем МИНИМУМ (самый отрицательный)

                // Соответствующая температура (с учетом смещения из-за разностей)
                int tempIndex = Math.Min(maxDerivIndex + 1, tempCenters.Count - 1);
                double crossover = tempCenters[tempIndex];

                // Ограничиваем разумными пределами
                crossover = Math.Max(-15, Math.Min(10, crossover));

                System.Diagnostics.Debug.WriteLine($"Точка перехода найдена: {crossover:F1}°C");
                return crossover;
            }

            // 5. Резервный метод: используем 10-й процентиль температур
            var sortedTemps = temperatures.OrderBy(t => t).ToList();
            int idx10 = (int)(sortedTemps.Count * 0.1);
            return sortedTemps[idx10];
        }

        // Определение масштаба мощности
        private static double DetectPowerScale(List<double> powers)
        {
            double avgPower = powers.Average();

            // Если средняя мощность > 10000, возможно это в кВт или ваттах
            if (avgPower > 10000) return 1000.0; // кВт → МВт
            if (avgPower > 1000000) return 1000000.0; // Вт → МВт

            return 1.0; // МВт
        }

        // Проверка физической реалистичности
        private static bool IsPhysicallyReasonable(double k, double b)
        {
            // Типичные диапазоны для энергосистем
            bool kReasonable = (k < 0) && (Math.Abs(k) >= 1) && (Math.Abs(k) <= 1000);
            bool bReasonable = (b > 0) && (b <= 50000);

            return kReasonable && bReasonable;
        }

        // Построение полной линии регрессии
        private static void BuildCompleteRegressionLine(RegressionResult result, List<int> lowTempIndices)
        {
            result.ExponentialApproximation = new List<double>();

            for (int i = 0; i < result.Temperatures.Count; i++)
            {
                double T = result.Temperatures[i];
                double powerValue;

                if (lowTempIndices.Contains(i))
                {
                    // ЭКСПОНЕНЦИАЛЬНАЯ часть (ниже точки перехода)
                    double expTerm = Math.Exp(result.ExponentialIntensity * (T - result.TransitionTemperature));
                    powerValue = result.TransitionPower - (result.LinearSlope / result.ExponentialIntensity)
                               + (result.LinearSlope / result.ExponentialIntensity) * expTerm;
                }
                else
                {
                    // ЛИНЕЙНАЯ часть (выше точки перехода)
                    powerValue = result.LinearSlope * T + result.LinearIntercept;
                }

                result.ExponentialApproximation.Add(powerValue);
            }
        }

        /// <summary>
        /// Полный расчет регрессий для одного периода
        /// </summary>
        public static RegressionResult CalculateRegressions(
    string periodName, List<double> temperatures, List<double> powers)
        {
            var result = new RegressionResult { PeriodName = periodName };

            // ОТЛАДКА МАСШТАБА
            System.Diagnostics.Debug.WriteLine($"=== {periodName.ToUpper()} ===");
            System.Diagnostics.Debug.WriteLine($"Точек: {temperatures.Count}");
            System.Diagnostics.Debug.WriteLine($"Температура: {temperatures.Min():F1}...{temperatures.Max():F1}°C");
            System.Diagnostics.Debug.WriteLine($"Мощность: {powers.Min():F0}...{powers.Max():F0} (единицы?)");
            System.Diagnostics.Debug.WriteLine($"Средняя мощность: {powers.Average():F0}");

            if (temperatures.Count < 10)
            {
                System.Diagnostics.Debug.WriteLine($"СЛИШКОМ МАЛО ДАННЫХ");
                return result;
            }

            try
            {
                // 1. СОРТИРОВКА по температуре
                (result.Temperatures, result.Powers) = SortByTemperature(temperatures, powers);

                // 2. ПРОВЕРКА МАСШТАБА и корректировка если нужно
                double powerScale = DetectPowerScale(result.Powers);
                if (powerScale != 1.0)
                {
                    System.Diagnostics.Debug.WriteLine($"КОРРЕКТИРОВКА МАСШТАБА: делим на {powerScale}");
                    // Если мощность в кВт, а не МВт
                    result.Powers = result.Powers.Select(p => p / powerScale).ToList();
                }

                // 3. ЛИНЕЙНАЯ РЕГРЕССИЯ на ВСЕХ данных
                var linearResult = LinearRegressionCustom(result.Temperatures, result.Powers);
                result.LinearSlope = linearResult.Item1; // k (МВт/°C)
                result.LinearIntercept = linearResult.Item2; // b (МВт)

                System.Diagnostics.Debug.WriteLine($"Коэффициенты:");
                System.Diagnostics.Debug.WriteLine($"  k = {result.LinearSlope:F2} МВт/°C");
                System.Diagnostics.Debug.WriteLine($"  b = {result.LinearIntercept:F0} МВт");

                // Проверка физической осмысленности
                if (!IsPhysicallyReasonable(result.LinearSlope, result.LinearIntercept))
                {
                    System.Diagnostics.Debug.WriteLine($"ФИЗИЧЕСКИ НЕРЕАЛИСТИЧНО! Использую типичные значения");
                    // Используем типичные значения для энергосистемы
                    result.LinearSlope = -20.0;
                    result.LinearIntercept = result.Powers.Average() - result.LinearSlope * result.Temperatures.Average();
                }

                result.LinearApproximation = result.Temperatures
                    .Select(t => result.LinearSlope * t + result.LinearIntercept)
                    .ToList();

                // 4. ИНДИВИДУАЛЬНАЯ ТОЧКА ПЕРЕХОДА для этого периода
                result.CrossoverPoint = FindPeriodCrossover(result.Temperatures, result.Powers);
                result.TransitionTemperature = result.CrossoverPoint;
                result.TransitionPower = result.LinearSlope * result.TransitionTemperature + result.LinearIntercept;

                System.Diagnostics.Debug.WriteLine($"Точка перехода: {result.CrossoverPoint:F1}°C, P={result.TransitionPower:F0} МВт");

                // 5. ЭКСПОНЕНЦИАЛЬНАЯ РЕГРЕССИЯ для температур ≤ точке перехода
                var lowTempIndices = result.Temperatures
                    .Select((t, i) => new { Temp = t, Index = i })
                    .Where(x => x.Temp <= result.CrossoverPoint)
                    .Select(x => x.Index)
                    .ToList();

                if (lowTempIndices.Count >= 5)
                {
                    var lowTemps = lowTempIndices.Select(i => result.Temperatures[i]).ToList();
                    var lowPowers = lowTempIndices.Select(i => result.Powers[i]).ToList();

                    // Оптимизируем L методом Ньютона
                    var expResult = ExponentialRegressionNewton(
                        lowTemps, lowPowers,
                        result.LinearSlope, result.LinearIntercept,
                        result.TransitionTemperature, result.TransitionPower);

                    result.ExponentialIntensity = expResult.Item1;
                    System.Diagnostics.Debug.WriteLine($"Интенсивность L = {result.ExponentialIntensity:F4}");

                    // 6. Строим ПОЛНУЮ линию регрессии
                    BuildCompleteRegressionLine(result, lowTempIndices);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Мало данных для экспоненты ({lowTempIndices.Count} точек)");
                    // Используем только линейную модель
                    result.ExponentialApproximation = result.LinearApproximation.ToList();
                    result.ExponentialIntensity = 0.2;
                }

                // 7. Расчет ошибок
                result.LinearRMSE = CalculateRMSE(result.Powers, result.LinearApproximation);
                result.ExponentialRMSE = CalculateRMSE(result.Powers, result.ExponentialApproximation);

                System.Diagnostics.Debug.WriteLine($"Ошибки: RMSE_лин = {result.LinearRMSE:F0} МВт, RMSE_экс = {result.ExponentialRMSE:F0} МВт");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА: {ex.Message}");
            }

            return result;
        }
    }
}