using ClassLibrary1.Models;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

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
                _connectionString = "Host=localhost;Port=5432;Database=Forecast16;Username=postgres;Password=your_password";
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
        public void SaveTemperatureRanges(int paramSetId, List<TemperatureRangeDb> ranges)
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

        public List<TemperatureRangeDb> GetTemperatureRanges(int paramSetId)
        {
            var sql = @"
                SELECT range_number, temp_lower, temp_upper, coefficient 
                FROM temperature_range_limits 
                WHERE param_set_id = @ParamSetId 
                ORDER BY range_number";

            return Query<TemperatureRangeDb>(sql, new { ParamSetId = paramSetId }).ToList();
        }

        // 5. Сохранение расчетов прогноза
        public int SaveForecastCalculation(ForecastCalculationDb calculation)
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
        public int SaveStaticCalculation(StaticCalculationDb calculation, bool askConfirmation = true)
        {
            try
            {
                var sql = @"
            INSERT INTO calculations_static 
            (type_id, calculation_name, calculation_date, 
             k_linear, b_linear, l_exponential) 
            VALUES (@TypeId, @CalculationName, @CalculationDate,
                    @KLinear, @BLinear, @LExponential)
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

                return ExecuteScalar<int>(sql, calculation);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения статического расчета: {ex.Message}");
                throw;
            }
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
        public ForecastParamsDb GetForecastParams(string systemName, string paramSetName)
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
            var paramsDb = new ForecastParamsDb
            {
                ParamSetId = first.param_set_id,
                NameSetId = first.name_set_id,
                SystemId = first.system_id,
                SystemName = first.system_name,
                Ranges = new List<TemperatureRangeDb>()
            };

            foreach (var row in results)
            {
                if (row.temp_lower != null && row.temp_upper != null)
                {
                    paramsDb.Ranges.Add(new TemperatureRangeDb
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

        public List<HistoryItem> GetHistoryData()
        {
            var historyItems = new List<HistoryItem>();

            // Загружаем данные прогнозов
            var forecastSql = @"
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

            var forecastData = Query<ForecastHistoryItem>(forecastSql);

            foreach (var item in forecastData)
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

            // Загружаем данные статических расчетов
            var staticSql = @"
                SELECT 
                    ct.type_name,
                    cs.calculation_date,
                    cs.k_linear,
                    cs.b_linear,
                    cs.l_exponential
                FROM calculations_static cs
                JOIN calculation_types ct ON cs.type_id = ct.type_id
                ORDER BY cs.calculation_date DESC";

            var staticData = Query<StaticHistoryItem>(staticSql);

            foreach (var item in staticData)
            {
                historyItems.Add(new HistoryItem
                {
                    TypeName = item.type_name,
                    CalculationDate = item.calculation_date,
                    KLinear = item.k_linear,
                    BLinear = item.b_linear,
                    LExponential = item.l_exponential
                });
            }

            return historyItems.OrderByDescending(x => x.CalculationDate).ToList();
        }
    }

    // Классы моделей для базы данных
    public class TemperatureRangeDb
    {
        public int RangeNumber { get; set; }
        public int? TempLower { get; set; }
        public int? TempUpper { get; set; }
        public double? Coefficient { get; set; }
    }

    public class ForecastParamsDb
    {
        public int ParamSetId { get; set; }
        public string NameSetId { get; set; }
        public int SystemId { get; set; }
        public string SystemName { get; set; }
        public List<TemperatureRangeDb> Ranges { get; set; }
    }

    public class ForecastCalculationDb
    {
        public int TypeId { get; set; }
        public string CalculationName { get; set; }
        public DateTime CalculationDate { get; set; }
        public DateTime TargetDate { get; set; }
        public int SystemId { get; set; }
        public double TOriginal { get; set; }
        public double? POriginal { get; set; }
        public double? EOriginal { get; set; }
        public double TResult { get; set; }
        public double? PResult { get; set; }
        public double? EResult { get; set; }
        public int ParamSetId { get; set; }
    }

    public class StaticCalculationDb
    {
        public int TypeId { get; set; }
        public string CalculationName { get; set; }
        public DateTime CalculationDate { get; set; }
        public double KLinear { get; set; }
        public double BLinear { get; set; }
        public double LExponential { get; set; }
    }
}