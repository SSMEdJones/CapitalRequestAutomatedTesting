namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioResult
    {
        public string ScenarioName { get; set; }
        public List<SeleniumStepResultDetail> Steps { get; set; } = new();
        public bool Passed => Steps.All(s => s.Success);
    }
}
