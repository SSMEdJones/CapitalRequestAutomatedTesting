using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SSMWorkflow.API.DataAccess.Models;
using System.Diagnostics;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;

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
        private readonly IActualRequestedInfoService _actualRequestedInfoService;
        private readonly IActualWorkflowStepResponderService _actualWorkflowStepResponderService;
        private readonly IActualWorkflowStepOptionService _actualWorkflowStepOptionService;
        private readonly IActualEmailNotificationService _actualEmailNotificationService;
        private readonly IActualDashboardService _actualDashboardService;
        private readonly IUserContextService _userContextService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public ActualSeleniumService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IActualRequestedInfoService actualRequestedInfoService,
            IActualWorkflowStepResponderService actualWorkflowStepResponderService,
            IActualWorkflowStepOptionService actualWorkflowStepOptionService,
            IActualEmailNotificationService actualEmailNotificationService,
            IActualDashboardService actualDashboardService,
            IUserContextService userContextService,
            IServiceScopeFactory scopeFactory,
            IMapper mapper)

        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _workflowControllerService = workflowControllerService;
            _actualRequestedInfoService = actualRequestedInfoService;
            _actualWorkflowStepResponderService = actualWorkflowStepResponderService;
            _actualWorkflowStepOptionService = actualWorkflowStepOptionService;
            _actualEmailNotificationService = actualEmailNotificationService;
            _actualDashboardService = actualDashboardService;
            _userContextService = userContextService;
            _scopeFactory = scopeFactory;
            _mapper = mapper;

        }

        public async Task<SeleniumScenarioOutcome> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var seleniumScenarioOutcome = new SeleniumScenarioOutcome();
            var scenarioId = scenarioDetail.ScenarioId;
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);
            var proposal = await _capitalRequestServices.GetProposal(detail.ProposalId);
            var requestingGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
            var reviewer = await _capitalRequestServices.GetReviewer(detail.ReviewerId);

            proposal.ReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestedInfo.RequestedInformation = detail.RequestedInformation;
            proposal.ReviewerId = detail.ReviewerId;
            proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);
            
            scenarioDetail.SelectedProperties["Scenario Name"] = scenarioDetail.DisplayText;
            scenarioDetail.SelectedProperties["Req Id"] = detail.ProposalId.ToString();
            scenarioDetail.SelectedProperties["Requesting Group"] = requestingGroup.Name;
            scenarioDetail.SelectedProperties["Reviewer"] = reviewer.FullName;
            scenarioDetail.SelectedProperties["Requested Information"] = detail.RequestedInformation;


            if (scenarioId == "SCN001")
            {
                proposal.ReviewerGroupId = detail.RequestingGroupId;
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(detail.TargetGroupId);
                proposal.RequestedInfo.ReviewerGroupId = detail.TargetGroupId;

                scenarioDetail.SelectedProperties["Target Group"] = targetGroup.Name;

                //var steps = await GenerateSeleniumSteps(scenarioDetail);
                //var options = GetChromeOptions();
                //var driver = new ChromeDriver(options);
                //driver.Manage().Window.Maximize();
                //try
                //{
                //    seleniumScenarioOutcome = await ExecuteSeleniumStepsAsync(steps, scenarioDetail, driver);
                //}
                //finally
                //{
                //    driver.Quit(); // Always clean up
                //}
            }
            else if (scenarioId == "SCN002")
            {
                proposal.ReplyingGroupId = detail.ReplyingGroupId;
                proposal.ReplyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);
                scenarioDetail.SelectedProperties["Replying Group"] = proposal.ReplyingGroup.Name;

                //var steps = await GenerateSeleniumSteps(scenarioDetail);
                //var options = GetChromeOptions();
                //var driver = new ChromeDriver(options);
                //driver.Manage().Window.Maximize();
                //try
                //{
                //    seleniumScenarioOutcome = await ExecuteSeleniumStepsAsync(steps, scenarioDetail, driver);
                //}
                //finally
                //{
                //    driver.Quit(); // Always clean up
                //}
            }

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
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);

            var proposalId = scenarioDetail.ProposalId;
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            proposal.RequestedInfoId = detail.RequestedInfoId;

            var actualSteps = new List<SeleniumScenarioStep>();

            var scenarioId = scenarioDetail.ScenarioId;
            var baseUrl = _workflowControllerService.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;
            
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
            var workflowPortion = $"{reviewerGroup.StepNumber} -{reviewerGroup.Name}";
            var workflowButtonId = "btnWorkflowActions";
            var workflowButtonText = "Workflow";
            var dashboardOrder = reviewerGroup.DashboardOrder ?? 0;
            var reviewer = await _capitalRequestServices.GetReviewer(detail.ReviewerId);

            var filter = new DashboardSearchFilter { CapitalFundingYear = DateTime.Now.Year };

            scenarioDetail.RequestCount = (await _actualDashboardService.GetDashboardDataByUserId(filter, reviewer)).Count();

            Debug.WriteLine($"ReviewerUserId: {reviewer.UserId ?? "null"}");

            var homeDashboardUrl = BuildTestModeUrl($"", reviewer.UserId);
            var viewProposalUrl = BuildTestModeUrl($"/Proposal/ViewProposal/{proposalId}", reviewer.UserId);
            bool reviewerHasNoRequests = scenarioDetail.RequestCount == 0;
            //TODO add to config
            var maxRetries = 3;

            var stepNumber = 0;
            var verifyButtonText = Constants.ACTION_TYPE_VERIFY;

            if (scenarioId == "SCN001")
            {
                var requestMoreInformationButton = Constants.RESPONSE_REQUEST_MORE_INFORMATION;
                var requestButtonId = "btnRequestMoreInfo";
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(detail.TargetGroupId);


                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = "Validate Workflow DashBoard button click and validate Requesting Reviewer Group Verify button",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.NavigateTo(viewProposalUrl))
                        .Then(Validate.ElementById(workflowButtonId, $"{workflowButtonText} button"))
                        .Then(Execute.RobustClickById(workflowButtonId, workflowButtonText, maxRetries))
                        .Then(Validate.Text(workflowPortion))
                        .Then(Validate.ButtonInRowWithText(workflowPortion, verifyButtonText))
                        .Build("Reached Workflow DashBoard page")

                });

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = $"Click '{verifyButtonText}' in row with WorkflowPortion '{workflowPortion}' and validate no rejection message",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.ClickButtonInRow(workflowPortion, verifyButtonText))
                        .Then(Validate.ElementNotPresentById("responseMessage", "Rejection message container"))
                        .Then(Validate.ButtonById(requestButtonId, requestMoreInformationButton))
                        .Build("Clicked Request and confirmed page transition")
                });

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = $"Click '{requestMoreInformationButton}' and validate Targeted Reviewer Group avaiable in drop down selector",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.RobustClickById(requestButtonId, requestMoreInformationButton, maxRetries))
                    .Then(Validate.ElementById("RequestedInfo_ReviewerGroupId", "Target Reviewer dropdown"))
                    .Then(Execute.SelectDropdown("RequestedInfo_ReviewerGroupId", targetGroup.Name, "Reviewer Group"))
                    .Build("Clicked Request More Information button and confirmed dropdown selection")


                });

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = $"Enter requested information press submit and verify success message",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.EnterRequestedInformation(scenarioDetail.RequestedInformation))
                    .Then(Execute.ClickButtonById("btnSubmitMoreInfo", "Submit button"))
                    .Then(Validate.ElementTextById("responseMessage", Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT, "Submission success message"))
                    .Build("Entered requested information and clicked Submit button")

                });

                var conditionalDashboardSteps = new SeleniumDsl()
                    .BeginWith(Execute.NavigateTo($"{homeDashboardUrl}"))
                    .Then(Conditional.If(
                        reviewerHasNoRequests,
                        Validate.NoRequestsMessage(), // When no requests
                        new SeleniumDsl()
                            .BeginWith(Execute.DashboardSearch(proposalId.ToString()))
                            .Then(Validate.DashboardStatus(dashboardOrder, targetGroup.Name, DateTime.Now))
                            .Build("Dashboard Search + Status Validation")
                    ))
                    .Build("Navigate to Home Dashboard and validate group status");

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = $"Navigate to Home Dashboard enter Request Id and verify group status",
                    Action = conditionalDashboardSteps,
                    Retryable = true
                });

                
            }
            else if (scenarioId == "SCN002")
            {
                //var WorkflowDashboardButtonText = Constants.ACTION_TYPE_VERIFY;
                var replyButton = Constants.RESPONSE_RETURN_MORE_INFORMATION;
                var buttonText = Constants.ACTION_TYPE_REPLY;
                var replyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);
                var requestedInfoId = proposal.RequestedInfoId.ToString();
                var description = $"Replying to Request Id {requestedInfoId}";
                workflowPortion = $"{replyingGroup.StepNumber} -{replyingGroup.Name}";
                maxRetries = 3;


                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = "Validate Workflow DashBoard button click and validate Replying Reviewer Group Reply button",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.NavigateTo(viewProposalUrl))
                    .Then(Validate.ElementById(workflowButtonId, $"{workflowButtonText} button"))
                    .Then(Execute.RobustClickById(workflowButtonId, workflowButtonText, maxRetries))
                    .Then(Validate.Text(workflowPortion))
                    .Then(Validate.ButtonInRowWithText(workflowPortion, buttonText))
                    .Build("Reached Workflow DashBoard page")

                });

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = $"Click '{buttonText}' in row with WorkflowPortion '{workflowPortion}' and validate no rejection message",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.RobustClickReplyInRow(workflowPortion, requestedInfoId, description, buttonText, maxRetries))
                    .Then(Validate.ElementNotPresentById("responseMessage", "Rejection message container"))
                    .Build("Clicked Request and confirmed page transition"),
                    Retryable = true

                });

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = $"Enter returned information press submit and verify success message",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.EnterReturnedInformation(scenarioDetail.ReturnedInformation))
                    .Then(Execute.ClickButtonById("btnSendAddedInfo", "Submit button"))
                    .Then(Validate.ElementTextById("responseMessage", Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT, "Submission success message"))
                    .Build("Entered requested information and clicked Submit button")

                });


                var conditionalDashboardSteps = new SeleniumDsl()
                    .BeginWith(Execute.NavigateTo($"{homeDashboardUrl}"))
                    .Then(Conditional.If(
                        reviewerHasNoRequests,
                        Validate.NoRequestsMessage(), // When no requests
                        new SeleniumDsl()
                            .BeginWith(Execute.DashboardSearch(proposalId.ToString()))
                            .Then(Validate.DashboardStatus(dashboardOrder, replyingGroup.Name, DateTime.Now))
                            .Build("Dashboard Search + Status Validation")
                    ))
                    .Build("Navigate to Home Dashboard and validate group status");

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = $"Navigate to Home Dashboard enter Request Id and verify group status",
                    Action = conditionalDashboardSteps,
                    Retryable = true
                });

                //actualSteps.Add(new SeleniumScenarioStep
                //{
                //    StepNumber = ++stepNumber,
                //    Description = $"Enter returned information press submit and verify success message",
                //    Action = new SeleniumDsl()
                //        .BeginWith(Execute.RobustClickReplyInRow(workflowPortion, requestedInfoId, description, buttonText, maxRetries))
                //        .Then(Execute.ClickButtonById("btnSendAddedInfo", "Submit button"))
                //        .Then(Validate.ElementTextById("responseMessage", Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT, "Submission success message"))
                //        .Build("Entered requested information and clicked Submit button"),
                //    Retryable = true

                //});
                //actualSteps.Add(new SeleniumScenarioStep
                //{
                //    StepNumber = ++stepNumber,
                //    Description = $"Click '{buttonText}' in row with WorkflowPortion '{workflowPortion}' and validate no rejection message",
                //    Action = new SeleniumDsl()
                //        .BeginWith(Execute.ClickButtonInRow(workflowPortion, buttonText))
                //        .Then(Validate.ElementNotPresentById("responseMessage", "Rejection message container"))
                //        .Build("Clicked Request and confirmed page transition")

                //});

                //.Then(Execute.RobustClickById(workflowButtonId, workflowButtonText, maxRetries))
                //BeginWith(Execute.RobustClickReplyInRow(workflowPortion, requestedInfoId, description, buttonText, maxRetries);
                //actualSteps.Add(new SeleniumScenarioStep
                //{
                //    StepNumber = ++stepNumber,
                //    Description = $"Enter returned information press submit and verify success message",
                //    Action = new SeleniumDsl()
                //        .BeginWith(Execute.RobustClickReplyInRow(workflowPortion, requestedInfoId, description, buttonText, maxRetries))
                //        .Then(Execute.ClickButtonById("btnSendAddedInfo", "Submit button"))
                //        .Then(Validate.ElementTextById("responseMessage", Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT, "Submission success message"))
                //        .Build("Entered requested information and clicked Submit button"),
                //    Retryable = true

                //});



            }

            return actualSteps;
        }

        public async Task<SeleniumScenarioOutcome> ExecuteSeleniumStepsAsync(List<SeleniumScenarioStep> steps, ScenarioDetailsViewModel scenarioDetail, IWebDriver driver)
        {
            var maxStep = scenarioDetail.PredictiveCompletionStep;

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
                if (maxStep > 0 && step.StepNumber > maxStep)
                {
                    step.Result = new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Step skipped due to predictive cutoff at step {maxStep}."
                    };

                    outcome.Expected.Steps.Add(step);
                    continue;
                }


                SeleniumStepResult result = null;

                try
                {
                    if (step.Retryable)
                    {
                        result = await SeleniumRetryHelper.RetryAsync(
                            () => step.Action.Invoke(driver),
                            maxRetries: 3,
                            delayBetweenRetries: TimeSpan.FromSeconds(2));
                    }
                    else
                    {
                        result = await step.Action.Invoke(driver);
                    }
                }
                catch (Exception ex)
                {
                    result = new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Exception during step execution: {ex.Message}"
                    };

                    Debug.WriteLine($"Error in step {step.StepNumber}: {ex.Message}");

                    try
                    {
                        if (driver is ITakesScreenshot screenshotDriver)
                        {
                            var screenshot = screenshotDriver.GetScreenshot();
                            var fileName = $"failure_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                            var filePath = Path.Combine("Screenshots", fileName);

                            Directory.CreateDirectory("Screenshots");
                            screenshot.SaveAsFile(filePath);


                        }
                    }
                    catch (Exception screenshotEx)
                    {
                        Debug.WriteLine($"Failed to capture screenshot: {screenshotEx.Message}");
                    }
                }

                step.Result = result;
                outcome.Expected.Steps.Add(step);
            }

            return outcome;
        }

        public string BuildTestModeUrl(string route, string testUserId)
        {
            var baseUrl = _workflowControllerService
                .GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL")
                .LookupValue;

            var suffix = $"testmode=true&testuser={Uri.EscapeDataString(testUserId)}";
            var separator = route.Contains("?") ? "&" : "?";

            return $"{baseUrl.TrimEnd('/')}/{route.TrimStart('/')}{separator}{suffix}";
        }


    }
}
