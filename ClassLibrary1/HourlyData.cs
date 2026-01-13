using System;

namespace WindowsFormsApp1
{
    public class HourlyData
    {
        public DateTime DateTime { get; set; }
        public int Hour { get; set; }
        public double Value { get; set; }

        // Для сопоставления с мощностью
        public double? PowerValue { get; set; }

        public override string ToString()
        {
            return $"{DateTime:dd.MM.yyyy HH:mm} - {Value:F2}";
        }
    }
}