using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
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

        public static Func<IWebDriver, Task<SeleniumStepResult>> ValidateElementIsVisibleById(string elementId, string description)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var element = wait.Until(drv =>
                    {
                        var el = drv.FindElement(By.Id(elementId));
                        return el.Displayed && el.Enabled ? el : null;
                    });

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"{description} is visible and enabled."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"{description} was not visible or enabled within timeout."
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> NavigateToRelativePath(string baseUrl, string relativePath)
        {
            var fullUrl = $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
            return NavigateToUrl(fullUrl);
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> NavigateToUrl(string url)
        {
            return async driver =>
            {
                driver.Navigate().GoToUrl(url);

                await Task.Delay(1000); // Optional: basic wait

                return new SeleniumStepResult
                {
                    Success = true,
                    Message = $"Navigated to {url}"
                };
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> ValidateElementById(string elementId, string description)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var element = wait.Until(drv =>
                    {
                        var el = drv.FindElement(By.Id(elementId));
                        return el.Displayed && el.Enabled ? el : null;
                    });

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"{description} is visible and enabled."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"{description} not found or not interactable."
                    };
                }
            };
        }


        public static Func<IWebDriver, Task<SeleniumStepResult>> ValidateElementWithText(string expectedText)
        {
            return ValidateElementWithText("td", expectedText);
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> ValidateElementWithText(string tag, string expectedText)
        {
            return async driver =>
            {
                try
                {
                    var xpath = $"//{tag.ToLower()}[normalize-space()='{expectedText}']";
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var element = wait.Until(drv => drv.FindElement(By.XPath(xpath)));

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Found <{tag}> element containing text: '{expectedText}'."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Could not find <{tag}> element with text: '{expectedText}'."
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> ValidateButtonInRowWithText(string rowText, string buttonText)
        {
            return async driver =>
            {
                try
                {
                    var xpath = $"//tr[td[normalize-space()='{rowText}']]//button[normalize-space()='{buttonText}']";
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var button = wait.Until(drv => drv.FindElement(By.XPath(xpath)));

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Found <button> with text '{buttonText}' in row with text '{rowText}'."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Button '{buttonText}' not found in row containing '{rowText}'."
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> AssertElementNotPresentById(string id, string description)
        {
            return async driver =>
            {
                try
                {
                    var elements = driver.FindElements(By.Id(id));
                    if (elements.Count == 0)
                    {
                        return new SeleniumStepResult
                        {
                            Success = true,
                            Message = $"{description} is not present — as expected"
                        };
                    }

                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"{description} is unexpectedly present"
                    };
                }
                catch (Exception ex)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Error checking presence of {description}: {ex.Message}"
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> ValidateElementTextIsEmpty(string elementId, string description)
        {
            return async driver =>
            {
                try
                {
                    var element = driver.FindElement(By.Id(elementId));
                    var text = element.Text.Trim();

                    if (string.IsNullOrEmpty(text))
                    {
                        return new SeleniumStepResult
                        {
                            Success = true,
                            Message = $"{description} is empty as expected"
                        };
                    }

                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"{description} contains unexpected text: '{text}'"
                    };
                }
                catch (NoSuchElementException)
                {
                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"{description} was not rendered — assuming no rejection message"
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> ClickButtonInRow(string workflowPortion, string buttonText)
        {
            return async driver =>
            {
                try
                {
                    var xpath = $"//tr[td[normalize-space()='{workflowPortion}']]//button[normalize-space()='{buttonText}']";
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var button = wait.Until(drv => drv.FindElement(By.XPath(xpath)));
                    button.Click();

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Clicked '{buttonText}' in row with WorkflowPortion '{workflowPortion}'."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Could not find button '{buttonText}' in row with WorkflowPortion '{workflowPortion}'."
                    };
                }
            };
        }


        public static Func<IWebDriver, Task<SeleniumStepResult>> ClickButtonById(string buttonId, string buttonText)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var button = wait.Until(drv => drv.FindElement(By.Id(buttonId)));
                    button.Click();
                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Clicked '{buttonText}' ."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Could not find button '{buttonText}'."
                    };
                }
               
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> ClickWhenVisibleById(string elementId, string description)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    var element = wait.Until(ExpectedConditions.ElementExists(By.Id(elementId)));

                    // Force scroll using JavaScript
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    await Task.Delay(500);                    // Wait again for the element to be clickable after scroll

                    //var element = driver.FindElement(By.Id(dropdownId));
                    //var yPosition = element.Location.Y;
                    //((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {yPosition - 100});");

                    element = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(elementId)));

                    element.Click();

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Clicked '{description}' after scrolling into view."
                    };
                }
                catch (Exception ex)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Could not click '{description}': {ex.Message}"
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> SelectDropdownById(string dropdownId, string visibleText, string description)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(8));
                    var dropdown = wait.Until(drv => drv.FindElement(By.Id(dropdownId)));

                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", dropdown);
                    await Task.Delay(200); // Give layout time to settle after scroll

                    var selectElement = new SelectElement(dropdown);
                    selectElement.SelectByText(visibleText);

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Selected '{visibleText}' from {description} dropdown."
                    };
                }
                catch (Exception ex)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Failed to select '{visibleText}' from {description}: {ex.Message}"
                    };
                }
            };
        }

        
    }
}
