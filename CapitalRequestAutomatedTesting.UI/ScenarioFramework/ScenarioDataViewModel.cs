namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class ScenarioDataViewModel
    {
        public string ScenarioId { get; internal set; }
        public int? ActualExecutionDurationMinutes { get; set; }
        public TimeSpan? ActualExecutionDuration { get; set; }


        public Dictionary<string, TableData> Tables { get; internal set; } = new Dictionary<string, TableData>();
    }
}