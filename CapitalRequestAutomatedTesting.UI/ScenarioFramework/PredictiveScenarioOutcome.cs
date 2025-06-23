namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class PredictiveScenarioOutcome
    {
        public string ScenarioName { get; set; }
        public List<PredictiveStepResult> Steps { get; set; } = new();
        public bool AllPassed => Steps.All(x => x.Passed);
    }

}
