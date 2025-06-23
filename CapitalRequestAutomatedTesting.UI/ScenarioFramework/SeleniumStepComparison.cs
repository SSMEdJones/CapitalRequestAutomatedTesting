namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumStepComparison
    {
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public string ExpectedMessage { get; set; }
        public string ActualMessage { get; set; }
        public bool Success { get; set; } // true if expected and actual match
    }
}
