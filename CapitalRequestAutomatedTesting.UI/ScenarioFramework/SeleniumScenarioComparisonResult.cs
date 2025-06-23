namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioComparisonResult
    {
        public bool AllStepsMatch { get; set; }
        public List<SeleniumStepComparison> StepComparisons { get; set; } = new();
    }

    

}
