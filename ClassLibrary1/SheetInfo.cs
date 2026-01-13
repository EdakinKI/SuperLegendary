using System.Data;

namespace ClassLibrary1
{
    public class SheetInfo
    {
        public string Name { get; set; }
        public SheetType Type { get; set; }
        public DataTable DataTable { get; set; }
    }
}
