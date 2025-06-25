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

            return result as SeleniumStepResult
                   ?? new SeleniumStepResult { Success = false, Message = "Unexpected result type." };
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


            }

            return predictiveMethods;
        }

        public async Task GenerateSeleniumSteps(ScenarioDetailsViewModel scenarioDetail)
        {
            var predictiveSteps = new List<SeleniumScenarioStep>();

            var scenarioId = scenarioDetail.ScenarioId;
            var baseUrl = _workflowControllerService.GetAppKeyValueByKey("CapitalRequest", "CapitalRequestURL").LookupValue;
            var proposalId = scenarioDetail.ProposalId;
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(scenarioDetail.RequestingGroupId);
            var workflowPortion = $"{reviewerGroup.StepNumber} -{reviewerGroup.Name}";
            var workflowButtonId = "btnWorkflowActions";
            var workflowButtonText = "Workflow";
            var requestButtonId = "btnRequestMoreInfo";
            var requestButtonText = "Request More Information button";


            /*
	1. Navigate to View Request
a. http://caps-dev.ssmhc.com/CapitalRequest/Proposal/ViewProposal/2884
	2. Test 1 validate  Workflow button
	3. Click Workflow button
	4. Test 2 Validate Verify button for selected Reviewer Group


	5. Click Reviewer Group Verify button 
	6. Test 3 Validate no rejection message 
	7. Select Target Reviewer Group from dropdown by target Id
	8. Test 4 Validate Target Reviewer Group available
	9. Enter previously captured Request for information text into Text Area
	10. Validate Submit button
	11. Click Submit button
	12. Test 5 Validate success message
	13. Navigate to Home Dashboard
	14. Validate search box appears
	15. Enter Request Id into text box
	16. Validate Request 
	17. Test 6 Validate Request from Targeted Reviewer Group has Requesting Group and date
		a. <tbody>
			i. <tr class = "odd">
			ii. <td> will be 11th + dashboardorder column
              
             */
            if (scenarioId == "SCN001")
            {
                var WorkflowDashboardButtonText = Constants.ACTION_TYPE_VERIFY;

                predictiveSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = 1,
                    Description = "Full navigation and interaction chain to Workflow DashBoard page",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.NavigateTo($"{baseUrl}/ViewProposal/{proposalId}"))
                        .Then(Validate.ElementById(workflowButtonId, $"{workflowButtonText} button"))
                        .Then(Execute.ClickButtonById(workflowButtonId, workflowButtonText))
                        .Then(Validate.Text(workflowPortion))
                        .Then(Validate.ButtonInRowWithText(workflowPortion, WorkflowDashboardButtonText))
                        .Build("Reached Workflow DashBoard page")

                });

                predictiveSteps.Add(new SeleniumScenarioStep
                {
                    StepNumber = 2,
                    Description = $"Click '{WorkflowDashboardButtonText}' in row with WorkflowPortion '{workflowPortion}'",
                    Action = new SeleniumDsl()
                        .BeginWith(Execute.ClickButtonInRow(workflowPortion, WorkflowDashboardButtonText))
                        .Then(Validate.ElementNotPresentById("responseMessage", "Rejection message container"))
                        .Then(Validate.ButtonById(requestButtonId, requestButtonText))
                        .Build("Clicked Request and confirmed page transition")

                });


            }

        }
    }
}
