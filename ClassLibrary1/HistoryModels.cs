using System;

namespace ClassLibrary1.Models
{
    public class ForecastHistoryItem
    {
        public string type_name { get; set; }
        public DateTime calculation_date { get; set; }
        public DateTime target_date { get; set; }
        public double? t_original { get; set; }
        public double? t_result { get; set; }
        public double? p_original { get; set; }
        public double? p_result { get; set; }
        public double? e_original { get; set; }
        public double? e_result { get; set; }
    }

    public class StaticHistoryItem
    {
        public string type_name { get; set; }
        public DateTime calculation_date { get; set; }
        public double? k_linear { get; set; }
        public double? b_linear { get; set; }
        public double? l_exponential { get; set; }
    }

    public class HistoryItem
    {
        public string TypeName { get; set; }
        public DateTime CalculationDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public double? TOriginal { get; set; }
        public double? TResult { get; set; }
        public double? POriginal { get; set; }
        public double? PResult { get; set; }
        public double? EOriginal { get; set; }
        public double? EResult { get; set; }
        public double? KLinear { get; set; }
        public double? BLinear { get; set; }
        public double? LExponential { get; set; }
        public string CalculationType =>
            POriginal.HasValue && EOriginal.HasValue ? "Прогноз потребления по мощности и электроэнергии" :
            POriginal.HasValue ? "Прогноз потребления по мощности" :
            EOriginal.HasValue ? "Прогноз потребления по электроэнергии" :
            "Расчет статических зависимостей";
    }
}