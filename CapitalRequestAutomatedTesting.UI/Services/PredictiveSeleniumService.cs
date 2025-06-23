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
        Task<ScenarioDataViewModel> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<ScenarioDataViewModel> ExecuteSeleniumMethodsAsync(List<PredictiveMethod> methods,ScenarioDetailsViewModel scenarioDetail);
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

        public async Task<ScenarioDataViewModel> GenerateSeleniumOutcomeAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioDataViewModel = new ScenarioDataViewModel();
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
                scenarioDataViewModel = await ExecuteSeleniumMethodsAsync( methods, scenarioDetail);
            }

            scenarioDataViewModel.ScenarioId = scenarioId;

            return scenarioDataViewModel;
        }

        public async Task<ScenarioDataViewModel> ExecuteSeleniumMethodsAsync(List<PredictiveMethod> methods,ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioData = new ScenarioDataViewModel();

            var scenarioDataViewModel = new ScenarioDataViewModel();

            foreach (var method in methods)
            {
                scenarioDataViewModel = await ExecuteSeleniumMethodAsync(method, scenarioDetail, scenarioData);
            }

            return scenarioDataViewModel;
        }


        public async Task<ScenarioDataViewModel> ExecuteSeleniumMethodAsync(PredictiveMethod method, ScenarioDetailsViewModel scenarioDetail, ScenarioDataViewModel scenarioDataViewModel)
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

            if (serviceType == null) return scenarioDataViewModel;

            // Get service instance
            using var scope = _scopeFactory.CreateScope();
            serviceInstance = scope.ServiceProvider.GetRequiredService(serviceType);

            if (serviceInstance == null) return scenarioDataViewModel;

            // Get method info
            MethodInfo methodInfo = serviceInstance.GetType().GetMethod(method.MethodName);
            if (methodInfo == null) return scenarioDataViewModel;

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

            // Map results dynamically to ViewModel
            //MapResultsToTables(scenarioDataViewModel, method.ServiceName, result, method.Operation);
            //MapUiStepToGroup(scenario, "Page: ViewProposal", new PredictiveStepResult { ... });
            //MapUiStepToGroup(scenario, "Component: Verify Section", new PredictiveStepResult { ... });
            //MapUiStepToGroup(scenario, "Final Validation", new PredictiveStepResult { ... });


            return scenarioDataViewModel;
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

        public async Task<bool> ValidateWorkflowButtonExists (vm.Proposal proposal)
        {
            return (await GetWorkflowActionAsync(proposal)).Any();
        }

        private void MapUiStepToGroup(ScenarioDetailsViewModel scenarioDetail, string groupKey, PredictiveStepResult stepResult)
        {
            if (string.IsNullOrEmpty(groupKey)) return;

            if (!scenarioDetail.UiStepGroups.ContainsKey(groupKey))
            {
                scenarioDetail.UiStepGroups[groupKey] = new List<PredictiveStepResult>();
            }

            scenarioDetail.UiStepGroups[groupKey].Add(stepResult);
        }


        //private string DetermineTableName(string serviceName)
        //{

        //    return serviceName switch
        //    {
        //        "IPredictiveRequestedInfoService" => "RequestedInfo",
        //        "IPredictiveWorkflowStepResponderService" => "WorkflowStepResponder",
        //        "IPredictiveWorkflowStepOptionService" => "WorkflowStepOption",
        //        "IPredictiveEmailNotificationService" => "EmailNotification",
        //        _ => "UnknownTable"
        //    };
        //}

        //private void MapResultsToTables(ScenarioDataViewModel scenarioDataViewModel, string serviceName, object result, CrudOperationType operationType)
        //{
        //    var tableName = DetermineTableName(serviceName);

        //    if (string.IsNullOrEmpty(tableName)) return;

        //    if (!scenarioDataViewModel.Tables.ContainsKey(tableName))
        //    {
        //        scenarioDataViewModel.Tables[tableName] = new TableData
        //        {
        //            TableName = tableName,
        //            Records = new List<RecordEntry>()
        //        };
        //    }

        //    // Add new record with operation type
        //    scenarioDataViewModel.Tables[tableName].Records.Add(new RecordEntry
        //    {
        //        Operation = operationType,
        //        Data = result
        //    });
        //}

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
                    new PredictiveMethod { 
                        ServiceName = "IPredictiveWorkflowActionService", 
                        MethodName = "ValidateWorkflowButtonAsync", 
                        Parameters = new List<object> { proposal }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod { 
                        ServiceName = "IPredictiveWorkflowActionService", 
                        MethodName = "ValidateVerifyButtonAsync", 
                        Parameters = new List<object> { proposal, proposal.ReviewerGroupId }
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod { 
                        ServiceName = "IScenarioControllerService", 
                        MethodName = "ValidateTargetGroupIdAsync", 
                        Parameters = new List<object> { proposal, Guid.Empty, Constants.OPTION_TYPE_VERIFY, null, Constants.OPTION_TYPE_REQUEST }
                    }
                );

                
            }

            return predictiveMethods;
        }

        public void GenerateSeleniumSteps(ScenarioDataViewModel scenario)
        {
            var predictiveSteps = new List<SeleniumScenarioStep>();

            var scenarioId = scenario.ScenarioId;
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
                //predictiveSteps.Add(new SeleniumScenarioStep
                //{
                //    StepName = "Page: ViewProposal",
                //    Description = "View the proposal details page",
                //    Action = "Navigate to the proposal details page",
                //    ExpectedOutcome = "Proposal details are displayed correctly"
                //});
                //predictiveSteps.Add(new SeleniumScenarioStep
                //{
                //    StepName = "Component: Verify Section",
                //    Description = "Verify the requested information section",
                //    Action = "Check if the requested information is displayed",
                //    ExpectedOutcome = "Requested information is displayed as expected"
                //});
                //predictiveSteps.Add(new SeleniumScenarioStep
                //{
                //    StepName = "Final Validation",
                //    Description = "Validate the final state of the proposal",
                //    Action = "Ensure all required fields are filled and actions are completed",
                //    ExpectedOutcome = "Proposal is ready for submission"
                //});

            }

    }
}
