using OpenQA.Selenium;

namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public static class Conditional
    {
        public static Func<IWebDriver, Task<SeleniumStepResult>> If(
            bool condition,
            Func<IWebDriver, Task<SeleniumStepResult>> whenTrue,
            Func<IWebDriver, Task<SeleniumStepResult>> whenFalse)
        {
            return async driver =>
            {
                if (condition)
                    return await whenTrue(driver);
                else
                    return await whenFalse(driver);
            };
        }
    }


}
