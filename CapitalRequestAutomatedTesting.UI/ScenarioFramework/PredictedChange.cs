using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class PredictedChange
    {
        public string TableName { get; set; }
        public string RowId { get; set; }
        public string FieldName { get; set; }
        public object OriginalValue { get; set; }
        public object NewValue { get; set; }
        public CrudOperationType Type { get; set; }

        // New fields for API routing
        public string TargetApi { get; set; } // "SSMWorkflow" or "CapitalRequest"
        public string Endpoint { get; set; }  // e.g., "/api/scenario/update"
    }


}
