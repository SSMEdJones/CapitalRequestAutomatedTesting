namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class PredictiveStepResult
    {
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public string ExpectedValue { get; set; } // "true", "false", or specific string like "Success message displayed"
        public string ActualValue { get; set; }
        public bool Passed => string.Equals(ExpectedValue, ActualValue, StringComparison.OrdinalIgnoreCase);
    }

}
