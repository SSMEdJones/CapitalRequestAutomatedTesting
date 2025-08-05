using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class FieldData
    {
        public object OriginalValue { get; set; }
        public object NewValue { get; set; }
        public CrudOperationType Operation { get; set; } // Optional: Insert, Update, Delete
    }


}
