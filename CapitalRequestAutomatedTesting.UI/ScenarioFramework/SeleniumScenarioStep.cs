using Newtonsoft.Json;
using OpenQA.Selenium;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumScenarioStep
    {

        public int StepNumber { get; set; }

        public string Description { get; set; }
        public SeleniumStepResult Result { get; set; }

        [JsonIgnore]
        public Func<IWebDriver, Task<SeleniumStepResult>> Action { get; set; }
        public bool Retryable { get; set; } = false;

        // Optional: for better diagnostics
        public string StepName { get; set; }
        public string StepType { get; set; }          // e.g., "Navigation", "Validation", "Click"
        public string ExpectedCondition { get; set; } // "Button 'Verify' should exist"
        public string DataSource { get; set; }        // e.g., "proposal.ReviewerGroupId"



    }

}
