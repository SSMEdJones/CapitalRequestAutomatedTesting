using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using System.Reflection;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
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
        private IPredictiveWorkflowStepResponderService _predictiveWorkflowStepResponderService;
        private IPredictiveWorkflowStepOptionService _predictiveWorkflowStepOptionService;
        private IPredictiveEmailNotificationService _predictiveEmailNotificationService;
        private readonly IUserContextService _userContextService;
        private readonly IServiceScopeFactory _scopeFactory;


        public PredictiveSeleniumService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IPredictiveRequestedInfoService predictiveRequestedInfoService,
            IPredictiveWorkflowStepResponderService predictiveWorkflowStepResponderService,
            IPredictiveWorkflowStepOptionService predictiveWorkflowStepOptionService,
            IPredictiveEmailNotificationService predictiveEmailNotificationService,
            IUserContextService userContextService,
            IServiceScopeFactory scopeFactory)
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


                var methods = await GetSeleniumMethodsAsync(scenarioDetail);
                seleniumScenarioOutcome = await ExecuteSeleniumMethodsAsync(methods, scenarioDetail);
            }

            seleniumScenarioOutcome.ScenarioId = scenarioId;

            return seleniumScenarioOutcome;
        }

        public async Task<SeleniumScenarioOutcome> ExecuteSeleniumMethodsAsync(List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail)
        {
            var outcome = new SeleniumScenarioOutcome
            {
                Expected = new SeleniumScenarioResult()
            };

            int stepNumber = 1;

            foreach (var method in methods)
            {
                var result = await ExecuteSeleniumMethodAsync(method, scenarioDetail);
                var step = new SeleniumScenarioStep
                {
                    StepNumber = stepNumber++,
                    Description = $"{method.MethodName} → {method.Parameters?.FirstOrDefault()?.ToString() ?? "no params"}",
                    Result = result,
                    Action = _ => Task.FromResult(result) // Not executable in prediction, but keeps step shape consistent
                };

                outcome.Expected.Steps.Add(step);
                outcome.Expected.Messages.Add(result.Message);
                if (!result.Success)
                    outcome.Expected.Success = false;
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

            if (result is bool boolResult)
            {
                return new SeleniumStepResult
                {
                    Success = boolResult,
                    Message = $"Boolean result: {boolResult}"
                };
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

        public async Task<bool> ValidateWorkflowButtonExists(vm.Proposal proposal)
        {
            return (await GetWorkflowActionAsync(proposal)).Any();
        }


        private async Task<List<PredictiveMethod>> GetSeleniumMethodsAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var predictiveMethods = new List<PredictiveMethod>();
            var scenarioId = scenarioDetail.ScenarioId;

            if (scenarioId == "SCN001")
            {
                var proposal = await _capitalRequestServices.GetProposal(scenarioDetail.ProposalId);
                proposal.ReviewerGroupId = scenarioDetail.RequestingGroupId;
                proposal.RequestedInfo.ReviewerGroupId = scenarioDetail.TargetGroupId;
                proposal.RequestedInfo.RequestingReviewerGroupId = scenarioDetail.RequestingGroupId;
                proposal.RequestedInfo.RequestedInformation = scenarioDetail.RequestedInformation;
                proposal.ReviewerId = scenarioDetail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);
                var requestingGroupId = scenarioDetail.RequestingGroupId;
                var targetGroupId = scenarioDetail.TargetGroupId;

                var requestingGroup = await _capitalRequestServices.GetReviewerGroup(requestingGroupId);
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(targetGroupId);

                var actionType = Constants.ACTION_TYPE_VERIFY;
                var expectedMessage = Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT;
                var increment = 1;

                proposal.RequestedInfo.Id = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter())).Max(x => x.Id) + increment; ;

                var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();

                var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                               .Where(x => x.IsComplete == false && x.IsTerminate == false)
                               .ToList();

                var email = _userContextService.Email;

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowActionService",
                        MethodName = "ValidateWorkflowButtonAsync",
                        Parameters = new List<object> { proposal }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowActionService",
                        MethodName = "ValidateVerifyButtonAsync",
                        Parameters = new List<object> { proposal, proposal.ReviewerGroupId }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IScenarioControllerService",
                        MethodName = "ValidateTargetGroupIdAsync",
                        Parameters = new List<object> { proposal, requestingGroupId, targetGroupId }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "ValidateResponseMessageAsync",
                        Parameters = new List<object> { proposal, actionType, expectedMessage }
                    }
                );

                predictiveMethods.Add(
                   new PredictiveMethod
                   {
                       ServiceName = "IPredictiveDashboardService",
                       MethodName = "ValidateDashboardStatusAsync",
                       Parameters = new List<object> { proposal, requestingGroup.Name, targetGroup.Name, Constants.DASHBOARD_STATUS_INFORMATION_REQUESTED }
                   }
               );

            }

            return predictiveMethods;
        }
       
    }
}
