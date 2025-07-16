namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioOutcome
    {
        public string ScenarioId { get; set; }
        public SeleniumScenarioResult Expected { get; set; }
        public SeleniumScenarioResult Actual { get; set; }  
        public SeleniumScenarioComparisonResult Comparison { get; set; }

        public int PredictiveCompletionStep { get; set; }
        public string PredictiveStopReason { get; set; }

        public bool Success { get; set; } = false; // Default to false


    }

}
