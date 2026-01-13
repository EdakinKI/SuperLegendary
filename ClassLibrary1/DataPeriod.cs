using System;
using System.Collections.Generic;
using WindowsFormsApp1;

public class DataPeriod
{
    public string Name { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<HourlyData> Data { get; set; } = new List<HourlyData>();

    public override string ToString()
    {
        return $"{Name} ({PeriodStart:MM.yyyy} - {PeriodEnd:MM.yyyy}): {Data.Count} точек";
    }
}