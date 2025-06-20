namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class RecordDifference
    {
        public object RecordPredictive { get; set; }
        public object RecordActual { get; set; }
        public List<FieldDifference> FieldDifferences { get; set; } = new();

        public string RowKey { get; set; }  // 👈 Add this
    }

}
