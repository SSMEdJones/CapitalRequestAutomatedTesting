using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class TableData
    {
        public string TableName { get; set; }
        public CrudOperationType Operation { get; set; } // Tracks Insert, Update, etc.
        //public List<object> Records { get; set; } = new List<object>(); // Stores multiple rows
        public List<RecordEntry> Records { get; set; } = new List<RecordEntry>(); // Now stores structured entries

    }
}
