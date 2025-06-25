using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public class SeleniumDsl
    {
        private readonly List<Func<IWebDriver, Task<SeleniumStepResult>>> _actions = new();

        public SeleniumDsl BeginWith(Func<IWebDriver, Task<SeleniumStepResult>> initialAction)
        {
            _actions.Add(initialAction);
            return this;
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> NavigateTo(string url)
        {
            return async driver =>
            {
                driver.Navigate().GoToUrl(url);
                await Task.Delay(500); // or smarter wait logic here
                return new SeleniumStepResult
                {
                    Success = true,
                    Message = $"Navigated to {url}"
                };
            };
        }

        public SeleniumDsl WaitForElement(string selector)
        {
            _actions.Add(async driver =>
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                wait.Until(drv => drv.FindElement(By.CssSelector(selector)));
                return new SeleniumStepResult { Success = true, Message = $"Element '{selector}' is visible." };
            });
            return this;
        }

        public SeleniumDsl Click(string selector)
        {
            _actions.Add(async driver =>
            {
                driver.FindElement(By.CssSelector(selector)).Click();
                return new SeleniumStepResult { Success = true, Message = $"Clicked element '{selector}'." };
            });
            return this;
        }

        public Func<IWebDriver, Task<SeleniumStepResult>> Build(string summary)
        {
            return async driver =>
            {
                foreach (var action in _actions)
                {
                    var result = await action(driver);
                    if (!result.Success)
                        return new SeleniumStepResult { Success = false, Message = result.Message };
                }

                return new SeleniumStepResult { Success = true, Message = summary };
            };
        }

        public SeleniumDsl Then(Func<IWebDriver, Task<SeleniumStepResult>> customAction)
        {
            _actions.Add(customAction);
            return this;
        }

        

    }

}
