using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.Browser;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;
using WorkflowAction = CapitalRequestAutomatedTesting.UI.Models.WorkflowAction;
//using WorkflowAction = CapitalRequest.API.DataAccess.Models.WorkflowAction;

namespace CapitalRequestAutomatedTesting.UI.Services
{

    public class WorkflowControllerService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

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

        public Dictionary<string, Func<string, WorkflowTestResult>> _scenarioMap;
        public List<SSMWorkflow.API.Models.Dashboard> _dashboardItems;
        public List<CapitalRequest.API.Models.WorkflowAction> _workflowActions;

        // Async Initialization Method
        public async Task InitializeDashboardItemsAsync()
        {
            _workflowActions = await GetWorkflowActionsFromApiAsync(null);
            _dashboardItems = await GetDashboardItemsFromApiAsync();
        }

        public async Task<List<CapitalRequest.API.Models.WorkflowAction>> GetWorkflowActionsFromApiAsync(int? id)
        {
            //TODO Implement persona
            var userId = _userContextService.UserId;

            var applicationUser = await _capitalRequestServices.GetApplicationUser(userId);

            var filter = new WorkflowActionSearchFilter {Id = id, UserId = applicationUser.UserId, Email = applicationUser.Email};

            var workflowActions = await _capitalRequestServices.GetAllWorkflowActions(filter);

            return workflowActions;
        }

        public Request GetRequestById(int id)
        {
            var item = _dashboardItems.FirstOrDefault(d => d.ReqId == id);
            if (item == null) return null;

            var allGroups = new[]
            {
                "EPMO", "Facilities", "Finance", "IT",
                "Purchasing", "SupplyChain", "VPFinance", "VPOps"
            };

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
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId.Value).Result;
            var workflowStep = new WorkFlowStepViewModel();
            int?stepNumber = null;
            
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
            allGroups = _capitalRequestServices.GetAllReviewerGroups(new CapitalRequest.API.DataAccess.Models.ReviewerGroupSearchFilter()).Result
                .Where(x => x.StepNumber.HasValue && x.StepNumber.Value == stepNumber && x.ReviewerType == "Review")
                .Select(x => x.Name)
                .ToArray();

            var groupsThatHaveTakenAction = allGroups
                .Where(group => DashboardHelper.HasGroupTakenAction(request, group))
                .ToList();

            return request;
        }

        public void RegisterWorkflowActions(List<WorkflowAction> actions)
        {
            _scenarioMap = new Dictionary<string, Func<string, WorkflowTestResult>>(StringComparer.OrdinalIgnoreCase);


            foreach (var action in actions)
            {
                var methodInfo = this.GetType().GetMethod(action.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (methodInfo != null)
                {
                    var scenarioId = action.ScenarioId;
                    var targetId = action.TargetId;
                    var identifier = action.Identifier;
                    var reqid = action.ReqId;

                    _scenarioMap[scenarioId] = (id) =>
                    {
                        return (WorkflowTestResult)methodInfo.Invoke(this, new object[] { reqid, identifier });
                    };
                }
            }

        }

        public WorkflowTestResult RunDynamicScenario(string scenarioId, string requestId)
        {
            if (_scenarioMap.TryGetValue(scenarioId, out var testMethod))
            {
                return testMethod(requestId);
            }

            return new WorkflowTestResult
            {
                Passed = false,
                Message = $"Unknown scenario: {scenarioId}"
            };
        }

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

        public WorkflowTestResult RunLoadButtonTest(string reqId, string identifier, string buttonText, string waitForElementId = null)
        {
            return SeleniumHelper.RunWithSafeChromeDriver(driver =>
            {
                var baseUrl = "http://caps-dev.ssmhc.com/CapitalRequest";
                baseUrl = "https://localhost:27867";

                var url = $"{baseUrl}/Proposal/WorkflowActions/{reqId}";
                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                try
                {
                    // Click the button in the correct row
                    var button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.XPath($"//tr[td[1][text()='{identifier}']]//button[text()='{buttonText}']")));
                    button.Click();

                    if (!string.IsNullOrEmpty(waitForElementId))
                    {
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(waitForElementId)));
                    }

                    return new WorkflowTestResult
                    {
                        Passed = true,
                        Message = $"'{buttonText}' button clicked successfully for '{identifier}'."
                    };
                }
                catch (WebDriverTimeoutException)
                {
                    return new WorkflowTestResult
                    {
                        Passed = false,
                        Message = $"Failed to click '{buttonText}' or find expected element for '{identifier}'."
                    };
                }
            });
        }

        //public TestResultModel RunLoadVerifyButtonTest(string reqId, string identifier)
        //{
        //    return RunLoadButtonTest(reqId, identifier, "Verify", "btnRequestMoreInfo");
        //}

        public WorkflowTestResult RunLoadVerifyButtonTest(string reqId, string groupName)
        {
            InitializeDashboardItemsAsync().Wait(); // Assuming this is an instance method

            if (!int.TryParse(reqId, out int requestId))
                throw new ArgumentException("Invalid request ID");

            var request = GetRequestById(requestId);
            var decision = DecideTestAction(request, groupName);

            switch (decision.ActionType)
            {
                case WorkflowActionType.ClickButton:
                    return RunLoadButtonTest(reqId, groupName, "Verify", decision.ElementId);

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
            return parts.Length > 1 ? parts[1].Trim().Replace(" ", string.Empty) : dashboardText.Trim();
        }

        public WorkflowTestResult RunLoadReplyButtonTest(string reqId, string identifier)
        {
            return RunLoadButtonTest(reqId, identifier, "Reply", "RequestedInfo_RequestedInformation");
        }

        public WorkflowTestResult RunLoadApproveButtonTest(string reqId, string identifier)
        {
            return RunLoadButtonTest(reqId, identifier, "Approve");
        }


        public WorkflowTestResult RunExpectMessageTest(string reqId, string identifier, string buttonText, string expectedMessage)
        {
            return SeleniumHelper.RunWithSafeChromeDriver(driver =>
            {
                var baseUrl = "https://localhost:27867";
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


            var workflowActions = _workflowActions
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
                    ActionName = x.ActionType,
                    MethodName = $"RunLoad{x.ActionType}ButtonTest",
                    TargetId = x.WorkflowPortion.ToLower().Replace(" ", "_").Replace("-", ""),
                    ScenarioId = GenerateScenarioId(x.WorkflowPortion, x.ActionType)
                })
                .ToList();

            return workflowActions;
            //var testModel = SeleniumHelper.RunWithSafeChromeDriver(driver =>
            //{
            //    var result = new WorkflowTestResult();
            //    var workflowActions = new List<WorkflowAction>();

            //    var baseUrl = "http://caps-dev.ssmhc.com/CapitalRequest";
            //    baseUrl = "https://localhost:27867";

            //    var url = $"{baseUrl}/Proposal/WorkflowActions/{id}";

            //    driver.Navigate().GoToUrl(url);

            //    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //    wait.Until(d => d.FindElement(By.CssSelector("table#admintable tbody")));

            //    var rows = driver.FindElements(By.CssSelector("table#admintable tbody tr"));

            //    foreach (var row in rows)
            //    {
            //        try
            //        {
            //            var identifier = row.FindElement(By.CssSelector("td:nth-child(1)")).Text.Trim();
            //            var button = row.FindElement(By.CssSelector("button[name='btnAction']"));
            //            var actionName = button.Text.Trim();

            //            var reqIdElement = driver.FindElement(By.Id("divReqId"));
            //            var reqIdText = reqIdElement.Text; // e.g., "Req Id: 2906"
            //            var reqId = reqIdText.Split(':')[1].Trim(); // "2906"

            //            if (!string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(actionName))
            //            {
            //                var scenarioId = GenerateScenarioId(identifier, actionName);


            //                var methodName = $"RunLoad{actionName}ButtonTest";
            //                var targetId = identifier.ToLower().Replace(" ", "_").Replace("-", "");

            //                workflowActions.Add(new WorkflowAction
            //                {
            //                    ReqId = reqId,
            //                    Identifier = identifier,
            //                    ActionName = actionName,
            //                    ScenarioId = scenarioId,
            //                    MethodName = methodName,
            //                    TargetId = targetId
            //                });

            //            }
            //        }
            //        catch (NoSuchElementException)
            //        {
            //            // Optionally log or skip rows without expected structure
            //        }
            //    }

            //    result.Passed = true;
            //    result.Message = $"Found {workflowActions.Count} Workflow Actions.";
            //    result.WorkflowActions = workflowActions;

            //    return result;
            //});

            //if (testModel.Passed)
            //{
            //    return testModel.WorkflowActions;
            //}
            //else
            //{
            //    throw new Exception(testModel.Message);
            //}
        }

        //public List<WorkflowAction> GetActionsFromWorkflowDashboard(string id)
        //{
        //    var testModel = SeleniumHelper.RunWithSafeChromeDriver(driver =>
        //    {
        //        var result = new WorkflowTestResult();
        //        var workflowActions = new List<WorkflowAction>();

        //        var baseUrl = "http://caps-dev.ssmhc.com/CapitalRequest";
        //        baseUrl = "https://localhost:27867";

        //        var url = $"{baseUrl}/Proposal/WorkflowActions/{id}";

        //        driver.Navigate().GoToUrl(url);

        //        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        //        wait.Until(d => d.FindElement(By.CssSelector("table#admintable tbody")));

        //        var rows = driver.FindElements(By.CssSelector("table#admintable tbody tr"));

        //        foreach (var row in rows)
        //        {
        //            try
        //            {
        //                var identifier = row.FindElement(By.CssSelector("td:nth-child(1)")).Text.Trim();
        //                var button = row.FindElement(By.CssSelector("button[name='btnAction']"));
        //                var actionName = button.Text.Trim();

        //                var reqIdElement = driver.FindElement(By.Id("divReqId"));
        //                var reqIdText = reqIdElement.Text; // e.g., "Req Id: 2906"
        //                var reqId = reqIdText.Split(':')[1].Trim(); // "2906"

        //                if (!string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(actionName))
        //                {
        //                    var scenarioId = GenerateScenarioId(identifier, actionName);


        //                    var methodName = $"RunLoad{actionName}ButtonTest";
        //                    var targetId = identifier.ToLower().Replace(" ", "_").Replace("-", "");

        //                    workflowActions.Add(new WorkflowAction
        //                    {
        //                        ReqId = reqId,
        //                        Identifier = identifier,
        //                        ActionName = actionName,
        //                        ScenarioId = scenarioId,
        //                        MethodName = methodName,
        //                        TargetId = targetId
        //                    });

        //                }
        //            }
        //            catch (NoSuchElementException)
        //            {
        //                // Optionally log or skip rows without expected structure
        //            }
        //        }

        //        result.Passed = true;
        //        result.Message = $"Found {workflowActions.Count} Workflow Actions.";
        //        result.WorkflowActions = workflowActions;

        //        return result;
        //    });

        //    if (testModel.Passed)
        //    {
        //        return testModel.WorkflowActions;
        //    }
        //    else
        //    {
        //        throw new Exception(testModel.Message);
        //    }
        //}

        public string GenerateScenarioId(string identifier, string action)
        {
            return $"{identifier.ToLower().Replace(" ", "_").Replace("-", "")}_{action.ToLower()}";
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


    }
}
