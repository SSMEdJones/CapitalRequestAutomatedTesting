using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class RecordEntry
    {
        public CrudOperationType Operation { get; set; } // Tracks Insert, Update, etc.
        public object Data { get; set; } // Holds actual row data
    }
}
