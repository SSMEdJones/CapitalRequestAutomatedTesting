namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class RollbackPreviewComparisonResult

    {
        public int Id { get; set; }
        public string ScenarioId { get; set; }              // 👈 Unique ID
        public string ScenarioName { get; set; }
        public Dictionary<string, string> SelectedProperties { get; set; } = new();
        public Dictionary<string, TableData> OriginalTables { get; set; } = new();
        public Dictionary<string, TableData> ActualTables { get; set; } = new();
        public List<string> TablesOnlyInOriginal { get; set; } = new();
        public List<string> TablesOnlyInActual { get; set; } = new();
        public List<TableDifference> DifferingTables { get; set; } = new();
        public List<SeleniumStepComparison> SeleniumComparisons { get; set; } = new();
    }
}
