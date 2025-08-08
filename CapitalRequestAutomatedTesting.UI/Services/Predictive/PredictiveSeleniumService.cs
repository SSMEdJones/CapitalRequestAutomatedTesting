using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using System.Reflection;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveSeleniumService
    {
        Task<SeleniumScenarioOutcome> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<SeleniumScenarioOutcome> ExecuteSeleniumMethodsAsync(List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail);
    }

    public class PredictiveSeleniumService : IPredictiveSeleniumService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly IPredictiveRequestedInfoService _predictiveRequestedInfoService;
        private readonly IPredictiveWorkflowStepResponderService _predictiveWorkflowStepResponderService;
        private readonly IPredictiveWorkflowStepOptionService _predictiveWorkflowStepOptionService;
        private readonly IPredictiveEmailNotificationService _predictiveEmailNotificationService;
        private readonly IUserContextService _userContextService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public PredictiveSeleniumService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IPredictiveRequestedInfoService predictiveRequestedInfoService,
            IPredictiveWorkflowStepResponderService predictiveWorkflowStepResponderService,
            IPredictiveWorkflowStepOptionService predictiveWorkflowStepOptionService,
            IPredictiveEmailNotificationService predictiveEmailNotificationService,
            IUserContextService userContextService,
            IServiceScopeFactory scopeFactory,
            IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _workflowControllerService = workflowControllerService;
            _predictiveRequestedInfoService = predictiveRequestedInfoService;
            _predictiveWorkflowStepResponderService = predictiveWorkflowStepResponderService;
            _predictiveWorkflowStepOptionService = predictiveWorkflowStepOptionService;
            _predictiveEmailNotificationService = predictiveEmailNotificationService;
            _userContextService = userContextService;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public async Task<SeleniumScenarioOutcome> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var seleniumScenarioOutcome = new SeleniumScenarioOutcome();
            var scenarioId = scenarioDetail.ScenarioId;
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);

            var requestingGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
            var reviewer = await _capitalRequestServices.GetReviewer(detail.ReviewerId);

            var proposal = await _capitalRequestServices.GetProposal(detail.ProposalId);
            proposal.ReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestedInfo.RequestedInformation = detail.RequestedInformation;
            proposal.ReviewerId = detail.ReviewerId;
            proposal.Reviewer = reviewer;

            scenarioDetail.SelectedProperties["Scenario Name"] = scenarioDetail.DisplayText;
            scenarioDetail.SelectedProperties["Req Id"] = scenarioDetail.ProposalId.ToString();
            scenarioDetail.SelectedProperties["Requesting Group"] = requestingGroup.Name;
            scenarioDetail.SelectedProperties["Reviewer"] = reviewer.FullName;
            scenarioDetail.SelectedProperties["Requested Information"] = scenarioDetail.RequestedInformation;
            scenarioDetail.SelectedProperties["Reviewer"] = reviewer.FullName;


            if (scenarioId == "SCN001")
            {
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(detail.TargetGroupId);
                proposal.RequestedInfo.ReviewerGroupId = detail.TargetGroupId;

                scenarioDetail.SelectedProperties["Target Group"] = targetGroup.Name;

                //var methods = await GetSeleniumMethodsAsync(scenarioDetail);
                //seleniumScenarioOutcome = await ExecuteSeleniumMethodsAsync(methods, scenarioDetail);
            }

            if (scenarioId == "SCN002")
            {
                var replyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);

                scenarioDetail.SelectedProperties["Replying Group"] = replyingGroup.Name;
                scenarioDetail.SelectedProperties["Returned Information"] = scenarioDetail.ReturnedInformation;

            }

            var methods = await GetSeleniumMethodsAsync(scenarioDetail);
            seleniumScenarioOutcome = await ExecuteSeleniumMethodsAsync(methods, scenarioDetail);

            seleniumScenarioOutcome.ScenarioId = scenarioId;

            return seleniumScenarioOutcome;
        }

        public async Task<SeleniumScenarioOutcome> ExecuteSeleniumMethodsAsync(List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail)
        {
            var outcome = scenarioDetail.PredictedSeleniumOutcome;
            //outcome.Expected
            //{
            //    Expected = new SeleniumScenarioResult()
            //};

            bool hasFailed = false;
            scenarioDetail.PredictedSeleniumOutcome.Success = true;

            foreach (var method in methods)
            {
                SeleniumStepResult result;

                if (!hasFailed)
                {
                    result = await ExecuteSeleniumMethodAsync(method, scenarioDetail);

                    if (!result.Success)
                    {
                        hasFailed = true;
                        outcome.Expected.Success = false;
                        outcome.Success = false;
                        scenarioDetail.PredictiveStopReason = $"Step {method.StepNumber} failed due to: {result.Message}";
                    }
                    else
                    {
                        scenarioDetail.PredictiveCompletionStep = method.StepNumber;
                    }
                }
                else
                {
                    // After failure, mark remaining steps as skipped
                    result = new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"Step skipped due to predictive failure at step {scenarioDetail.PredictiveCompletionStep}."
                    };
                }

                var step = new SeleniumScenarioStep
                {
                    StepNumber = method.StepNumber,
                    Description = $"{method.MethodName} → {method.Parameters?.FirstOrDefault()?.ToString() ?? "no params"}",
                    Result = result,
                    Action = _ => Task.FromResult(result)
                };

                outcome.Expected.Steps.Add(step);
                outcome.Expected.Messages.Add(result.Message);
            }

            return outcome;
        }


        public async Task<SeleniumStepResult> ExecuteSeleniumMethodAsync(PredictiveMethod method, ScenarioDetailsViewModel scenarioDetail)
        {
            var nameSpace = "CapitalRequestAutomatedTesting.UI.Services.";
            object serviceInstance = null;
            Type serviceType = null;

            var serviceName = $"{nameSpace}{method.ServiceName}";
            // Resolve service type
            if (serviceName == $"{nameSpace}IPredictiveWorkflowActionService")
                serviceType = typeof(IPredictiveWorkflowActionService);
            else if (serviceName == $"{nameSpace}IScenarioControllerService")
                serviceType = typeof(IScenarioControllerService);
            else if (serviceName == $"{nameSpace}IPredictiveDashboardService")
                serviceType = typeof(IPredictiveDashboardService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStepOptionService")
                serviceType = typeof(IPredictiveWorkflowStepOptionService);


            // Get service instance
            using var scope = _scopeFactory.CreateScope();
            serviceInstance = scope.ServiceProvider.GetRequiredService(serviceType);

            // Get method info
            MethodInfo methodInfo = serviceInstance.GetType().GetMethod(method.MethodName);

            object[] formattedParameters = method.Parameters?.ToArray() ?? new object[] { };

            object result = methodInfo.Invoke(serviceInstance, formattedParameters);
            if (result is Task taskResult) // If method returns a Task
            {
                await taskResult.ConfigureAwait(false); // Await task completion
                await Task.Delay(200); // Temporary delay to test execution timing

                // If Task<T>, retrieve the actual result
                var resultProperty = taskResult.GetType().GetProperty("Result");
                result = resultProperty?.GetValue(taskResult);
            }

            // Ensure inner async methods are awaited properly
            if (result is Task innerTaskResult)
            {
                //await innerTaskResult.ConfigureAwait(false);
                var innerResultProperty = innerTaskResult.GetType().GetProperty("Result");
                result = innerResultProperty?.GetValue(innerTaskResult);
            }

            if (result is SeleniumStepResult stepResult)
            {
                return stepResult;
            }

            return new SeleniumStepResult { Success = false, Message = "Unexpected result type." };

        }


        public async Task<List<vm.WorkflowAction>> GetWorkflowActionAsync(vm.Proposal proposal)
        {

            var workflowActions = await _capitalRequestServices.GetAllWorkflowActions(new WorkflowActionSearchFilter
            {
                Id = proposal.Id,
                UserId = proposal.Reviewer.UserId,
                Email = proposal.Reviewer.Email,
            });

            return workflowActions;
        }

        //public async Task<bool> ValidateWorkflowButtonExists(vm.Proposal proposal)
        //{
        //    return (await GetWorkflowActionAsync(proposal)).Any();
        //}

        public async Task<bool> ValidateWorkflowButtonExists(vm.Proposal proposal, string actionType)
        {
            var workflowActions = await GetWorkflowActionAsync(proposal);
            if (workflowActions == null || !workflowActions.Any())
                return false;

            if (actionType != null)
            {
                workflowActions = workflowActions
                    .Where(x => x.ActionType == actionType)
                    .ToList();
            }

            return workflowActions.Any();
        }

        private async Task<List<PredictiveMethod>> GetSeleniumMethodsAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var actionType = string.Empty;
            var expectedMessage = string.Empty;
            var predictiveMethods = new List<PredictiveMethod>();
            var scenarioId = scenarioDetail.ScenarioId;
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);
            var requestingGroupId = detail.RequestingGroupId;
            var replyingGroupId = detail.ReplyingGroupId;
            var requestingGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
            var replyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);

            var proposal = await _capitalRequestServices.GetProposal(detail.ProposalId);
            proposal.RequestingGroupId = detail.RequestingGroupId;
            proposal.ReplyingGroupId = detail.ReplyingGroupId;
            proposal.ReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestedInfo.ReviewerGroupId = detail.TargetGroupId;
            proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestedInfo.RequestedInformation = detail.RequestedInformation;
            proposal.ReturnedInformation = detail.ReturnedInformation;

            proposal.ReviewerId = detail.ReviewerId;
            proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);

            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => !x.IsComplete)
                .FirstOrDefault();

            proposal.WorkflowStepId = workflowStep.WorkflowStepID;
            proposal.WorkflowStep = await _ssmWorkflowServices.GetWorkflowStep(workflowStep.WorkflowStepID);
            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                .Where(x => x.IsComplete == false && x.IsTerminate == false)
                .ToList();

            var stepNumber = 0;

            if (scenarioId == "SCN001")
            {
                var targetGroupId = detail.TargetGroupId;
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(targetGroupId);
                proposal.ButtonCaption = Constants.BUTTON_CAPTION_VERIFY;

                proposal.ActionType = Constants.ACTION_TYPE_VERIFY;
                actionType = proposal.ActionType;
                expectedMessage = Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT;
                
                var increment = 1;

                proposal.RequestedInfo.Id = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter())).Max(x => x.Id) + increment; ;

                //var email = _userContextService.Email;

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        StepNumber = ++stepNumber,
                        ServiceName = "IPredictiveWorkflowActionService",
                        MethodName = "ValidateWorkflowButtonAsync",
                        Parameters = new List<object> { proposal }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        StepNumber = ++stepNumber,
                        ServiceName = "IPredictiveWorkflowActionService",
                        MethodName = "ValidateVerifyButtonAsync",
                        Parameters = new List<object> { proposal, requestingGroupId, expectedMessage }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        StepNumber = ++stepNumber,
                        ServiceName = "IScenarioControllerService",
                        MethodName = "ValidateTargetGroupIdAsync",
                        Parameters = new List<object> { proposal, requestingGroupId, targetGroupId }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        StepNumber = ++stepNumber,
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "ValidateResponseMessageAsync",
                        Parameters = new List<object> { proposal, actionType, expectedMessage }
                    }
                );

                predictiveMethods.Add(
                   new PredictiveMethod
                   {
                       StepNumber = ++stepNumber,
                       ServiceName = "IPredictiveDashboardService",
                       MethodName = "ValidateDashboardStatusAsync",
                       Parameters = new List<object> { proposal, requestingGroup.Name, targetGroup.Name, Constants.DASHBOARD_STATUS_INFORMATION_REQUESTED }
                   }
               );

            }
            else if (scenarioId == "SCN002")
            {
                proposal.ActionType = Constants.ACTION_TYPE_ADD_INFO;
                proposal.ButtonCaption = Constants.BUTTON_CAPTION_REPLY;
                proposal.ExpectedMessage = Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT;
                actionType = proposal.ActionType;
                expectedMessage = Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT;

                var filter = new RequestedInfoSearchFilter
                {
                    ProposalId = detail.ProposalId,
                    RequestingReviewerGroupId = detail.RequestingGroupId,
                    ReviewerGroupId = detail.ReplyingGroupId,
                    IsOpen = true

                };
                var requestedInfo = (await _capitalRequestServices.GetAllRequestedInfos(filter)).FirstOrDefault();
                if (requestedInfo != null)
                {
                    proposal.RequestedInfo = requestedInfo;
                }
                    
                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        StepNumber = ++stepNumber,
                        ServiceName = "IPredictiveWorkflowActionService",
                        MethodName = "ValidateWorkflowButtonAsync",
                        Parameters = new List<object> { proposal }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        StepNumber = ++stepNumber,
                        ServiceName = "IPredictiveWorkflowActionService",
                        MethodName = "ValidateActionButtonAsync",
                        Parameters = new List<object> { proposal, requestingGroupId, expectedMessage }
                    }
                );

                //predictiveMethods.Add(
                //    new PredictiveMethod
                //    {
                //        StepNumber = ++stepNumber,
                //        ServiceName = "IScenarioControllerService",
                //        MethodName = "ValidateRequestingGroupForReplyIdAsync",
                //        Parameters = new List<object> { proposal, requestingGroupId, replyingGroupId }
                //    }
                //);

                predictiveMethods.Add(
                   new PredictiveMethod
                   {
                       StepNumber = ++stepNumber,
                       ServiceName = "IPredictiveWorkflowStepOptionService",
                       MethodName = "ValidateResponseMessageAsync",
                       Parameters = new List<object> { proposal, actionType, expectedMessage }
                   }
               );

                predictiveMethods.Add(
                   new PredictiveMethod
                   {
                       StepNumber = ++stepNumber,
                       ServiceName = "IPredictiveDashboardService",
                       MethodName = "ValidateDashboardStatusAsync",
                       Parameters = new List<object> { proposal, requestingGroup.Name, requestingGroup.Name, Constants.DASHBOARD_STATUS_CLEAR }
                   }
               );

            }

            return predictiveMethods;
        }

    }
}
