namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class RowData
    {
        public string RowId { get; set; }

        // Key: Field name, Value: FieldData
        public Dictionary<string, FieldData> Fields { get; set; } = new();

        // Original values before prediction
        public Dictionary<string, object> OriginalValues { get; set; } = new();

        // Predicted values from the scenario engine
        public Dictionary<string, object> PredictedValues { get; set; } = new();

        // Actual values after execution
        public Dictionary<string, object> ActualValues { get; set; } = new();
    }

}
