using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SSMAuthenticationCore;
using SSMAuthenticationCore.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
using System.Reflection;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;
using WorkflowAction = CapitalRequestAutomatedTesting.UI.Models.WorkflowAction;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IWorkflowControllerService
    {

        Task<DashboardInitializationResult> InitializeDashboardItemsAsync();
        Task<List<CapitalRequest.API.Models.WorkflowAction>> GetWorkflowActionsFromApiAsync(int? id);
        Request GetRequestById(int id);
        //void RegisterWorkflowActions(List<WorkflowAction> actions);
        //WorkflowTestResult RunDynamicScenario(string scenarioId, string requestId);
        WorkflowTestResult RunLoadButtonTest(WorkflowTestContext context);
        WorkflowTestResult RunLoadVerifyButtonTest(WorkflowTestContext context);
        WorkflowTestResult RunLoadApproveWBSButtonTest(string reqId, string groupName);
        WorkflowTestResult RunLoadReplyButtonTest(string reqId, string identifier);
        WorkflowTestResult RunLoadApproveButtonTest(string reqId, string identifier);
        WorkflowTestResult RunExpectMessageTest(string reqId, string identifier, string buttonText, string expectedMessage);
        WorkflowTestResult RunLoadWorkflowDashboardTest(string id);
        WorkflowTestResult RunLoadRequestTest(string requestId);
        Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardItemsFromApiAsync();
        List<WorkflowAction> GetActionsFromWorkflowDashboard(string id);
        string GenerateScenarioId(string identifier, string action);
        ActionDecision DecideTestAction(Request request, string dashboardText);
        AppKeyValues GetAppKeyValueByKey(string AppName, string LookupKey);

    }

    public class WorkflowControllerService : IWorkflowControllerService
    {

        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        //private readonly ISSMWorkflowServices _ssmWorkflowServices;
        //private readonly ICapitalRequestServices _capitalRequestServices;
        //private readonly IUserContextService _userContextService;
        //private Dictionary<string, Func<string, WorkflowTestResult>> _scenarioMap;
        //private List<CapitalRequest.API.Models.WorkflowAction> _workflowActions;
        //private List<SSMWorkflow.API.Models.Dashboard> _dashboardItems;
        //private readonly IMapper _mapper;

        public WorkflowControllerService(
         ISSMWorkflowServices ssmWorkflowServices,
         ICapitalRequestServices capitalRequestServices,
         IUserContextService userContextService,
         IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _userContextService = userContextService;
            _mapper = mapper;
        }


        //public Dictionary<string, Func<string, WorkflowTestResult>> _scenarioMap;
        //public List<SSMWorkflow.API.Models.Dashboard> _dashboardItems;
        //public List<CapitalRequest.API.Models.WorkflowAction> _workflowActions;

        // Async Initialization Method

        public async Task<DashboardInitializationResult> InitializeDashboardItemsAsync()
        {
            var workflowActions = await GetWorkflowActionsFromApiAsync(null);
            var dashboardItems = await GetDashboardItemsFromApiAsync();

            return new DashboardInitializationResult
            {
                WorkflowActions = workflowActions,
                DashboardItems = dashboardItems
            };
        }


        public async Task<List<CapitalRequest.API.Models.WorkflowAction>> GetWorkflowActionsFromApiAsync(int? id)
        {
            //TODO Implement persona
            var userId = _userContextService.UserId;

            var applicationUser = await _capitalRequestServices.GetApplicationUser(userId);

            var filter = new WorkflowActionSearchFilter { Id = id, UserId = applicationUser.UserId, Email = applicationUser.Email };

            var workflowActions = await _capitalRequestServices.GetAllWorkflowActions(filter);

            return workflowActions;
        }

        public Request GetRequestById(int id)
        {
            var initResult = InitializeDashboardItemsAsync().Result;
            var actions = initResult.WorkflowActions;
            var dashboardItems = initResult.DashboardItems;

            var item = dashboardItems.FirstOrDefault(d => d.ReqId == id);
            if (item == null) return null;

            var request = new Request
            {
                Id = item.ReqId,
                ITReviewStatus = item.ITReviewStatus,
                FacilitiesReviewStatus = item.FacilitiesReviewStatus,
                SupplyChainReviewStatus = item.SupplyChainReviewStatus,
                EPMOReviewStatus = item.EPMOReviewStatus,
                PurchasingReviewStatus = item.PurchasingReviewStatus,
                FinanceReviewStatus = item.FinanceReviewStatus,
                VPOpsReviewStatus = item.VPOpsReviewStatus,
                VPFinanceReviewStatus = item.VPFinanceReviewStatus
            };

            var proposal = _capitalRequestServices.GetProposal(item.ReqId).Result;
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId).Result;
            var workflowStep = new WorkFlowStepViewModel();
            int? stepNumber = null;

            if (workflowSteps != null)
            {
                workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);
            }

            var workflowTemplates = _capitalRequestServices.GetAllWorkflowTemplates(new CapitalRequest.API.DataAccess.Models.WorkflowTemplateSearchFilter()).Result;
            if (workflowStep != null)
            {
                stepNumber = workflowTemplates
                    .Where(x => x.StepName == workflowStep.StepName)
                    .Select(x => x.StepNumber)
                    .First();

            }
            //TODO Constant
            var allGroups = _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter()).Result
                .Where(x => x.StepNumber.HasValue && x.StepNumber.Value == stepNumber && x.ReviewerType == "Review")
                .Select(x => x.Name)
                .ToArray();

            var groupsThatHaveTakenAction = allGroups
                .Where(group => DashboardHelper.HasGroupTakenAction(request, group))
                .ToList();

            return request;
        }

        //public void RegisterWorkflowActions(List<WorkflowAction> actions)
        //{
        //    _scenarioMap = new Dictionary<string, Func<string, WorkflowTestResult>>(StringComparer.OrdinalIgnoreCase);


        //    foreach (var action in actions)
        //    {
        //        var methodInfo = this.GetType().GetMethod(action.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //        if (methodInfo != null)
        //        {
        //            var scenarioId = action.ScenarioId;
        //            var targetId = action.TargetId;
        //            var identifier = action.Identifier;
        //            var reqid = action.ReqId;

        //            _scenarioMap[scenarioId] = (id) =>
        //            {
        //                return (WorkflowTestResult)methodInfo.Invoke(this, new object[] { reqid, identifier });
        //            };
        //        }
        //    }

        //}

        //public WorkflowTestResult RunDynamicScenario(string scenarioId, string requestId)
        //{
        //    if (_scenarioMap.TryGetValue(scenarioId, out var testMethod))
        //    {
        //        return testMethod(requestId);
        //    }

        //    return new WorkflowTestResult
        //    {
        //        Passed = false,
        //        Message = $"Unknown scenario: {scenarioId}"
        //    };
        //}

        private ChromeOptions GetChromeOptions()
        {
            var options = new ChromeOptions();
            options.BinaryLocation = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--remote-debugging-port=9222");
            options.AddArgument("--disable-session-crashed-bubble");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-popup-blocking");

            return options;
        }

        public WorkflowTestResult RunLoadButtonTest(WorkflowTestContext context)
        {
            return SeleniumHelper.RunWithSafeChromeDriver(driver =>
            {
                var baseUrl = GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;
                var url = $"{baseUrl}/Proposal/WorkflowActions/{context.ReqId}";
                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                try
                {
                    var button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.XPath($"//tr[td[1][text()='{context.Identifier}']]//button[text()='{context.ButtonText}']")));
                    button.Click();

                    if (!string.IsNullOrEmpty(context.WaitForElementId))
                    {
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(context.WaitForElementId)));
                        if (context.WaitForElementId == "btnRequestMoreInfo" && context.SelectedAction == "Request More Info")
                        {
                            var selectedButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("btnRequestMoreInfo")));
                            selectedButton.Click();

                            return FillOutVerifyForm(driver, wait);
                        }
                    }

                    return new WorkflowTestResult
                    {
                        Passed = true,
                        Message = $"'{context.ButtonText}' button clicked successfully for '{context.Identifier}' as '{context.ImpersonatedUserId}'."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new WorkflowTestResult
                    {
                        Passed = false,
                        Message = $"Failed to click '{context.ButtonText}' or find expected element for '{context.Identifier}'."
                    };
                }
            });
        }

        //public WorkflowTestResult RunLoadButtonTest(string reqId, string identifier, string buttonText, string waitForElementId = null)
        //{
        //    return SeleniumHelper.RunWithSafeChromeDriver(driver =>
        //    {

        //        var baseUrl = GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;

        //        var url = $"{baseUrl}/Proposal/WorkflowActions/{reqId}";
        //        driver.Navigate().GoToUrl(url);

        //        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        //        try
        //        {
        //            // Click the button in the correct row
        //            var button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
        //                By.XPath($"//tr[td[1][text()='{identifier}']]//button[text()='{buttonText}']")));
        //            button.Click();

        //            if (!string.IsNullOrEmpty(waitForElementId))
        //            {
        //                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(waitForElementId)));
        //            }

        //            return new WorkflowTestResult
        //            {
        //                Passed = true,
        //                Message = $"'{buttonText}' button clicked successfully for '{identifier}'."
        //            };
        //        }
        //        catch (WebDriverTimeoutException)
        //        {
        //            return new WorkflowTestResult
        //            {
        //                Passed = false,
        //                Message = $"Failed to click '{buttonText}' or find expected element for '{identifier}'."
        //            };
        //        }
        //    });
        //}

        public WorkflowTestResult RunLoadVerifyButtonTest(WorkflowTestContext context)
        {


            if (!int.TryParse(context.ReqId, out int requestId))
                throw new ArgumentException("Invalid request ID");

            var request = GetRequestById(requestId);
            var decision = DecideTestAction(request, context.Identifier);

            context.ButtonText = "Verify";
            context.WaitForElementId = decision.ElementId;

            var result = RunLoadButtonTest(context);

            if (!result.Passed)
                return result;

            // Only run the form fill if it's the Verify button
            //if (context.ButtonText == "Verify")
            //{
            //    return SeleniumHelper.RunWithSafeChromeDriver(driver =>
            //    {
            //        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //        return FillOutVerifyForm(driver, wait);
            //    });
            //}

            return result;
        }

        //public WorkflowTestResult RunLoadVerifyButtonTest(string reqId, string groupName)
        //{
        //    InitializeDashboardItemsAsync().Wait(); // Assuming this is an instance method

        //    if (!int.TryParse(reqId, out int requestId))
        //        throw new ArgumentException("Invalid request ID");

        //    var request = GetRequestById(requestId);
        //    var decision = DecideTestAction(request, groupName);

        //    switch (decision.ActionType)
        //    {
        //        case WorkflowActionType.ClickButton:
        //            return RunLoadButtonTest(reqId, groupName, "Verify", decision.ElementId);

        //        case WorkflowActionType.ExpectMessage:
        //            return RunExpectMessageTest(reqId, groupName, "Verify", decision.ExpectedMessage);

        //        default:
        //            throw new InvalidOperationException("Unknown test action type");
        //    }
        //}

        public WorkflowTestResult RunLoadApproveWBSButtonTest(string reqId, string groupName)
        {
            InitializeDashboardItemsAsync().Wait(); // Assuming this is an instance method

            if (!int.TryParse(reqId, out int requestId))
                throw new ArgumentException("Invalid request ID");

            var request = GetRequestById(requestId);
            var decision = DecideTestAction(request, groupName);

            decision.ElementId = "btnApproveWBS";
            switch (decision.ActionType)
            {
                case WorkflowActionType.ClickButton:
                //return RunLoadButtonTest(reqId, groupName, "Approve WBS", decision.ElementId);

                case WorkflowActionType.ExpectMessage:
                    return RunExpectMessageTest(reqId, groupName, "Verify", decision.ExpectedMessage);

                default:
                    throw new InvalidOperationException("Unknown test action type");
            }
        }

        public static string ExtractGroupName(string dashboardText)
        {
            // Split on the first dash and trim the result
            var parts = dashboardText.Split('-', 2);
            var returnVal = parts.Length > 1 ? parts[1].Trim() : dashboardText.Trim();
            return returnVal;
            //return parts.Length > 1 ? parts[1].Trim().Replace(" ", string.Empty) : dashboardText.Trim();
        }

        public WorkflowTestResult RunLoadReplyButtonTest(string reqId, string identifier)
        {
            return new WorkflowTestResult();
            //return RunLoadButtonTest(reqId, identifier, "Reply", "RequestedInfo_RequestedInformation");
        }

        public WorkflowTestResult RunLoadApproveButtonTest(string reqId, string identifier)
        {
            return new WorkflowTestResult();

            //return RunLoadButtonTest(reqId, identifier, "Approve");
        }


        public WorkflowTestResult RunExpectMessageTest(string reqId, string identifier, string buttonText, string expectedMessage)
        {
            return SeleniumHelper.RunWithSafeChromeDriver(driver =>
            {

                var baseUrl = GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;
                var url = $"{baseUrl}/Proposal/WorkflowActions/{reqId}?testmode=true";

                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                try
                {
                    var button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.XPath($"//tr[td[1][text()='{identifier}']]//button[text()='{buttonText}']")));

                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", button);

                    Thread.Sleep(1000); // Allow time for DOM update

                    var elements = driver.FindElements(By.XPath("//*[contains(text(), 'Thank you')]"));

                    foreach (var el in elements)
                    {
                        Debug.WriteLine($"Found element: Displayed={el.Displayed}, Text='{el.Text}'");
                    }

                    var visibleElement = elements.FirstOrDefault(e => e.Displayed);

                    if (visibleElement == null)
                    {
                        return new WorkflowTestResult
                        {
                            Passed = false,
                            Message = "No visible 'Thank you' message found."
                        };
                    }

                    var actualMessage = visibleElement.Text;
                    bool success = actualMessage == expectedMessage;

                    return new WorkflowTestResult
                    {
                        Passed = success,
                        Message = success
                            ? $"Verify' button clicked Expected message. '{actualMessage}'."
                            : $"Expected '{expectedMessage}', but found '{actualMessage}'"
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new WorkflowTestResult
                    {
                        Passed = false,
                        Message = "Timed out waiting for the expected message element."
                    };
                }
            });
        }

        public WorkflowTestResult RunLoadWorkflowDashboardTest(string id)
        {
            return SeleniumHelper.RunWithSafeChromeDriver(driver =>
            {
                var baseUrl = "http://caps-dev.ssmhc.com/CapitalRequest";
                baseUrl = "https://localhost:27867";

                var debug = GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;

                var url = $"{baseUrl}/Proposal/WorkflowActions/{id}";
                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var heading = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                    By.XPath("//h2[text()='Workflow Dashboard']")));

                return new WorkflowTestResult
                {
                    Passed = heading.Displayed,
                    Message = "Dashboard loaded successfully."
                };
            });
        }

        public WorkflowTestResult RunLoadRequestTest(string requestId)
        {
            return RunTest(requestId, "divReqId");
        }
        private WorkflowTestResult RunTest(string linkText, string elementId)
        {
            var result = new WorkflowTestResult();
            var options = GetChromeOptions();

            try
            {
                using (var driver = new ChromeDriver(options))
                {
                    driver.Navigate().GoToUrl("http://caps-dev.ssmhc.com/CapitalRequest");
                    Thread.Sleep(2000);

                    IWebElement element = driver.FindElement(By.LinkText(linkText));
                    string elementText = element.GetAttribute("innerText");

                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);


                    Debug.WriteLine("Element text: " + elementText);
                    if (elementText.Contains(linkText))
                    {
                        result.Passed = true;
                        result.Message = $"Request with ID '{linkText}' loaded successfully.";
                    }
                    else
                    {
                        result.Passed = false;
                        result.Message = $"Request with ID '{linkText}' failed to load.";
                    }
                }
            }
            catch (NoSuchElementException ex)
            {
                result.Passed = false;
                result.Message = $"Element not found: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Unexpected error: {ex.Message}";
            }

            return result;
        }

        public async Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardItemsFromApiAsync()
        {
            var dashboardFilter = new DashboardSearchFilter
            {
                CapitalFundingYear = DateTime.Now.Year,
                HistoricalDataOnly = false,
            };


            var workflowActions = (await GetWorkflowActionsFromApiAsync(null))
             .GroupBy(x => x.ProposalId)
             .Select(g => g.First())
             .ToList();


            var dashboardData = _ssmWorkflowServices.GetCapitalRequestDashboard(dashboardFilter).Result
                .Where(x => x.SubmittedBy != null && x.IsMovingForward && x.ProjectNumber == null)
                .ToList();

            dashboardData = (from data in dashboardData
                             join action in workflowActions on data.ReqId equals action.ProposalId
                             select data)
                .ToList();

            return dashboardData;
        }

        public List<WorkflowAction> GetActionsFromWorkflowDashboard(string id)
        {

            var allWorkflowActions = new List<CapitalRequest.API.Models.WorkflowAction>();
            if (id != null && int.TryParse(id.ToString(), out int parsedId))
            {
                allWorkflowActions = GetWorkflowActionsFromApiAsync(parsedId).Result.ToList();
            }
            else
            {
                allWorkflowActions = GetWorkflowActionsFromApiAsync(null).Result.ToList();
            }

            var workflowActions = allWorkflowActions
                .Where(x => x.ProposalId == Convert.ToInt32(id))
                .Select(x => new WorkflowAction
                {
                    ReqId = x.ProposalId.ToString(),
                    Identifier = x.WorkflowPortion,
                    ActionName = x.ButtonCaption,
                    MethodName = $"RunLoad{x.ButtonCaption.Replace(" ", string.Empty)}ButtonTest",
                    TargetId = x.WorkflowPortion.ToLower().Replace(" ", "_").Replace("-", ""),
                    ScenarioId = GenerateScenarioId(x.WorkflowPortion, x.ButtonCaption)
                })
                .ToList();

            return workflowActions;
        }

        public string GenerateScenarioId(string identifier, string action)
        {
            var debug = $"{identifier.ToLower().Replace(" ", "_").Replace("-", "")}_{action.ToLower().Replace(" ", "_").Replace("-", "")}";
            return $"{identifier.ToLower().Replace(" ", "_").Replace("-", "")}_{action.ToLower().Replace(" ", "_").Replace("-", "")}";
        }


        public ActionDecision DecideTestAction(Request request, string dashboardText)
        {

            var groupName = ExtractGroupName(dashboardText);

            if (request != null && DashboardHelper.HasGroupTakenAction(request, groupName))
            {
                return new ActionDecision
                {
                    ActionType = WorkflowActionType.ExpectMessage,
                    ExpectedMessage = Constants.RESPONSE_ACTION_TAKEN
                };
            }

            return new ActionDecision
            {
                ActionType = WorkflowActionType.ClickButton,
                ElementId = "btnRequestMoreInfo"
            };
        }

        public AppKeyValues GetAppKeyValueByKey(string AppName, string LookupKey)
        {
            ConfigurationSettings configuration = new ConfigurationSettings();
            var appKeyValue = configuration.GetAppKeyValues(AppName).Where(kv => kv.LookupKey == LookupKey).FirstOrDefault();

            return appKeyValue;
        }

        private WorkflowTestResult FillOutVerifyForm(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                var dropdown = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("RequestedInfo_ReviewerGroupId")));
                var selectElement = new SelectElement(dropdown);
                selectElement.SelectByValue("3");

                var textbox = driver.FindElement(By.Id("RequestedInfo_RequestedInformation"));
                textbox.Clear();
                textbox.SendKeys("This message brought to you by Workflow Automated Testing.");

                var submitButton = driver.FindElement(By.Id("btnSubmitMoreInfo"));
                submitButton.Click();

                //TODO Verify message 
                //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("confirmationElementId")));
                //var elements = driver.FindElements(By.XPath("//*[contains(text(), 'Thank you')]"));

                //foreach (var el in elements)
                //{
                //    Debug.WriteLine($"Found element: Displayed={el.Displayed}, Text='{el.Text}'");
                //}

                //var visibleElement = elements.FirstOrDefault(e => e.Displayed);

                //if (visibleElement == null)
                //{
                //    return new WorkflowTestResult
                //    {
                //        Passed = false,
                //        Message = "No visible 'Thank you' message found."
                //    };
                //}

                //var actualMessage = visibleElement.Text;
                //bool success = actualMessage == expectedMessage;

                //return new WorkflowTestResult
                //{
                //    Passed = success,
                //    Message = success
                //        ? $"Verify' button clicked Expected message. '{actualMessage}'."
                //        : $"Expected '{expectedMessage}', but found '{actualMessage}'"
                //};


                return new WorkflowTestResult
                {
                    Passed = true,
                    Message = "Verify form submitted successfully."
                };
            }
            catch (Exception ex)
            {
                return new WorkflowTestResult
                {
                    Passed = false,
                    Message = $"Error submitting verify form: {ex.Message}"
                };
            }
        }

    }
}
