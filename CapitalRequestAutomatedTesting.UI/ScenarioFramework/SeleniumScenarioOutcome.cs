namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioOutcome
    {
        public SeleniumScenarioResult Expected { get; set; }
        public SeleniumScenarioResult Actual { get; set; }
        public SeleniumScenarioComparisonResult Comparison { get; set; }
    }

}
