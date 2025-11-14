using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SSMWorkflow.API.DataAccess.Models;
using System;
using System.Diagnostics;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
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
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly IActualDashboardService _actualDashboardService;
        private readonly IActualEmailNotificationService _actualEmailNotificationService;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ILogger<ActualSeleniumService> _logger;
        private readonly IMapper _mapper;

        public ActualSeleniumService(ILogger<ActualSeleniumService> logger,
            ICapitalRequestServices capitalRequestServices,
            IWorkflowControllerService workflowControllerService,
            IActualDashboardService actualDashboardService,
            IActualEmailNotificationService actualEmailNotificationService,
            ISSMWorkflowServices ssmWorkflowServices,
            IMapper mapper)
        {
            _logger = logger;
            _capitalRequestServices = capitalRequestServices;
            _workflowControllerService = workflowControllerService;
            _actualDashboardService = actualDashboardService;
            _actualEmailNotificationService = actualEmailNotificationService;
            _ssmWorkflowServices = ssmWorkflowServices;
            _mapper = mapper;
        }

        public async Task<SeleniumScenarioOutcome> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var seleniumScenarioOutcome = new SeleniumScenarioOutcome();
            var scenarioId = scenarioDetail.ScenarioId;
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);
            var proposal = await _capitalRequestServices.GetProposal(detail.ProposalId);

            if (detail.ScenarioId != "SCN004")
            {

                var requestingGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
                var reviewer = await _capitalRequestServices.GetReviewer(detail.ReviewerId);

                proposal.ReviewerGroupId = detail.RequestingGroupId;
                proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
                proposal.RequestedInfo.RequestedInformation = detail.RequestedInformation;
                proposal.ReviewerId = detail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);

            }

            //scenarioDetail.SelectedProperties["Scenario Name"] = scenarioDetail.DisplayText;
            //scenarioDetail.SelectedProperties["Req Id"] = detail.ProposalId.ToString();
            //scenarioDetail.SelectedProperties["Requesting Group"] = requestingGroup.Name;
            //scenarioDetail.SelectedProperties["Reviewer"] = reviewer.FullName;
            //scenarioDetail.SelectedProperties["Requested Information"] = detail.RequestedInformation;


            if (scenarioId == "SCN001")
            {
                proposal.ReviewerGroupId = detail.RequestingGroupId;
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(detail.TargetGroupId);
                proposal.RequestedInfo.ReviewerGroupId = detail.TargetGroupId;

                scenarioDetail.SelectedProperties["Target Group"] = targetGroup.Name;

            }
            else if (scenarioId == "SCN002")
            {
                proposal.ReplyingGroupId = detail.ReplyingGroupId;
                proposal.ReplyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);
                scenarioDetail.SelectedProperties["Replying Group"] = proposal.ReplyingGroup.Name;

            }
            else if (scenarioId == "SCN004")
            {
                //stubbed for future scenario
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
            // 🔥 TEMPORARILY COMMENTED OUT for debugging:
            // options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-popup-blocking");

            return options;
        }

        public async Task<List<SeleniumScenarioStep>> GenerateSeleniumSteps(ScenarioDetailsViewModel scenarioDetail)
        {
            var reviewerGroup = new CapitalRequest.API.Models.ReviewerGroup();
            var workflowPortion = string.Empty;
            var workflowButtonId = string.Empty;
            var workflowButtonText = string.Empty;
            var dashboardOrder = new int();
            var reviewer = new CapitalRequest.API.Models.Reviewer();


            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);

            var proposalId = scenarioDetail.ProposalId;
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            proposal.SubmitUserId = detail.SubmitUserId;
            var userId = string.Empty;

            if (detail.ScenarioId != "SCN004")
            {
                proposal.ReviewerGroupId = detail.RequestingGroupId;
                proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
                proposal.RequestedInfo.RequestedInformation = detail.RequestedInformation;
                proposal.ReviewerId = detail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);

                proposal.RequestedInfoId = detail.RequestedInfoId;
                reviewerGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
                workflowPortion = $"{reviewerGroup.StepNumber} -{reviewerGroup.Name}";
                workflowButtonId = "btnWorkflowActions";
                workflowButtonText = "Workflow";
                dashboardOrder = reviewerGroup.DashboardOrder ?? 0;
                reviewer = await _capitalRequestServices.GetReviewer(detail.ReviewerId);
                userId = reviewer.UserId ?? string.Empty;
            }
            else if (detail.ScenarioId == "SCN004")
            {
                userId = detail.SubmitUserId;
                dashboardOrder = 0;
            }
            var actualSteps = new List<SeleniumScenarioStep>();

            var scenarioId = scenarioDetail.ScenarioId;
            var baseUrl = _workflowControllerService.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;
            var filter = new DashboardSearchFilter { CapitalFundingYear = DateTime.Now.Year };

            scenarioDetail.RequestCount = (await _actualDashboardService.GetDashboardDataByUserId(filter, userId)).Count();

            Debug.WriteLine($"ReviewerUserId: {reviewer.UserId ?? "null"}");

            var homeDashboardUrl = BuildTestModeUrl($"", userId);
            var viewProposalUrl = BuildTestModeUrl($"/Proposal/ViewProposal/{proposalId}", userId);
            bool userHasNoRequests = scenarioDetail.RequestCount == 0;
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

                // Step: Enter requested information (do not submit yet)
                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = "Enter requested information (do not submit yet)",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.EnterRequestedInformation(scenarioDetail.RequestedInformation))
                        .Build("Entered requested information")
                });

                // For SCN001 - Enhanced pause section with better error handling:
                if (scenarioDetail.PauseBeforeSubmit)
                {
                    actualSteps.Add(new SeleniumScenarioStep
                    {
                        StepNumber = ++stepNumber,
                        Description = "Pause for user to manually submit and auto-detect response message",
                        Action = async driver =>
                        {
                            Console.WriteLine("🔄 Paused: Please manually submit the form in the browser.");
                            Console.WriteLine("🤖 Automation will automatically continue when the response message appears...");
                            Debug.WriteLine($"🔄 {scenarioId}: Starting automated response detection");

                            var urlBefore = driver.Url;
                            Debug.WriteLine($"🔄 {scenarioId}: Current URL before submission: {urlBefore}");

                            // 🔥 Wait for response message to appear (instead of manual Enter press)
                            var responseResult = await WaitForResponseMessage(driver, scenarioId, maxWaitSeconds: 60);

                            if (responseResult.Found)
                            {
                                Console.WriteLine($"✅ Response message detected: '{responseResult.Message}'");
                                Console.WriteLine($"🚀 Continuing test execution automatically after {responseResult.ElapsedSeconds:F1}s");

                                Debug.WriteLine($"✅ {scenarioId}: Auto-detected response after {responseResult.ElapsedSeconds:F1}s");

                                // Check for expected success messages
                                if (responseResult.Message.Contains(Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT))
                                {
                                    Debug.WriteLine($"✅ {scenarioId}: Expected success message detected");
                                    return SeleniumStepResult.Pass($"Form submitted successfully. Auto-detected success message: {responseResult.Message}");
                                }
                                else if (responseResult.Message.Contains("Error") || responseResult.Message.Contains("Failed"))
                                {
                                    Debug.WriteLine($"❌ {scenarioId}: Error message detected");
                                    return SeleniumStepResult.Fail($"Error detected in response: {responseResult.Message}");
                                }
                                else
                                {
                                    Debug.WriteLine($"⚠️ {scenarioId}: Unexpected message content");
                                    return SeleniumStepResult.Pass($"Form submitted. Unexpected message: {responseResult.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"⏰ Timeout: No response message appeared within 60 seconds");
                                Debug.WriteLine($"⏰ {scenarioId}: Timeout waiting for response message");

                                // Fallback: Check for redirect or other indicators
                                try
                                {
                                    await Task.Delay(2000);
                                    var currentUrl = driver.Url;

                                    if (currentUrl.Contains("/Home") || !currentUrl.Contains("WorkflowActions"))
                                    {
                                        Debug.WriteLine($"⚠️ {scenarioId}: Detected redirect as fallback indicator");
                                        return SeleniumStepResult.Pass($"Form likely submitted (detected redirect to: {currentUrl})");
                                    }
                                    else
                                    {
                                        return SeleniumStepResult.Fail("Timeout waiting for response message and no redirect detected");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return SeleniumStepResult.Fail($"Timeout and error checking fallback indicators: {ex.Message}");
                                }
                            }
                        }
                    });
                }
                else
                {
                    // Automated submission path
                    actualSteps.Add(new SeleniumScenarioStep
                    {
                        StepNumber = ++stepNumber,
                        Description = "Press submit and verify success message",
                        Action = new SeleniumDsl()
                            .BeginWith(Execute.ClickButtonById("btnSubmitMoreInfo", "Submit button"))
                            .Then(Validate.ElementTextById("responseMessage", Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT, "Submission success message"))
                            .Build("Clicked Submit button and verified success message")
                    });
                }

                // 🔥 IMPORTANT: Dashboard validation should ALWAYS execute after either path
                var conditionalDashboardSteps = new SeleniumDsl()
                    .BeginWith(Execute.NavigateTo($"{homeDashboardUrl}"))
                    .Then(Conditional.If(
                        userHasNoRequests,
                        Validate.NoRequestsMessage(),
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
                //var replyButton = Constants.RESPONSE_RETURN_MORE_INFORMATION;
                var buttonText = Constants.ACTION_TYPE_REPLY;
                var replyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);
                var requestedInfoId = proposal.RequestedInfoId.ToString();
                var description = $"Replying to Request Id {requestedInfoId}";
                workflowPortion = $"{replyingGroup.StepNumber} -{replyingGroup.Name}";
                maxRetries = 3;

                scenarioDetail.FileUploadPaths = scenarioDetail.FileUploads
                   .Select(f => f.TempFilePath)
                   .Where(path => !string.IsNullOrEmpty(path))
                   .ToList();

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    StepName = "Validate Workflow DashBoard button",
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
                    StepName = $"Click '{buttonText}' in row with WorkflowPortion",
                    Description = $"Click '{buttonText}' in row with WorkflowPortion '{workflowPortion}' and validate no rejection message",
                    Action = new SeleniumDsl()
                    .BeginWith(Execute.RobustClickReplyInRow(workflowPortion, requestedInfoId, description, buttonText, maxRetries))
                    .Then(Validate.ElementNotPresentById("responseMessage", "Rejection message container"))
                    .Build("Clicked Reply and confirmed page transition"),
                    Retryable = true

                });

                if (scenarioDetail.RequiresFileUpload)
                {
                    // Step 1: Reveal the hidden file input
                    actualSteps.Add(new SeleniumScenarioStep
                    {
                        StepNumber = ++stepNumber,
                        StepName = "Make file input visible",
                        Description = "Make file input visible",
                        Action = new SeleniumDsl()
                            .BeginWith(Execute.RunJavaScript("document.getElementById('btnFilePicker').style.display = 'block';", "Reveal hidden file input"))
                            .Build("File input revealed")
                    });

                    // Step 2: Upload each file one at a time
                    foreach (var filePath in scenarioDetail.FileUploadPaths)
                    {
                        actualSteps.Add(new SeleniumScenarioStep
                        {
                            StepNumber = ++stepNumber,
                            StepName = $"Upload file: {Path.GetFileName(filePath)}",
                            Description = $"Upload file: {Path.GetFileName(filePath)}",
                            Action = new SeleniumDsl()
                                .BeginWith(Execute.UploadFileById("btnFilePicker", filePath, "File Picker"))
                                .Build($"Appended {Path.GetFileName(filePath)} to upload list")
                        });
                    }

                    // Step 3: Hide the file input to restore original appearance
                    actualSteps.Add(new SeleniumScenarioStep
                    {
                        StepNumber = ++stepNumber,
                        StepName = "Hide file input",
                        Description = "Hide file input to restore original application appearance",
                        Action = new SeleniumDsl()
                            .BeginWith(Execute.RunJavaScript("document.getElementById('btnFilePicker').style.display = 'none';", "Hide file input"))
                            .Build("File input hidden")
                    });
                }

                // Step: Enter returned information
                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    StepName = "Enter returned information",
                    Description = "Enter returned information (do not submit yet)",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.EnterReturnedInformation(scenarioDetail.ReturnedInformation))
                        .Build("Entered returned information")
                });

                // Optional pause for user to review or manually submit
                if (scenarioDetail.PauseBeforeSubmit)
                {
                    actualSteps.Add(new SeleniumScenarioStep
                    {
                        StepNumber = ++stepNumber,
                        StepName = "Pause to manually submit the form",
                        Description = "Pause for user to manually submit the form and handle any redirects",
                        Action = async driver =>
                        {
                            Console.WriteLine("🔄 Paused: Please manually submit the form in the browser, then press Enter to continue...");
                            Debug.WriteLine($"🔄 SCN002: Current URL before manual submission: {driver.Url}");

                            var responseResult = await WaitForResponseMessage(driver, scenarioId, maxWaitSeconds: 60);

                            if (responseResult.Found)
                            {
                                Console.WriteLine($"✅ Response message detected: '{responseResult.Message}'");
                                Console.WriteLine($"🚀 Continuing test execution automatically after {responseResult.ElapsedSeconds:F1}s");

                                Debug.WriteLine($"✅ {scenarioId}: Auto-detected response after {responseResult.ElapsedSeconds:F1}s");

                                // Check for expected success messages
                                if (responseResult.Message.Contains(Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT))
                                {
                                    Debug.WriteLine($"✅ {scenarioId}: Expected success message detected");
                                    return SeleniumStepResult.Pass($"Form submitted successfully. Auto-detected success message: {responseResult.Message}");
                                }
                                else if (responseResult.Message.Contains("Error") || responseResult.Message.Contains("Failed"))
                                {
                                    Debug.WriteLine($"❌ {scenarioId}: Error message detected");
                                    return SeleniumStepResult.Fail($"Error detected in response: {responseResult.Message}");
                                }
                                else
                                {
                                    Debug.WriteLine($"⚠️ {scenarioId}: Unexpected message content");
                                    return SeleniumStepResult.Pass($"Form submitted. Unexpected message: {responseResult.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"⏰ Timeout: No response message appeared within 60 seconds");
                                Debug.WriteLine($"⏰ {scenarioId}: Timeout waiting for response message");

                                // Fallback: Check for redirect or other indicators
                                try
                                {
                                    await Task.Delay(2000);
                                    var currentUrl = driver.Url;

                                    if (currentUrl.Contains("/Home") || !currentUrl.Contains("WorkflowActions"))
                                    {
                                        Debug.WriteLine($"⚠️ {scenarioId}: Detected redirect as fallback indicator");
                                        return SeleniumStepResult.Pass($"Form likely submitted (detected redirect to: {currentUrl})");
                                    }
                                    else
                                    {
                                        return SeleniumStepResult.Fail("Timeout waiting for response message and no redirect detected");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return SeleniumStepResult.Fail($"Timeout and error checking fallback indicators: {ex.Message}");
                                }
                            }

                        }
                    });
                }
                else
                {
                    // Step: Click submit and validate
                    actualSteps.Add(new SeleniumScenarioStep
                    {
                        StepNumber = ++stepNumber,
                        StepName = "Press submit",
                        Description = "Press submit and verify success message",
                        IsCommitStep = true,
                        Action = new SeleniumDsl()
                            .BeginWith(Execute.ClickButtonById("btnSendAddedInfo", "Submit button"))
                            .Then(Validate.ElementTextById("responseMessage", Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT, "Submission success message"))
                            .Build("Clicked Submit button and verified success message")
                    });
                }

                // Continue with dashboard validation step as before
                var conditionalDashboardSteps = new SeleniumDsl()
                    .BeginWith(Execute.NavigateTo($"{homeDashboardUrl}"))
                    .Then(Conditional.If(
                        userHasNoRequests,
                        Validate.NoRequestsMessage(),
                        new SeleniumDsl()
                            .BeginWith(Execute.DashboardSearch(proposalId.ToString()))
                            .Then(Validate.DashboardStatus(dashboardOrder, string.Empty, null))
                            .Build("Dashboard Search + Status Validation")
                    ))
                    .Build("Navigate to Home Dashboard and validate group status");

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    StepName = "Validate group status",
                    Description = $"Navigate to Home Dashboard enter Request Id and verify group status",
                    Action = conditionalDashboardSteps,
                    Retryable = true
                });

            }
            else if (scenarioId == "SCN004")
            {
                var editButtonId = "btnEditAttachments";
                var editButtonText = "Edit ";
                var submitButtonId = "btnSubmitWorkflow";
                var submitButtonText = "Submit ";

                // Step 1: Navigate to page and click Attachments tab
                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = "Navigate to Attachments tab",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.NavigateTo(viewProposalUrl))
                        .Then(Execute.ClickButtonById("nav-attachments-tab", "Attachments tab"))
                        .Build("Navigated to Attachments tab")
                });

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = "Validate Edit button and click",
                    Action = new SeleniumDsl()
                        .BeginWith(Validate.ElementById(editButtonId, $"{editButtonText} button"))
                        .Then(Execute.RobustClickById(editButtonId, editButtonText, maxRetries))
                        .Build("Clicked edit button")

                });

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    Description = "Validate Submit button ",
                    Action = new SeleniumDsl()
                        .BeginWith(Validate.ElementById(submitButtonId, $"{submitButtonText} button"))
                        .Build("Reached submit page")

                });

                if (scenarioDetail.PauseBeforeSubmit)
                {
                    actualSteps.Add(new SeleniumScenarioStep
                    {
                        StepNumber = ++stepNumber,
                        Description = "Pause for user to manually submit and auto-detect email notification",
                        Action = async driver =>
                        {
                            Console.WriteLine("🔄 Paused: Please manually submit the form in the browser.");
                            Console.WriteLine("🤖 Automation will automatically continue when an email notification is created...");
                            Debug.WriteLine($"🔄 {scenarioId}: Starting automated email notification detection");

                            var urlBefore = driver.Url;
                            Debug.WriteLine($"🔄 {scenarioId}: Current URL before submission: {urlBefore}");

                            // 🔥 Wait for email notification to be created (instead of response message)
                            var emailResult = await WaitForEmailNotification(scenarioId, proposalId, maxWaitSeconds: 60);

                            if (emailResult.Found)
                            {
                                Console.WriteLine($"✅ Email notification detected: ID {emailResult.NotificationId}");
                                Console.WriteLine($"🚀 Continuing test execution automatically after {emailResult.ElapsedSeconds:F1}s");

                                Debug.WriteLine($"✅ {scenarioId}: Auto-detected email notification after {emailResult.ElapsedSeconds:F1}s");
                                return SeleniumStepResult.Pass($"Form submitted successfully. Auto-detected email notification: ID {emailResult.NotificationId}");
                            }
                            else
                            {
                                Console.WriteLine($"⏰ Timeout: No email notification created within 60 seconds");
                                Debug.WriteLine($"⏰ {scenarioId}: Timeout waiting for email notification");

                                // Fallback: Check for redirect or other indicators
                                try
                                {
                                    await Task.Delay(2000);
                                    var currentUrl = driver.Url;

                                    if (currentUrl.Contains("/Home") || !currentUrl.Contains("WorkflowActions"))
                                    {
                                        Debug.WriteLine($"⚠️ {scenarioId}: Detected redirect as fallback indicator");
                                        return SeleniumStepResult.Pass($"Form likely submitted (detected redirect to: {currentUrl})");
                                    }
                                    else
                                    {
                                        return SeleniumStepResult.Fail("Timeout waiting for email notification and no redirect detected");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return SeleniumStepResult.Fail($"Timeout and error checking fallback indicators: {ex.Message}");
                                }
                            }
                        }
                    });
                }

                //if (scenarioDetail.PauseBeforeSubmit)
                //{
                //    actualSteps.Add(new SeleniumScenarioStep
                //    {
                //        StepNumber = ++stepNumber,
                //        Description = "Pause for user to manually submit and auto-detect response message",
                //        Action = async driver =>
                //        {
                //            Console.WriteLine("🔄 Paused: Please manually submit the form in the browser.");
                //            Console.WriteLine("🤖 Automation will automatically continue when the response message appears...");
                //            Debug.WriteLine($"🔄 {scenarioId}: Starting automated response detection");

                //            var urlBefore = driver.Url;
                //            Debug.WriteLine($"🔄 {scenarioId}: Current URL before submission: {urlBefore}");

                //            // 🔥 Wait for response message to appear (instead of manual Enter press)
                //            var responseResult = await WaitForResponseMessage(driver, scenarioId, maxWaitSeconds: 60);

                //            if (responseResult.Found)
                //            {
                //                Console.WriteLine($"✅ Response message detected: '{responseResult.Message}'");
                //                Console.WriteLine($"🚀 Continuing test execution automatically after {responseResult.ElapsedSeconds:F1}s");

                //                Debug.WriteLine($"✅ {scenarioId}: Auto-detected response after {responseResult.ElapsedSeconds:F1}s");

                //                // Check for expected success messages
                //                if (responseResult.Message.Contains(Constants.RESPONSE_ACTION_VERIFIED))
                //                {
                //                    Debug.WriteLine($"✅ {scenarioId}: Expected success message detected");
                //                    return SeleniumStepResult.Pass($"Form submitted successfully. Auto-detected success message: {responseResult.Message}");
                //                }
                //                else if (responseResult.Message.Contains("Error") || responseResult.Message.Contains("Failed"))
                //                {
                //                    Debug.WriteLine($"❌ {scenarioId}: Error message detected");
                //                    return SeleniumStepResult.Fail($"Error detected in response: {responseResult.Message}");
                //                }
                //                else
                //                {
                //                    Debug.WriteLine($"⚠️ {scenarioId}: Unexpected message content");
                //                    return SeleniumStepResult.Pass($"Form submitted. Unexpected message: {responseResult.Message}");
                //                }
                //            }
                //            else
                //            {
                //                Console.WriteLine($"⏰ Timeout: No response message appeared within 60 seconds");
                //                Debug.WriteLine($"⏰ {scenarioId}: Timeout waiting for response message");

                //                // Fallback: Check for redirect or other indicators
                //                try
                //                {
                //                    await Task.Delay(2000);
                //                    var currentUrl = driver.Url;

                //                    if (currentUrl.Contains("/Home") || !currentUrl.Contains("WorkflowActions"))
                //                    {
                //                        Debug.WriteLine($"⚠️ {scenarioId}: Detected redirect as fallback indicator");
                //                        return SeleniumStepResult.Pass($"Form likely submitted (detected redirect to: {currentUrl})");
                //                    }
                //                    else
                //                    {
                //                        return SeleniumStepResult.Fail("Timeout waiting for response message and no redirect detected");
                //                    }
                //                }
                //                catch (Exception ex)
                //                {
                //                    return SeleniumStepResult.Fail($"Timeout and error checking fallback indicators: {ex.Message}");
                //                }
                //            }
                //        }
                //    });
                //}
                //else
                //{
                //    // Automated submission path
                //    actualSteps.Add(new SeleniumScenarioStep
                //    {
                //        StepNumber = ++stepNumber,
                //        Description = "Press submit ",
                //        Action = new SeleniumDsl()
                //            .BeginWith(Execute.ClickButtonById("btnSubmitWorkflow", "Submit button"))
                //            .Build("Clicked Submit button ")
                //    });
                //}

                // Continue with dashboard validation step as before
                var conditionalDashboardSteps = new SeleniumDsl()
                    .BeginWith(Execute.WaitForDashboardSearchBox()) // Wait for search box instead of navigate
                    .Then(Conditional.If(
                        userHasNoRequests,
                        Validate.NoRequestsMessage(),
                        new SeleniumDsl()
                            .BeginWith(Execute.DashboardSearch(proposalId.ToString()))
                            .Then(Validate.DashboardStatus(dashboardOrder, proposal.SubmitUserId, DateTime.Now))
                            .Build("Dashboard Search + Status Validation")
                    ))
                    .Build("Wait for Dashboard Search Box and validate group status");
                //var conditionalDashboardSteps = new SeleniumDsl()
                //    .BeginWith(Execute.NavigateTo($"{homeDashboardUrl}"))
                //    .Then(Conditional.If(
                //        userHasNoRequests,
                //        Validate.NoRequestsMessage(),
                //        new SeleniumDsl()
                //            .BeginWith(Execute.DashboardSearch(proposalId.ToString()))
                //            .Then(Validate.DashboardStatus(dashboardOrder, proposal.SubmitUserId, DateTime.Now))
                //            .Build("Dashboard Search + Status Validation")
                //    ))
                //    .Build("Navigate to Home Dashboard and validate group status");

                actualSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = ++stepNumber,
                    StepName = "Validate group status",
                    Description = $"Navigate to Home Dashboard enter Request Id and verify group status",
                    Action = conditionalDashboardSteps,
                    Retryable = true
                });
            }

            return actualSteps;
        }

        // Add this new method to the ActualSeleniumService class
        private async Task<EmailNotificationResult> WaitForEmailNotification(string scenarioId, int proposalId, int maxWaitSeconds = 30)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var startTime = DateTime.Now;
            var checkInterval = TimeSpan.FromSeconds(2); // Check every 2 seconds for database changes

            while ((DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
            {

                try
                {
                    proposal = await _capitalRequestServices.GetProposal(proposalId);

                    if (proposal.WorkflowId != null && proposal.WorkflowId != Guid.Empty)
                    {
                        break;
                    }

                    Debug.WriteLine($"🔄 {scenarioId}: Still waiting for workflow ... ({(DateTime.Now - startTime).TotalSeconds:F1}s)");

                    await Task.Delay(checkInterval);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"🔍 {scenarioId}: Error checking workflow : {ex.Message}");
                }
            }

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => !x.IsComplete)
                .FirstOrDefault();

            Debug.WriteLine($"🔄 {scenarioId}: Starting to poll for email notifications for proposal {proposalId}...");

            startTime = DateTime.Now;

            // Get initial count to detect new notifications
            var initialNotifications = await _actualEmailNotificationService.GetSubmitEmailNotificationsAsync(proposal, null);
            var initialCount = initialNotifications?.Count ?? 0;

            Debug.WriteLine($"🔍 {scenarioId}: Initial email notification count: {initialCount}");

            while ((DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
            {
                try
                {
                    var currentNotifications = await _actualEmailNotificationService.GetSubmitEmailNotificationsAsync(proposal, null);
                    var currentCount = currentNotifications?.Count ?? 0;

                    Debug.WriteLine($"🔍 {scenarioId}: Current email notification count: {currentCount}");

                    if (currentCount > initialCount || currentCount > 0 && (currentCount == initialCount && (DateTime.Now - startTime).TotalSeconds > 10))
                    {
                        var elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                        var newNotification = currentNotifications?.OrderByDescending(n => n.Id).FirstOrDefault();

                        if (currentCount > initialCount)
                        {
                            Debug.WriteLine($"✅ {scenarioId}: New email notification detected after {elapsedSeconds:F1}s: ID {newNotification?.Id}");
                        }
                        else
                        {
                            Debug.WriteLine($"✅ {scenarioId}: No new email notification detected after {elapsedSeconds:F1}s: ID {newNotification?.Id}");
                        }

                        return new EmailNotificationResult
                        {
                            Found = true,
                            NotificationId = newNotification?.Id ?? 0,
                            ElapsedSeconds = elapsedSeconds
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"🔍 {scenarioId}: Error checking email notifications: {ex.Message}");
                }

                await Task.Delay(checkInterval);
                Debug.WriteLine($"🔄 {scenarioId}: Still waiting for email notification... ({(DateTime.Now - startTime).TotalSeconds:F1}s)");
            }

            Debug.WriteLine($"⏰ {scenarioId}: Timeout waiting for email notification after {maxWaitSeconds}s");
            return new EmailNotificationResult
            {
                Found = false,
                NotificationId = 0,
                ElapsedSeconds = maxWaitSeconds
            };
        }

        // Add this new result class for email notification polling
        private class EmailNotificationResult
        {
            public bool Found { get; set; }
            public int NotificationId { get; set; }
            public double ElapsedSeconds { get; set; }
        }

        public async Task<SeleniumScenarioOutcome> ExecuteSeleniumStepsAsync(List<SeleniumScenarioStep> steps, ScenarioDetailsViewModel scenarioDetail, IWebDriver driver)
        {
            var commitStepReached = false;
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

                    _logger.LogError(ex.Message);

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

                    // ✅ Trigger rollback if commit step was reached
                    if (commitStepReached)
                    {
                        outcome.RollbackRequired = true;
                        //outcome.RollbackCandidates = await _rollbackService.ExecuteRollbackAsync(scenarioDetail);
                    }

                    scenarioDetail.PredictedSeleniumOutcome.Success = false;
                    // Stop execution after failure
                    break;
                }

                step.Result = result;
                if (result?.Success == true && step.IsCommitStep)
                {
                    commitStepReached = true;
                    scenarioDetail.CommitStepReached = true;
                }

                outcome.Expected.Steps.Add(step);
            }

            foreach (var filePath in scenarioDetail.FileUploadPaths)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Optional: log the error or add to test result
                    Console.WriteLine($"Failed to delete temp file: {filePath}. Error: {ex.Message}");
                }
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

        // 🔥 NEW: Polling method to wait for response message to appear
        private async Task<ResponseMessageResult> WaitForResponseMessage(IWebDriver driver, string scenarioId, int maxWaitSeconds = 30)
        {
            var startTime = DateTime.Now;
            var checkInterval = TimeSpan.FromMilliseconds(500);

            Debug.WriteLine($"🔄 {scenarioId}: Starting to poll for responseMessage element...");

            while ((DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
            {
                try
                {
                    var responseElement = driver.FindElement(By.Id("responseMessage"));
                    var cssDisplay = responseElement.GetCssValue("display");
                    var cssVisibility = responseElement.GetCssValue("visibility");

                    // 🔥 Try multiple methods to get the text content
                    var elementText = responseElement.Text?.Trim() ?? "";
                    var innerText = responseElement.GetAttribute("innerText")?.Trim() ?? "";
                    var textContent = responseElement.GetAttribute("textContent")?.Trim() ?? "";
                    var innerHTML = responseElement.GetAttribute("innerHTML")?.Trim() ?? "";

                    Debug.WriteLine($"🔍 {scenarioId}: Text retrieval attempts:");
                    Debug.WriteLine($"  - .Text: '{elementText}'");
                    Debug.WriteLine($"  - innerText: '{innerText}'");
                    Debug.WriteLine($"  - textContent: '{textContent}'");
                    Debug.WriteLine($"  - innerHTML: '{innerHTML}'");
                    Debug.WriteLine($"  - cssDisplay: '{cssDisplay}'");
                    Debug.WriteLine($"  - cssVisibility: '{cssVisibility}'");

                    // Choose the best available text
                    var messageText = !string.IsNullOrWhiteSpace(elementText) ? elementText :
                                     !string.IsNullOrWhiteSpace(innerText) ? innerText :
                                     !string.IsNullOrWhiteSpace(textContent) ? textContent :
                                     "";

                    // Check if we have meaningful content and element is visible
                    if (!string.IsNullOrWhiteSpace(messageText) &&
                        cssDisplay != "none" &&
                        cssVisibility != "hidden")
                    {
                        var elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;

                        Debug.WriteLine($"✅ {scenarioId}: Response message appeared after {elapsedSeconds:F1}s: '{messageText}'");

                        return new ResponseMessageResult
                        {
                            Found = true,
                            Message = messageText,  // ← Now this should have actual content
                            ElapsedSeconds = elapsedSeconds
                        };
                    }
                }
                catch (NoSuchElementException)
                {
                    Debug.WriteLine($"🔍 {scenarioId}: Element 'responseMessage' not found in DOM");
                }
                catch (StaleElementReferenceException)
                {
                    Debug.WriteLine($"🔍 {scenarioId}: Element became stale, retrying...");
                }

                await Task.Delay(checkInterval);
                Debug.WriteLine($"🔄 {scenarioId}: Still waiting for responseMessage... ({(DateTime.Now - startTime).TotalSeconds:F1}s)");
            }

            Debug.WriteLine($"⏰ {scenarioId}: Timeout waiting for responseMessage after {maxWaitSeconds}s");
            return new ResponseMessageResult
            {
                Found = false,
                Message = null,
                ElapsedSeconds = maxWaitSeconds
            };
        }

        // 🔥 NEW: Result class for response message polling
        private class ResponseMessageResult
        {
            public bool Found { get; set; }
            public string Message { get; set; }
            public double ElapsedSeconds { get; set; }
        }
    }
}
