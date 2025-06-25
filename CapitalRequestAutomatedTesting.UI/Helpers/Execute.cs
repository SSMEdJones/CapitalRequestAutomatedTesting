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

        public static Func<IWebDriver, Task<SeleniumStepResult>> ClickWhenVisibleById(string id, string description)
            => SeleniumHelper.ClickWhenVisibleById(id, description);

        public static Func<IWebDriver, Task<SeleniumStepResult>> SelectDropdown(string dropdownId, string visibleText, string description)
            => SeleniumHelper.SelectDropdownById(dropdownId, visibleText, description);



        // You could later add:
        // TypeTextIntoField(...)
        // SelectDropdownOption(...)
        // SubmitForm(...)
    }

}
