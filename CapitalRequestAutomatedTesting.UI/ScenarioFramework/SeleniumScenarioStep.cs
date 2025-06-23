using OpenQA.Selenium;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioStep
    {
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public Func<IWebDriver, Task<SeleniumStepResult>> Action { get; set; }
        public SeleniumStepResult Result { get; set; }
    }

}
