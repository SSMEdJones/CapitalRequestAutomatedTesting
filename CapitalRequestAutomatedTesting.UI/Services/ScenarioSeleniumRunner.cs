using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium.Chrome;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public class ScenarioSeleniumRunner
    {
        private readonly IActualSeleniumService _actualSeleniumService;

        public ScenarioSeleniumRunner(IActualSeleniumService actualSeleniumService)
        {
            _actualSeleniumService = actualSeleniumService;
        }

        public async Task<SeleniumScenarioOutcome> RunScenarioAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var options = new ChromeOptions();
            options.BinaryLocation = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--remote-debugging-port=9222");
            options.AddArgument("--disable-session-crashed-bubble");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-popup-blocking");

            using var driver = new ChromeDriver(options);
            //using var driver = new ChromeDriver(); // or EdgeDriver, etc.

            try
            {
                // Step 1: Generate the steps for the current scenario
                var steps = await _actualSeleniumService.GenerateSeleniumSteps(scenarioDetail);

                // Step 2: Execute those steps using the real WebDriver
                var result = await _actualSeleniumService.ExecuteSeleniumStepsAsync(steps, scenarioDetail, driver);

                return result;
            }
            finally
            {
                driver.Quit(); // always clean up
            }
        }
    }

}
