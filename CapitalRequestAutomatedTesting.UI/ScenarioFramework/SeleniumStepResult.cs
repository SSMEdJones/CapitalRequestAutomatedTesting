namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class SeleniumStepResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ScreenshotPath { get; set; }

        public static SeleniumStepResult Pass(string message)
        {
            return new SeleniumStepResult
            {
                Success = true,
                Message = message
            };
        }

        public static SeleniumStepResult Fail(string message, string screenshotPath = null)
        {
            return new SeleniumStepResult
            {
                Success = false,
                Message = message,
                ScreenshotPath = screenshotPath
            };
        }
    }

}
