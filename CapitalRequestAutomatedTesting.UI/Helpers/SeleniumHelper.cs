using CapitalRequestAutomatedTesting.UI.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{

    public class SeleniumHelper
    {
        public static WorkflowTestResult RunWithSafeChromeDriver(Func<IWebDriver, WorkflowTestResult> testLogic)
        {
            var result = new WorkflowTestResult();



            var options = new ChromeOptions();
            options.BinaryLocation = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--remote-debugging-port=9222");
            options.AddArgument("--disable-session-crashed-bubble");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-popup-blocking");

            string tempProfile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempProfile);
            options.AddArgument($"--user-data-dir={tempProfile}");

            IWebDriver driver = null;

            try
            {
                driver = new ChromeDriver(options);
                result = testLogic(driver);
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Unexpected error: {ex.Message}";
            }

            finally
            {
                try
                {
                    driver?.Quit();
                }
                catch { }

                // Force kill ChromeDriver and Chrome
                foreach (var processName in new[] { "chromedriver", "chrome" })
                {
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        try { process.Kill(); } catch { }
                    }
                }
            }


            return result;
        }

    }
}
