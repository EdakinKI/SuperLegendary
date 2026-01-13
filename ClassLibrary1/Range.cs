namespace ClassLibrary1
{
    public class TemperatureRange
    {
        public double From { get; set; }
        public double To { get; set; }
        public double Coefficient { get; set; }

        public bool IsInRange(double temperature)
        {
            return temperature >= From && temperature <= To;
        }
    }
}
