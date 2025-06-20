namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioComparisonResult
    {
        public string ScenarioId { get; set; }              // 👈 Unique ID
        public string ScenarioName { get; set; }
        public Dictionary<string, string> SelectedProperties { get; set; } = new();
        public Dictionary<string, TableData> PredictiveTables { get; set; } = new();
        public Dictionary<string, TableData> ActualTables { get; set; } = new();
        public List<string> TablesOnlyInPredictive { get; set; } = new();
        public List<string> TablesOnlyInActual { get; set; } = new();
        public List<TableDifference> DifferingTables { get; set; } = new();
    }
}
