using ClassLibrary1;
using ClassLibrary1.Models;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"]?.ConnectionString;

            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = "Host=localhost;Port=5432;Database=SS;Username=EdakinKI;Password=nata230572";
            }
        }

        // Метод для выполнения запросов
        public IEnumerable<T> Query<T>(string sql, object parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<T>(sql, parameters);
            }
        }

        public int Execute(string sql, object parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Execute(sql, parameters);
            }
        }

        public T ExecuteScalar<T>(string sql, object parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                return connection.ExecuteScalar<T>(sql, parameters);
            }
        }

        // 1. Работа с типами расчетов
        public int GetOrCreateCalculationType(string typeName)
        {
            var sql = @"
                INSERT INTO calculation_types (type_name) 
                VALUES (@TypeName) 
                ON CONFLICT (type_name) DO UPDATE SET type_name = EXCLUDED.type_name
                RETURNING type_id";

            return ExecuteScalar<int>(sql, new { TypeName = typeName });
        }

        // 2. Работа с энергосистемами
        public int GetOrCreateEnergySystem(string systemName)
        {
            var sql = @"
                INSERT INTO energy_systems (system_name) 
                VALUES (@SystemName) 
                ON CONFLICT (system_name) DO UPDATE SET system_name = EXCLUDED.system_name
                RETURNING system_id";

            return ExecuteScalar<int>(sql, new { SystemName = systemName });
        }

        public List<string> GetAllEnergySystems()
        {
            var sql = "SELECT system_name FROM energy_systems ORDER BY system_name";
            return Query<string>(sql).ToList();
        }

        // 3. Работа с наборами параметров прогноза
        public int CreateForecastParamSet(string nameSetId, int systemId, int tableTypeId, DateTime dateStart, DateTime? dateEnd = null)
        {
            var sql = @"
        INSERT INTO forecast_param_sets (name_set_id, system_id, table_type_id, date_start, date_end) 
        VALUES (@NameSetId, @SystemId, @TableTypeId, @DateStart, @DateEnd)
        RETURNING param_set_id";

            return ExecuteScalar<int>(sql, new
            {
                NameSetId = nameSetId,
                SystemId = systemId,
                TableTypeId = tableTypeId,
                DateStart = dateStart,
                DateEnd = dateEnd
            });
        }

        public void UpdatePreviousParamSetEndDate(int systemId, DateTime newStartDate)
        {
            var sql = @"
                UPDATE forecast_param_sets 
                SET date_end = @NewStartDate 
                WHERE system_id = @SystemId 
                AND date_end IS NULL";

            Execute(sql, new { SystemId = systemId, NewStartDate = newStartDate });
        }

        public List<string> GetAvailableParamSets(string systemName)
        {
            var sql = @"
                SELECT DISTINCT fps.name_set_id 
                FROM forecast_param_sets fps
                JOIN energy_systems es ON fps.system_id = es.system_id
                WHERE es.system_name = @SystemName
                AND (fps.date_end IS NULL OR fps.date_end > CURRENT_TIMESTAMP)
                ORDER BY fps.name_set_id";

            return Query<string>(sql, new { SystemName = systemName }).ToList();
        }

        // 4. Работа с температурными диапазонами
        public void SaveTemperatureRanges(int paramSetId, List<DbTemperatureRange> ranges)
        {
            foreach (var range in ranges)
            {
                var sql = @"
                    INSERT INTO temperature_range_limits 
                    (param_set_id, range_number, temp_lower, temp_upper, coefficient) 
                    VALUES (@ParamSetId, @RangeNumber, @TempLower, @TempUpper, @Coefficient)
                    ON CONFLICT (param_set_id, range_number) DO UPDATE SET
                        temp_lower = EXCLUDED.temp_lower,
                        temp_upper = EXCLUDED.temp_upper,
                        coefficient = EXCLUDED.coefficient";

                Execute(sql, new
                {
                    ParamSetId = paramSetId,
                    RangeNumber = range.RangeNumber,
                    TempLower = range.TempLower,
                    TempUpper = range.TempUpper,
                    Coefficient = range.Coefficient
                });
            }
        }

        public List<DbTemperatureRange> GetTemperatureRanges(int paramSetId)
        {
            var sql = @"
                SELECT range_number, temp_lower, temp_upper, coefficient 
                FROM temperature_range_limits 
                WHERE param_set_id = @ParamSetId 
                ORDER BY range_number";

            return Query<DbTemperatureRange>(sql, new { ParamSetId = paramSetId }).ToList();
        }

        // 5. Сохранение расчетов прогноза
        public int SaveForecastCalculation(DbForecastCalculation calculation)
        {
            var sql = @"
        INSERT INTO calculations_forecast 
        (type_id, calculation_name, calculation_date, target_date, 
         system_id, t_original, p_original, e_original, 
         t_result, p_result, e_result, param_set_id) 
        VALUES (@TypeId, @CalculationName, @CalculationDate, @TargetDate,
                @SystemId, @TOriginal, @POriginal, @EOriginal,
                @TResult, @PResult, @EResult, @ParamSetId)
        RETURNING forecast_id";

            return ExecuteScalar<int>(sql, calculation);
        }

        // 6. Сохранение расчетов статических зависимостей
        public int SaveStaticCalculation(DbStaticCalculation calculation, List<RegressionResult> regressionResults = null)
        {
            try
            {
                var sql = @"
            INSERT INTO calculations_static 
            (type_id, calculation_name, calculation_date) 
            VALUES (@TypeId, @CalculationName, @CalculationDate)
            RETURNING static_id";

                // Убедимся, что все поля заполнены
                if (string.IsNullOrEmpty(calculation.CalculationName))
                {
                    calculation.CalculationName = $"Статический анализ {DateTime.Now:dd.MM.yyyy HH:mm}";
                }

                if (calculation.CalculationDate == default)
                {
                    calculation.CalculationDate = DateTime.Now;
                }

                int staticId = ExecuteScalar<int>(sql, calculation);

                // Если есть результаты регрессии, сохраняем их
                if (regressionResults != null && regressionResults.Count > 0)
                {
                    SaveRegressionResults(staticId, regressionResults);
                }

                return staticId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения статического расчета: {ex.Message}");
                throw;
            }
        }

        // Новый метод для сохранения результатов регрессии
        private void SaveRegressionResults(int staticId, List<RegressionResult> regressionResults)
        {
            foreach (var result in regressionResults)
            {
                // Сохраняем период
                var periodSql = @"
            INSERT INTO regression_periods 
            (period_name, period_year, static_id) 
            VALUES (@PeriodName, @PeriodYear, @StaticId)
            RETURNING period_id";

                string periodYear = result.PeriodName.Contains("Сен-Май")
                    ? result.PeriodName.Split(' ')[0]
                    : result.PeriodName;

                int periodId = ExecuteScalar<int>(periodSql, new
                {
                    PeriodName = result.PeriodName,
                    PeriodYear = periodYear,
                    StaticId = staticId
                });

                // Сохраняем линейную регрессию
                var linearSql = @"
            INSERT INTO regression_results 
            (period_id, regression_type_id, k_coefficient, b_coefficient) 
            VALUES (@PeriodId, @RegressionTypeId, @KCoefficient, @BCoefficient)";

                // Получаем ID типа линейной регрессии
                int linearTypeId = GetOrCreateRegressionType("Линейная регрессия");

                Execute(linearSql, new
                {
                    PeriodId = periodId,
                    RegressionTypeId = linearTypeId,
                    KCoefficient = result.LinearSlope,
                    BCoefficient = result.LinearIntercept
                });

                // Сохраняем экспоненциальную регрессию
                var expSql = @"
            INSERT INTO regression_results 
            (period_id, regression_type_id, k_coefficient, l_coefficient) 
            VALUES (@PeriodId, @RegressionTypeId, @KCoefficient, @LCoefficient)";

                // Получаем ID типа экспоненциальной регрессии
                int expTypeId = GetOrCreateRegressionType("Экспоненциальная регрессия");

                Execute(expSql, new
                {
                    PeriodId = periodId,
                    RegressionTypeId = expTypeId,
                    KCoefficient = result.LinearSlope, // Используем тот же k
                    LCoefficient = result.ExponentialIntensity
                });
            }
        }

        // Метод для получения/создания типа регрессии
        public int GetOrCreateRegressionType(string regressionTypeName)
        {
            var sql = @"
        INSERT INTO regression_types (regression_type_name) 
        VALUES (@RegressionTypeName) 
        ON CONFLICT (regression_type_name) DO UPDATE SET regression_type_name = EXCLUDED.regression_type_name
        RETURNING regression_type_id";

            return ExecuteScalar<int>(sql, new { RegressionTypeName = regressionTypeName });
        }

        public int GetOrCreateTableType(string tableTypeName)
        {
            var sql = @"
        INSERT INTO table_types (table_type_name) 
        VALUES (@TableTypeName) 
        ON CONFLICT (table_type_name) DO UPDATE SET table_type_name = EXCLUDED.table_type_name
        RETURNING table_type_id";

            return ExecuteScalar<int>(sql, new { TableTypeName = tableTypeName });
        }

        // 10. Получение ID табличного типа по имени
        public int? GetTableTypeId(string tableTypeName)
        {
            var sql = "SELECT table_type_id FROM table_types WHERE table_type_name = @TableTypeName";

            try
            {
                return ExecuteScalar<int?>(sql, new { TableTypeName = tableTypeName });
            }
            catch
            {
                return null;
            }
        }

        // 7. Получение данных для расчета прогноза
        public DbForecastParams GetForecastParams(string systemName, string paramSetName)
        {
            var sql = @"
        SELECT 
            fps.param_set_id,
            fps.name_set_id,
            es.system_id,
            es.system_name,
            trl.range_number,
            trl.temp_lower,
            trl.temp_upper,
            trl.coefficient
        FROM forecast_param_sets fps
        JOIN energy_systems es ON fps.system_id = es.system_id
        LEFT JOIN temperature_range_limits trl ON fps.param_set_id = trl.param_set_id
        WHERE es.system_name = @SystemName 
        AND fps.name_set_id = @ParamSetName
        AND (fps.date_end IS NULL OR fps.date_end > CURRENT_TIMESTAMP)
        ORDER BY trl.range_number";

            var results = Query<dynamic>(sql, new { SystemName = systemName, ParamSetName = paramSetName });

            if (!results.Any()) return null;

            var first = results.First();
            var paramsDb = new DbForecastParams
            {
                ParamSetId = first.param_set_id,
                NameSetId = first.name_set_id,
                SystemId = first.system_id,
                SystemName = first.system_name,
                Ranges = new List<DbTemperatureRange>()
            };

            foreach (var row in results)
            {
                if (row.temp_lower != null && row.temp_upper != null)
                {
                    paramsDb.Ranges.Add(new DbTemperatureRange
                    {
                        RangeNumber = row.range_number,
                        TempLower = row.temp_lower,
                        TempUpper = row.temp_upper,
                        Coefficient = row.coefficient
                    });
                }
            }

            return paramsDb;
        }

        // 8. Проверка подключения
        public bool TestConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        // Удаление расчета прогноза
        public bool DeleteForecastCalculation(int forecastId)
        {
            try
            {
                var sql = @"DELETE FROM calculations_forecast WHERE forecast_id = @ForecastId";
                int affectedRows = Execute(sql, new { ForecastId = forecastId });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления расчета прогноза: {ex.Message}");
                return false;
            }
        }

        // Удаление статического расчета (удалит и связанные периоды с регрессиями благодаря CASCADE)
        public bool DeleteStaticCalculation(int staticId)
        {
            try
            {
                var sql = @"DELETE FROM calculations_static WHERE static_id = @StaticId";
                int affectedRows = Execute(sql, new { StaticId = staticId });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления статического расчета: {ex.Message}");
                return false;
            }
        }

        // Получение ID расчета по данным (для идентификации)
        public int? GetForecastId(DateTime calculationDate, string systemName, string calculationName)
        {
            try
            {
                var sql = @"
            SELECT cf.forecast_id 
            FROM calculations_forecast cf
            JOIN energy_systems es ON cf.system_id = es.system_id
            WHERE cf.calculation_date = @CalculationDate 
            AND es.system_name = @SystemName 
            AND cf.calculation_name = @CalculationName
            LIMIT 1";

                return ExecuteScalar<int?>(sql, new
                {
                    CalculationDate = calculationDate,
                    SystemName = systemName,
                    CalculationName = calculationName
                });
            }
            catch
            {
                return null;
            }
        }

        public int? GetStaticId(DateTime calculationDate, string calculationName)
        {
            try
            {
                var sql = @"
            SELECT static_id 
            FROM calculations_static 
            WHERE calculation_date = @CalculationDate 
            AND calculation_name = @CalculationName
            LIMIT 1";

                return ExecuteScalar<int?>(sql, new
                {
                    CalculationDate = calculationDate,
                    CalculationName = calculationName
                });
            }
            catch
            {
                return null;
            }
        }

        public List<HistoryItem> GetHistoryData()
        {
            var historyItems = new List<HistoryItem>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // 1. Загружаем данные прогнозов
                    var forecastSql = @"
                    SELECT 
                        cf.forecast_id,
                        ct.type_name,
                        cf.calculation_date,
                        cf.target_date,
                        cf.t_original,
                        cf.t_result,
                        cf.p_original,
                        cf.p_result,
                        cf.e_original,
                        cf.e_result,
                        es.system_name,
                        cf.calculation_name
                    FROM calculations_forecast cf
                    JOIN calculation_types ct ON cf.type_id = ct.type_id
                    JOIN energy_systems es ON cf.system_id = es.system_id
                    WHERE ct.type_name IN (
                        'Прогноз потребления по мощности',
                        'Прогноз потребления по электроэнергии',
                        'Прогноз потребления по мощности и электроэнергии'
                    )
                    ORDER BY cf.calculation_date DESC";

                    var forecastData = connection.Query<ForecastHistoryItem>(forecastSql);

                    foreach (var item in forecastData)
                    {
                        var historyItem = new HistoryItem
                        {
                            ForecastId = item.forecast_id,
                            TypeName = item.type_name,
                            CalculationDate = item.calculation_date,
                            TargetDate = item.target_date,
                            TOriginal = item.t_original,
                            TResult = item.t_result,
                            SystemName = item.system_name,
                            CalculationName = item.calculation_name
                        };

                        // В зависимости от типа расчета добавляем соответствующие поля
                        switch (item.type_name)
                        {
                            case "Прогноз потребления по мощности":
                                historyItem.POriginal = item.p_original;
                                historyItem.PResult = item.p_result;
                                break;

                            case "Прогноз потребления по электроэнергии":
                                historyItem.EOriginal = item.e_original;
                                historyItem.EResult = item.e_result;
                                break;

                            case "Прогноз потребления по мощности и электроэнергии":
                                historyItem.POriginal = item.p_original;
                                historyItem.PResult = item.p_result;
                                historyItem.EOriginal = item.e_original;
                                historyItem.EResult = item.e_result;
                                break;
                        }

                        historyItems.Add(historyItem);
                    }

                    // 2. Загружаем данные статических зависимостей
                    var staticSql = @"
                SELECT DISTINCT
                    cs.static_id,
                    ct.type_name,
                    cs.calculation_date,
                    cs.calculation_name
                FROM calculations_static cs
                JOIN calculation_types ct ON cs.type_id = ct.type_id
                WHERE ct.type_name = 'Расчет статических зависимостей'
                ORDER BY cs.calculation_date DESC";

                    var staticCalculations = connection.Query<dynamic>(staticSql);

                    foreach (var calc in staticCalculations)
                    {
                        var historyItem = new HistoryItem
                        {
                            TypeName = calc.type_name,
                            CalculationDate = calc.calculation_date,
                            CalculationName = calc.calculation_name,
                            IsStaticAnalysis = true,
                            StaticId = calc.static_id
                        };

                        // Теперь загружаем периоды и регрессии для этого статического расчета
                        var periodsSql = @"
                    SELECT 
                        rp.period_id,
                        rp.period_name,
                        rp.period_year,
                        -- Линейная регрессия
                        MAX(CASE WHEN rt.regression_type_name = 'Линейная регрессия' THEN rr.k_coefficient END) as linear_k,
                        MAX(CASE WHEN rt.regression_type_name = 'Линейная регрессия' THEN rr.b_coefficient END) as linear_b,
                        -- Экспоненциальная регрессия
                        MAX(CASE WHEN rt.regression_type_name = 'Экспоненциальная регрессия' THEN rr.l_coefficient END) as exp_l
                    FROM regression_periods rp
                    LEFT JOIN regression_results rr ON rp.period_id = rr.period_id
                    LEFT JOIN regression_types rt ON rr.regression_type_id = rt.regression_type_id
                    WHERE rp.static_id = @StaticId
                    GROUP BY rp.period_id, rp.period_name, rp.period_year
                    ORDER BY rp.period_name";

                        var periods = connection.Query<dynamic>(periodsSql, new { StaticId = calc.static_id });

                        var periodDetails = new List<PeriodDetail>();
                        var detailsText = new List<string>();

                        foreach (var period in periods)
                        {
                            var periodDetail = new PeriodDetail
                            {
                                PeriodName = period.period_name?.ToString() ?? "Неизвестный период",
                                PeriodYear = period.period_year?.ToString() ?? "",
                                KLinear = period.linear_k != null ? Convert.ToDouble(period.linear_k) : (double?)null,
                                BLinear = period.linear_b != null ? Convert.ToDouble(period.linear_b) : (double?)null,
                                LExponential = period.exp_l != null ? Convert.ToDouble(period.exp_l) : (double?)null
                            };

                            periodDetails.Add(periodDetail);

                            // Формируем текст для отображения
                            detailsText.Add($"Период: {periodDetail.PeriodName} ({periodDetail.PeriodYear})");
                            if (periodDetail.KLinear.HasValue)
                                detailsText.Add($"  k (наклон): {periodDetail.KLinear.Value:F4}");
                            if (periodDetail.BLinear.HasValue)
                                detailsText.Add($"  b (свободный член): {periodDetail.BLinear.Value:F2}");
                            if (periodDetail.LExponential.HasValue)
                                detailsText.Add($"  L (интенсивность): {periodDetail.LExponential.Value:F4}");
                            detailsText.Add(""); // Пустая строка между периодами
                        }

                        historyItem.PeriodDetails = periodDetails;
                        historyItem.RegressionSummary = string.Join("\n", detailsText);

                        historyItems.Add(historyItem);
                    }

                    // 3. Сортируем все по дате
                    historyItems = historyItems.OrderByDescending(x => x.CalculationDate).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки истории расчетов: {ex.Message}\n{ex.StackTrace}");

                // Запасной вариант - простой запрос
                try
                {
                    var fallbackSql = @"
                SELECT 
                    ct.type_name,
                    cf.calculation_date,
                    cf.target_date,
                    cf.t_original,
                    cf.t_result,
                    cf.p_original,
                    cf.p_result,
                    cf.e_original,
                    cf.e_result
                FROM calculations_forecast cf
                JOIN calculation_types ct ON cf.type_id = ct.type_id
                ORDER BY cf.calculation_date DESC";

                    var fallbackData = Query<ForecastHistoryItem>(fallbackSql);

                    foreach (var item in fallbackData)
                    {
                        historyItems.Add(new HistoryItem
                        {
                            TypeName = item.type_name,
                            CalculationDate = item.calculation_date,
                            TargetDate = item.target_date,
                            TOriginal = item.t_original,
                            TResult = item.t_result,
                            POriginal = item.p_original,
                            PResult = item.p_result,
                            EOriginal = item.e_original,
                            EResult = item.e_result
                        });
                    }

                    // Также попробуем загрузить статические расчеты простым способом
                    var simpleStaticSql = @"
                SELECT 
                    ct.type_name,
                    cs.calculation_date,
                    cs.calculation_name
                FROM calculations_static cs
                JOIN calculation_types ct ON cs.type_id = ct.type_id
                WHERE ct.type_name = 'Расчет статических зависимостей'
                ORDER BY cs.calculation_date DESC";

                    var staticData = Query<dynamic>(simpleStaticSql);
                    foreach (var item in staticData)
                    {
                        historyItems.Add(new HistoryItem
                        {
                            TypeName = item.type_name,
                            CalculationDate = item.calculation_date,
                            CalculationName = item.calculation_name,
                            IsStaticAnalysis = true
                        });
                    }
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при запасной загрузке: {ex2.Message}");
                }
            }

            return historyItems;
        }
    }
}