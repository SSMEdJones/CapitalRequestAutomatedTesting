using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class PredictedChange
    {
        public string TableName { get; set; }
        public string RowId { get; set; }
        public string FieldName { get; set; }
        public object OriginalValue { get; set; }
        public object NewValue { get; set; } // Optional if you're only tracking original
        public CrudOperationType Type { get; set; } = CrudOperationType.Update;
    }

}
