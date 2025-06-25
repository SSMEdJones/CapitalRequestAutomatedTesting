namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumStepComparison
    {
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public string ExpectedMessage { get; set; }
        public bool? ExpectedSuccess { get; set; }
        public string ActualMessage { get; set; }
        public bool? ActualSuccess { get; set; }
        public bool IsMatch => ExpectedSuccess == ActualSuccess;

    }
}
