using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Services
{
    // Классы моделей для базы данных - ПЕРЕИМЕНОВАЛ
    public class DbTemperatureRange
    {
        public int RangeNumber { get; set; }
        public int? TempLower { get; set; }
        public int? TempUpper { get; set; }
        public double? Coefficient { get; set; }
    }

    public class DbForecastParams
    {
        public int ParamSetId { get; set; }
        public string NameSetId { get; set; }
        public int SystemId { get; set; }
        public string SystemName { get; set; }
        public List<DbTemperatureRange> Ranges { get; set; }
    }

    public class DbForecastCalculation
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

    public class DbStaticCalculation
    {
        public int TypeId { get; set; }
        public string CalculationName { get; set; }
        public DateTime CalculationDate { get; set; }
        public double KLinear { get; set; }
        public double BLinear { get; set; }
        public double LExponential { get; set; }
    }

    public class DbEnergySystem
    {
        public int SystemId { get; set; }
        public string SystemName { get; set; }
    }
}