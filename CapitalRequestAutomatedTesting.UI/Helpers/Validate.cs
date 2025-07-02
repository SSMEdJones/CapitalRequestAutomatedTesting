using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class Validate
    {
        public static Func<IWebDriver, Task<SeleniumStepResult>> ButtonById(string buttonId, string buttonText)
            => SeleniumHelper.ValidateElementById(buttonId, buttonText);

        public static Func<IWebDriver, Task<SeleniumStepResult>> ElementById(string id, string description)
            => SeleniumHelper.ValidateElementById(id, description);


        public static Func<IWebDriver, Task<SeleniumStepResult>> TextInTag(string tag, string text)
            => SeleniumHelper.ValidateElementWithText(tag, text);

        public static Func<IWebDriver, Task<SeleniumStepResult>> Text(string text)
            => SeleniumHelper.ValidateElementWithText(text);

        public static Func<IWebDriver, Task<SeleniumStepResult>> ButtonInRowWithText(string rowText, string buttonText)
           => SeleniumHelper.ValidateButtonInRowWithText(rowText, buttonText);

        public static Func<IWebDriver, Task<SeleniumStepResult>> ElementNotPresentById(string id, string description)
           => SeleniumHelper.AssertElementNotPresentById(id, description);
        public static Func<IWebDriver, Task<SeleniumStepResult>> ElementTextById(string id, string expectedText, string description)
           => SeleniumHelper.AssertElementTextById(id, expectedText, description);

        public static Func<IWebDriver, Task<SeleniumStepResult>> TextIsEmpty(string id, string description)
            => SeleniumHelper.ValidateElementTextIsEmpty(id, description);

        public static Func<IWebDriver, Task<SeleniumStepResult>> DashboardStatus(int dashboardOrder, string expectedName, DateTime expectedDate)
            => SeleniumHelper.ValidateReviewerDashboardCell(dashboardOrder, expectedName, expectedDate);

        public static Func<IWebDriver, Task<SeleniumStepResult>> NoRequestsMessage()
            => SeleniumHelper.NoRequestsMessage();
        //public static Func<IWebDriver, Task<SeleniumStepResult>> NoRequestsMessage()
        //{
        //    var syncFunc = SeleniumHelper.NoRequestsMessage(); // This is Func<IWebDriver, SeleniumStepResult>

        //    return driver =>
        //    {
        //        var result = syncFunc(driver); // Call the sync function
        //        return Task.FromResult(result); // Wrap the result in a Task
        //    };
        //}


    }

}
