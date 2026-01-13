using System.Collections.Generic;

namespace ClassLibrary1
{
    public class EnergySystem
    {
        public string Name { get; set; }
        public List<TemperatureRange> Ranges { get; set; } = new List<TemperatureRange>();
    }
}