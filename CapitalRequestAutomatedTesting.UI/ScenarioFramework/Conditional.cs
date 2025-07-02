using OpenQA.Selenium;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public static class Conditional
    {
        public static Func<IWebDriver, SeleniumStepResult> If(
            bool condition,
            Func<IWebDriver, SeleniumStepResult> whenTrue,
            Func<IWebDriver, SeleniumStepResult> whenFalse)
        {
            return driver => condition ? whenTrue(driver) : whenFalse(driver);
        }
    }

}
