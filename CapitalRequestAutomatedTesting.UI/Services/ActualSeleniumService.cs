using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Reflection;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;
using vm = CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.UI.Helpers;
using OpenQA.Selenium.Chrome;
using CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IActualSeleniumService
    {
        Task<SeleniumScenarioOutcome> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<List<SeleniumScenarioStep>> GenerateSeleniumSteps(ScenarioDetailsViewModel scenarioDetail);
        Task<SeleniumScenarioOutcome> ExecuteSeleniumStepsAsync(List<SeleniumScenarioStep> steps, ScenarioDetailsViewModel scenarioDetail, IWebDriver driver);
    }

    public class ActualSeleniumService : IActualSeleniumService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly IActualRequestedInfoService _ActualRequestedInfoService;
        private IActualWorkflowStepResponderService _ActualWorkflowStepResponderService;
        private IActualWorkflowStepOptionService _ActualWorkflowStepOptionService;
        private IActualEmailNotificationService _ActualEmailNotificationService;
        private readonly IUserContextService _userContextService;
        private readonly IServiceScopeFactory _scopeFactory;


        public ActualSeleniumService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IActualRequestedInfoService ActualRequestedInfoService,
            IActualWorkflowStepResponderService ActualWorkflowStepResponderService,
            IActualWorkflowStepOptionService ActualWorkflowStepOptionService,
            IActualEmailNotificationService ActualEmailNotificationService,
            IUserContextService userContextService,
            IServiceScopeFactory scopeFactory)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _workflowControllerService = workflowControllerService;
            _ActualRequestedInfoService = ActualRequestedInfoService;
            _ActualWorkflowStepResponderService = ActualWorkflowStepResponderService;
            _ActualWorkflowStepOptionService = ActualWorkflowStepOptionService;
            _ActualEmailNotificationService = ActualEmailNotificationService;
            _userContextService = userContextService;
            _scopeFactory = scopeFactory;
        }

        public async Task<SeleniumScenarioOutcome> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var seleniumScenarioOutcome = new SeleniumScenarioOutcome();
            var scenarioId = scenarioDetail.ScenarioId;
            if (scenarioId == "SCN001")
            {
                var proposal = await _capitalRequestServices.GetProposal(scenarioDetail.ProposalId);
                proposal.ReviewerGroupId = scenarioDetail.RequestingGroupId;
                var requestingGroup = await _capitalRequestServices.GetReviewerGroup(scenarioDetail.RequestingGroupId);
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(scenarioDetail.TargetGroupId);
                var reviewer = await _capitalRequestServices.GetReviewer(scenarioDetail.ReviewerId);
                proposal.RequestedInfo.ReviewerGroupId = scenarioDetail.TargetGroupId;
                proposal.RequestedInfo.RequestingReviewerGroupId = scenarioDetail.RequestingGroupId;
                proposal.RequestedInfo.RequestedInformation = scenarioDetail.RequestedInformation;
                proposal.ReviewerId = scenarioDetail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);

                scenarioDetail.SelectedProperties["Scenario Name"] = scenarioDetail.DisplayText;
                scenarioDetail.SelectedProperties["Req Id"] = scenarioDetail.ProposalId.ToString();
                scenarioDetail.SelectedProperties["Requesting Group"] = requestingGroup.Name;
                scenarioDetail.SelectedProperties["Target Group"] = targetGroup.Name;
                scenarioDetail.SelectedProperties["Reviewer"] = reviewer.FullName;
                scenarioDetail.SelectedProperties["Requested Information"] = scenarioDetail.RequestedInformation;

                var steps = await GenerateSeleniumSteps(scenarioDetail);
                var options = GetChromeOptions();
                var driver = new ChromeDriver(options);
                driver.Manage().Window.Maximize();
                try
                {
                    seleniumScenarioOutcome = await ExecuteSeleniumStepsAsync(steps, scenarioDetail, driver);
                }
                finally
                {
                    driver.Quit(); // Always clean up
                }
                //seleniumScenarioOutcome = await ExecuteSeleniumStepsAsync(steps, scenarioDetail);
            }

            seleniumScenarioOutcome.ScenarioId = scenarioId;

            return seleniumScenarioOutcome;
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

        public async Task<List<SeleniumScenarioStep>> GenerateSeleniumSteps(ScenarioDetailsViewModel scenarioDetail)
        {
            var ActualSteps = new List<SeleniumScenarioStep>();

            var scenarioId = scenarioDetail.ScenarioId;
            var baseUrl = _workflowControllerService.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;
            var proposalId = scenarioDetail.ProposalId;
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(scenarioDetail.RequestingGroupId);
            var targetGroup = await _capitalRequestServices.GetReviewerGroup(scenarioDetail.TargetGroupId);
            var workflowPortion = $"{reviewerGroup.StepNumber} -{reviewerGroup.Name}";
            var workflowButtonId = "btnWorkflowActions";
            var workflowButtonText = "Workflow";
            var requestButtonId = "btnRequestMoreInfo";
            var requestButtonText = "Request More Information button";
            var dashboardOrder = reviewerGroup.DashboardOrder ?? 0;



            if (scenarioId == "SCN001")
            {
                var WorkflowDashboardButtonText = Constants.ACTION_TYPE_VERIFY;
                var RequestMoreInformationButton = Constants.RESPONSE_REQUEST_MORE_INFORMATION;

                ActualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = 1,
                    Description = "Validate Workflow DashBoard button click and validate Requesting Reviewer Group Verify button",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.NavigateTo($"{baseUrl}/Proposal/ViewProposal/{proposalId}"))
                        .Then(Validate.ElementById(workflowButtonId, $"{workflowButtonText} button"))
                        .Then(Execute.ClickWhenVisibleById(workflowButtonId, workflowButtonText))
                        .Then(Validate.Text(workflowPortion))
                        .Then(Validate.ButtonInRowWithText(workflowPortion, WorkflowDashboardButtonText))
                        .Build("Reached Workflow DashBoard page")

                });

                ActualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = 2,
                    Description = $"Click '{WorkflowDashboardButtonText}' in row with WorkflowPortion '{workflowPortion}' and validate no rejection message",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.ClickButtonInRow(workflowPortion, WorkflowDashboardButtonText))
                        .Then(Validate.ElementNotPresentById("responseMessage", "Rejection message container"))
                        .Then(Validate.ButtonById(requestButtonId, requestButtonText))
                        .Build("Clicked Request and confirmed page transition")

                });

                ActualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = 3,
                    Description = $"Click '{RequestMoreInformationButton}' and validate Targeted Reviewer Group avaiable in drop down selector",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.ClickWhenVisibleById(requestButtonId, requestButtonText))
                    .Then(Validate.ElementById("RequestedInfo_ReviewerGroupId", "Target Reviewer dropdown"))
                    .Then(Execute.SelectDropdown("RequestedInfo_ReviewerGroupId", targetGroup.Name, "Reviewer Group"))
                    .Build("Clicked Request More Information button and confirmed dropdown selection")


                });

                ActualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = 4,
                    Description = $"Enter requested information press submit and verify success message",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.EnterRequestedInformation(scenarioDetail.RequestedInformation))
                    .Then(Execute.ClickButtonById("btnSubmitMoreInfo", "Submit button"))
                    .Then(Validate.ElementTextById("responseMessage",Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT, "Submission success message"))
                    .Build("Entered requested information and clicked Submit button")

                });

                ActualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = 5,
                    Description = $"Navigate to Home Dashboard enter Request Id and verify group status",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.NavigateTo($"{baseUrl}"))
                    .Then(Execute.DashboardSearch(proposalId.ToString()))
                    .Then(Validate.DashboardStatus(dashboardOrder, targetGroup.Name, DateTime.Now))
                    .Build("Navigate to Home Dashboard enter Request verify group status")

                });
            }

            return ActualSteps;
        }

        public async Task<SeleniumScenarioOutcome> ExecuteSeleniumStepsAsync(List<SeleniumScenarioStep> steps, ScenarioDetailsViewModel scenarioDetail, IWebDriver driver)
        {
            var outcome = new SeleniumScenarioOutcome
            {
                ScenarioId = scenarioDetail.ScenarioId,
                Expected = new SeleniumScenarioResult
                {
                    ScenarioName = scenarioDetail.DisplayText
                }
            };

            foreach (var step in steps)
            {
                SeleniumStepResult result = null;
                try
                {
                    result = await step.Action.Invoke(driver); 
                }
                catch (Exception ex)
                {
                    result = new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Exception during step execution: {ex.Message}"
                    };
                }

                step.Result = result;

                outcome.Expected.Steps.Add(step);
            }
            outcome.Expected.Success = outcome.Expected.Passed;

            return outcome;
        }

    }
}
