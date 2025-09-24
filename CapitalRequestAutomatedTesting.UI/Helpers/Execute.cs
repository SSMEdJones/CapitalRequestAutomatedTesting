using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class Execute
    {
        public static Func<IWebDriver, Task<SeleniumStepResult>> ClickButtonInRow(string rowText, string buttonText)
            => SeleniumHelper.ClickButtonInRow(rowText, buttonText);

        public static Func<IWebDriver, Task<SeleniumStepResult>> NavigateTo(string url)
            => SeleniumHelper.NavigateToUrl(url);


        public static Func<IWebDriver, Task<SeleniumStepResult>> ClickButtonById(string buttonId, string buttonText)
            => SeleniumHelper.ClickButtonById(buttonId, buttonText);

        //public static Func<IWebDriver, Task<SeleniumStepResult>> ClickWhenVisibleById(string id, string description)
        //    => SeleniumHelper.ClickWhenVisibleById(id, description);

        public static Func<IWebDriver, Task<SeleniumStepResult>> RobustClickById(string elementId, string description, int maxRetries = 3)
            => SeleniumHelper.RobustClickById(elementId, description, maxRetries);

        public static Func<IWebDriver, Task<SeleniumStepResult>> SelectDropdown(string dropdownId, string visibleText, string description)
            => SeleniumHelper.SelectDropdownById(dropdownId, visibleText, description);

        public static Func<IWebDriver, Task<SeleniumStepResult>> EnterRequestedInformation(string text)
            => SeleniumHelper.EnterTextById("RequestedInfo_RequestedInformation", text, "Requested Information field");

        public static Func<IWebDriver, Task<SeleniumStepResult>> EnterReturnedInformation(string text)
            => SeleniumHelper.EnterTextById("ProvidedInfo_ProvidedInformation", text, "Returned Information field");

        public static Func<IWebDriver, Task<SeleniumStepResult>> DashboardSearch(string proposalId)
            => SeleniumHelper.EnterDashboardSearch(proposalId);

        public static Func<IWebDriver, Task<SeleniumStepResult>> RobustClickReplyInRow(string workflowPortion, string requestedInfoId, string description, string buttonText = "Reply", int maxRetries = 3)
            => SeleniumHelper.RobustClickReplyInRow(workflowPortion, requestedInfoId, description, buttonText, maxRetries);

        public static Func<IWebDriver, Task<SeleniumStepResult>> UploadFileById(string inputId, string filePath, string description)
            => SeleniumHelper.UploadFileById(inputId, filePath, description);

        public static Func<IWebDriver, Task<SeleniumStepResult>> RunJavaScript(string script, string description)
            => SeleniumHelper.RunJavaScript(script, description);


    }

}
