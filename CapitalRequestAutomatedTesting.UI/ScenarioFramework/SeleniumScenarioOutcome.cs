namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioOutcome
    {
        // Add backing field for Success property to fix CS0103
        private bool _success = false;

        public string ScenarioId { get; set; }
        public SeleniumScenarioResult Expected { get; set; } = new();
        public SeleniumScenarioResult Actual { get; set; }  
        public SeleniumScenarioComparisonResult Comparison { get; set; }

        public int PredictiveCompletionStep { get; set; }
        public string PredictiveStopReason { get; set; }


        public bool Success
        {
            get => Expected?.Passed ?? false;
            set => _success = value; // Allow manual override if needed
        }

        public bool RollbackRequired { get; set; } = false;
        public List<RollbackCandidate> RollbackCandidates { get; set; } = new();

    }

}
