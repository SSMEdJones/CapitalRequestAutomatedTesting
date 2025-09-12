using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;
using System.Linq.Expressions;

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

        public static Func<IWebDriver, Task<SeleniumStepResult>> AssertElementTextById(
    string id,
    string expectedText,
    string description)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    var element = wait.Until(drv => drv.FindElement(By.Id(id)));

                    string actualText = element.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(actualText))
                    {
                        actualText = ((IJavaScriptExecutor)driver)
                            .ExecuteScript($"return document.getElementById('{id}')?.textContent?.trim()")
                            ?.ToString();
                    }

                    if (actualText == expectedText.Trim())
                    {
                        return new SeleniumStepResult
                        {
                            Success = true,
                            Message = $"{description} is present and matches expected text."
                        };
                    }

                    string screenshotPath = CaptureScreenshot(driver, $"{description}_text_mismatch");

                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"{description} is present but text differs.\nExpected: '{expectedText}'\nActual: '{actualText}'\nScreenshot: {screenshotPath}",
                        ScreenshotPath = screenshotPath
                    };
                }
                catch (Exception ex)
                {
                    string screenshotPath = CaptureScreenshot(driver, $"{description}_error");

                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Error validating {description}: {ex.Message}\nScreenshot: {screenshotPath}",
                        ScreenshotPath = screenshotPath
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

        //public static Func<IWebDriver, Task<SeleniumStepResult>> ClickWhenVisibleById(string elementId, string description)
        //{
        //    return async driver =>
        //    {
        //        try
        //        {
        //            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        //            var element = wait.Until(ExpectedConditions.ElementExists(By.Id(elementId)));

        //            // Force scroll using JavaScript
        //            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        //            await Task.Delay(500);                    // Wait again for the element to be clickable after scroll

        //            //var element = driver.FindElement(By.Id(dropdownId));
        //            //var yPosition = element.Location.Y;
        //            //((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {yPosition - 100});");

        //            element = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(elementId)));

        //            element.Click();

        //            return new SeleniumStepResult
        //            {
        //                Success = true,
        //                Message = $"Clicked '{description}' after scrolling into view."
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            return new SeleniumStepResult
        //            {
        //                Success = false,
        //                Message = $"Could not click '{description}': {ex.Message}"
        //            };
        //        }
        //    };
        //}

        public static Func<IWebDriver, Task<SeleniumStepResult>> RobustClickById(string elementId, string description, int maxRetries = 3)
        {
            return async driver =>
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var element = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(elementId)));

                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);

                        await Task.Delay(500);
                        element.Click();

                        return new SeleniumStepResult
                        {
                            Success = true,
                            Message = $"✅ Clicked '{description}' on attempt #{attempt}."
                        };
                    }
                    catch (Exception ex) when (attempt < maxRetries)
                    {
                        Debug.WriteLine($"Retry #{attempt} failed: {ex.Message}");
                        await Task.Delay(700);
                    }
                    catch (Exception finalEx)
                    {
                        var screenshotPath = CaptureScreenshot(driver, $"{description}_ClickFailed");

                        return new SeleniumStepResult
                        {
                            Success = false,
                            Message = $"❌ Failed to click '{description}' after {maxRetries} attempts.\nError: {finalEx.Message}\nScreenshot: {screenshotPath}",
                            ScreenshotPath = screenshotPath
                        };
                    }
                }

                var fallbackScreenshot = CaptureScreenshot(driver, $"{description}_ClickExceeded");
                return new SeleniumStepResult
                {
                    Success = false,
                    Message = $"❌ Exceeded max attempts clicking '{description}'. Screenshot saved: {fallbackScreenshot}",
                    ScreenshotPath = fallbackScreenshot
                };
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

        public static Func<IWebDriver, Task<SeleniumStepResult>> EnterTextById(string elementId, string text, string description)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(8));
                    var textarea = wait.Until(drv => drv.FindElement(By.Id(elementId)));

                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", textarea);
                    await Task.Delay(250);

                    textarea.Clear();
                    textarea.SendKeys(text);

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Entered text into '{description}'."
                    };
                }
                catch (Exception ex)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Failed to enter text in '{description}': {ex.Message}"
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> EnterDashboardSearch(string proposalId)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    var filterContainer = wait.Until(d => d.FindElement(By.Id("dashboard_filter")));
                    var input = filterContainer.FindElement(By.CssSelector("input[type='search']"));

                    input.Clear();
                    input.SendKeys(proposalId);
                    await Task.Delay(1000); // Allow table to re-render/filter

                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"Entered proposal ID '{proposalId}' into dashboard search field."
                    };
                }
                catch (Exception ex)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Failed to enter dashboard search value: {ex.Message}"
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> ValidateReviewerDashboardCell(int dashboardOrder, string expectedName, DateTime? expectedDate)
        {
            return async driver =>
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    var row = wait.Until(d =>
                        d.FindElement(By.CssSelector("#dashboard tbody tr"))); // Assume only one filtered row

                    var cells = row.FindElements(By.CssSelector("td")).ToList();
                    int anchorIndex = -1;

                    // Find the last fa-check-circle cell
                    for (int i = 0; i < cells.Count; i++)
                    {
                        if (cells[i].FindElements(By.CssSelector("i.fa-check-circle")).Any())
                        {
                            anchorIndex = i;
                        }
                    }

                    if (anchorIndex == -1)
                    {
                        return new SeleniumStepResult
                        {
                            Success = false,
                            Message = "No check-mark columns (Pending/Submit) found to anchor reviewer columns."
                        };
                    }

                    int reviewerIndex = anchorIndex + dashboardOrder;
                    if (reviewerIndex >= cells.Count)
                    {
                        return new SeleniumStepResult
                        {
                            Success = false,
                            Message = $"Reviewer index {reviewerIndex} is out of bounds for table row with {cells.Count} cells."
                        };
                    }

                    var reviewCell = cells[reviewerIndex];
                    var textParts = reviewCell.Text
                        .Split('\n')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();

                    if (expectedDate != null)
                    {


                        var expectedDateStr = string.Empty;
                        if (expectedDate.HasValue)
                        {
                            expectedDateStr = expectedDate.Value.ToString("MM/dd/yy");
                        }

                        var issues = new List<string>();

                        if (!textParts.Contains(expectedName))
                            issues.Add($"Expected reviewer name '{expectedName}' not found in cell.");

                        if (!textParts.Contains(expectedDateStr))
                            issues.Add($"Expected date '{expectedDateStr}' not found in cell.");

                        var hasInfoIcon = reviewCell.FindElements(By.CssSelector("i.fa-info-circle")).Any();
                        if (!hasInfoIcon)
                            issues.Add("Expected info icon not found in reviewer cell.");

                        if (issues.Any())
                        {
                            return new SeleniumStepResult
                            {
                                Success = false,
                                Message = "Reviewer cell mismatch:\n" + string.Join("\n", issues)
                            };
                        }
                    }
                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = "Reviewer cell matches expected name, date, and icon."
                    };
                }
                catch (Exception ex)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Error during reviewer dashboard cell validation: {ex.Message}"
                    };
                }
            };
        }

        public static Func<IWebDriver, Task<SeleniumStepResult>> RobustClickReplyInRow(
    string workflowPortion,
    string requestedInfoId,
    string description,
    string buttonText = "Reply",
    int maxRetries = 3)
        {
            return async driver =>
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var rows = driver.FindElements(By.XPath("//tbody/tr"));

                        foreach (var row in rows)
                        {
                            var cells = row.FindElements(By.TagName("td"));
                            Debug.WriteLine($"Row has {cells.Count} cells.");

                            if (cells.Count >= 5)
                            {
                                var requestedInfo = cells[4].GetAttribute("textContent")?.Trim();

                                Debug.WriteLine($"Cell[0]: '{cells[0].Text.Trim()}', Cell[4] (raw): '{requestedInfo}'");

                                if (cells[0].Text.Trim() == workflowPortion && requestedInfo == requestedInfoId)
                                {
                                    Debug.WriteLine("✅ Match found. Attempting to locate button...");
                                    var button = row.FindElement(By.XPath($".//button[contains(text(),'{buttonText}')]"));

                                    ((IJavaScriptExecutor)driver)
                                        .ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", button);

                                    await Task.Delay(500);
                                    button.Click();

                                    return new SeleniumStepResult
                                    {
                                        Success = true,
                                        Message = $"✅ Clicked '{description}' in row with WorkflowPortion '{workflowPortion}' and RequestedInfoId '{requestedInfoId}'."
                                    };
                                }
                            }
                            else
                            {
                                Debug.WriteLine("❌ Row skipped due to insufficient cells.");
                            }
                        }

                        //foreach (var row in rows)
                        //{
                        //    var cells = row.FindElements(By.TagName("td"));
                        //    if (cells.Count >= 5 &&
                        //        cells[0].Text.Trim() == workflowPortion &&
                        //        cells[4].Text.Trim() == requestedInfoId)
                        //    {
                        //        var button = row.FindElement(By.XPath($".//button[contains(text(),'{buttonText}')]"));

                        //        ((IJavaScriptExecutor)driver)
                        //            .ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", button);

                        //        await Task.Delay(500);
                        //        button.Click();

                        //        return new SeleniumStepResult
                        //        {
                        //            Success = true,
                        //            Message = $"✅ Clicked '{description}' in row with WorkflowPortion '{workflowPortion}' and RequestedInfoId '{requestedInfoId}' on attempt #{attempt}."
                        //        };
                        //    }
                        //}

                        throw new NoSuchElementException($"No matching row found for WorkflowPortion '{workflowPortion}' and RequestedInfoId '{requestedInfoId}'.");
                    }
                    catch (Exception ex) when (attempt < maxRetries)
                    {
                        Debug.WriteLine($"Retry #{attempt} failed: {ex.Message}");
                        await Task.Delay(700);
                    }
                    catch (Exception finalEx)
                    {
                        var screenshotPath = CaptureScreenshot(driver, $"{description}_ClickFailed");

                        return new SeleniumStepResult
                        {
                            Success = false,
                            Message = $"❌ Failed to click '{description}' after {maxRetries} attempts.\nError: {finalEx.Message}\nScreenshot: {screenshotPath}",
                            ScreenshotPath = screenshotPath
                        };
                    }
                }

                var fallbackScreenshot = CaptureScreenshot(driver, $"{description}_ClickExceeded");
                return new SeleniumStepResult
                {
                    Success = false,
                    Message = $"❌ Exceeded max attempts clicking '{description}'. Screenshot saved: {fallbackScreenshot}",
                    ScreenshotPath = fallbackScreenshot
                };
            };
        }

        //public static Func<IWebDriver, Task<SeleniumStepResult>> RobustClickReplyInRow(string workflowPortion,string requestedInfoId,string description,string buttonText = "Reply",int maxRetries = 3)
        //{
        //    return async driver =>
        //    {
        //        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        //        for (int attempt = 1; attempt <= maxRetries; attempt++)
        //        {
        //            try
        //            {
        //                var rows = driver.FindElements(By.XPath($"//tr[td[1][normalize-space(text())='{workflowPortion}']]"));

        //                foreach (var row in rows)
        //                {
        //                    var cells = row.FindElements(By.TagName("td"));
        //                    if (cells.Count >= 5 && cells[4].Text.Trim() == requestedInfoId)
        //                    {
        //                        var button = row.FindElement(By.XPath($".//button[contains(text(),'{buttonText}')]"));

        //                        ((IJavaScriptExecutor)driver)
        //                            .ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", button);

        //                        await Task.Delay(500);
        //                        button.Click();

        //                        return new SeleniumStepResult
        //                        {
        //                            Success = true,
        //                            Message = $"✅ Clicked '{description}' in row with WorkflowPortion '{workflowPortion}' and RequestedInfoId '{requestedInfoId}' on attempt #{attempt}."
        //                        };
        //                    }
        //                }

        //                throw new NoSuchElementException($"No matching row found for WorkflowPortion '{workflowPortion}' and RequestedInfoId '{requestedInfoId}'.");
        //            }
        //            catch (Exception ex) when (attempt < maxRetries)
        //            {
        //                Debug.WriteLine($"Retry #{attempt} failed: {ex.Message}");
        //                await Task.Delay(700);
        //            }
        //            catch (Exception finalEx)
        //            {
        //                var screenshotPath = CaptureScreenshot(driver, $"{description}_ClickFailed");

        //                return new SeleniumStepResult
        //                {
        //                    Success = false,
        //                    Message = $"❌ Failed to click '{description}' after {maxRetries} attempts.\nError: {finalEx.Message}\nScreenshot: {screenshotPath}",
        //                    ScreenshotPath = screenshotPath
        //                };
        //            }
        //        }

        //        var fallbackScreenshot = CaptureScreenshot(driver, $"{description}_ClickExceeded");
        //        return new SeleniumStepResult
        //        {
        //            Success = false,
        //            Message = $"❌ Exceeded max attempts clicking '{description}'. Screenshot saved: {fallbackScreenshot}",
        //            ScreenshotPath = fallbackScreenshot
        //        };
        //    };
        //}

        //public static Func<IWebDriver, Task<SeleniumStepResult>> ClickReplyButtonByWorkflowPortionAndRequestedInfoId(IWebDriver driver, string workflowPortion, string requestedInfoId)
        //{
        //    return async driver =>
        //    {
        //        try
        //        {
        //            var rows = driver.FindElements(By.XPath("//tr[td[1][normalize-space(text())='" + workflowPortion + "']]"));

        //            foreach (var row in rows)
        //            {
        //                var cells = row.FindElements(By.TagName("td"));
        //                if (cells.Count >= 5 && cells[4].Text.Trim() == requestedInfoId)
        //                {
        //                    var replyButton = row.FindElement(By.XPath(".//button[contains(text(),'Reply')]"));
        //                    replyButton.Click();
        //                    break;
        //                }
        //            }
        //        catch (Exception ex)
        //        {
        //            return new SeleniumStepResult
        //            {
        //                Success = false,
        //                Message = $"No row found with WorkflowPortion '{{workflowPortion}}' and RequestedInfoId '{{requestedInfoId}}'.\": {ex.Message}"
        //            };
        //        }

        //    };

        //        throw new NoSuchElementException($"No row found with WorkflowPortion '{workflowPortion}' and RequestedInfoId '{requestedInfoId}'.");
        //}
        public static Func<IWebDriver, Task<SeleniumStepResult>> NoRequestsMessage()
        {
            return driver =>
            {
                var message = "We couldn't find any Requests for you.";
                var elements = driver.FindElements(By.CssSelector(".noResults h3"));
                bool found = elements.Any(el => el.Text.Trim().Equals(message, StringComparison.OrdinalIgnoreCase));

                var result = new SeleniumStepResult
                {
                    Success = found,
                    Message = found ? "No requests message found." : "Expected no-requests message not found."
                };

                return Task.FromResult(result);
            };
        }

        private static string CaptureScreenshot(IWebDriver driver, string label)
        {
            try
            {
                if (driver is ITakesScreenshot screenshotDriver)
                {
                    var screenshot = screenshotDriver.GetScreenshot();
                    var fileName = $"{label}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var filePath = Path.Combine("Screenshots", fileName);

                    Directory.CreateDirectory("Screenshots");
                    screenshot.SaveAsFile(filePath);

                    Debug.WriteLine($"Screenshot saved: {filePath}");
                    return filePath;
                }
            }
            catch (Exception screenshotEx)
            {
                Debug.WriteLine($"Screenshot capture failed: {screenshotEx.Message}");
            }

            return null;
        }



        //public static Func<IWebDriver, SeleniumStepResult> NoRequestsMessage()
        //{
        //    return driver =>
        //    {
        //        var message = "We couldn't find any Requests for you.";
        //        var elements = driver.FindElements(By.CssSelector(".noResults h3"));
        //        bool found = elements.Any(el => el.Text.Trim().Equals(message, StringComparison.OrdinalIgnoreCase));

        //        return new SeleniumStepResult
        //        {
        //            Success = found,
        //            Message = found ? "No requests message found." : "Expected no-requests message not found."
        //        };
        //    };
        //}

    }
}
