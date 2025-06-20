using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Microsoft.VisualBasic;
using SSMWorkflow.API.DataAccess.Models;
using System.Reflection;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveScenarioService
    {
        Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<PredictiveMethod> methods,ScenarioDetailsViewModel scenarioDetail);
    }
    public class PredictiveScenarioService : IPredictiveScenarioService
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

        
        public PredictiveScenarioService(ICapitalRequestServices capitalRequestServices,
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

        public async Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail)
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


                var methods = await GetScenarioMethodsAsync(scenarioDetail);
                scenarioDataViewModel = await ExecuteScenarioMethodsAsync( methods, scenarioDetail);
            }

            scenarioDataViewModel.ScenarioId = scenarioId;

            return scenarioDataViewModel;
        }

        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<PredictiveMethod> methods,ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioData = new ScenarioDataViewModel();

            var scenarioDataViewModel = new ScenarioDataViewModel();

            foreach (var method in methods)
            {
                scenarioDataViewModel = await ExecuteScenarioMethodAsync(method, scenarioDetail, scenarioData);
            }

            return scenarioDataViewModel;
        }


        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodAsync(PredictiveMethod method, ScenarioDetailsViewModel scenarioDetail, ScenarioDataViewModel scenarioDataViewModel)
        {

            var nameSpace = "CapitalRequestAutomatedTesting.UI.Services.";
            object serviceInstance = null;
            Type serviceType = null;

            var serviceName = $"{nameSpace}{method.ServiceName}";
            // Resolve service type
            if (serviceName == $"{nameSpace}IPredictiveRequestedInfoService")
                serviceType = typeof(IPredictiveRequestedInfoService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStepResponderService")
                serviceType = typeof(IPredictiveWorkflowStepResponderService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStepOptionService")
                serviceType = typeof(IPredictiveWorkflowStepOptionService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStepService")
                serviceType = typeof(IPredictiveWorkflowStepService);
            else if (serviceName == $"{nameSpace}IPredictiveEmailNotificationService")
                serviceType = typeof(IPredictiveEmailNotificationService);
            else if (serviceName == $"{nameSpace}IPredictiveScenarioService")
                serviceType = typeof(IPredictiveScenarioService);

            if (serviceType == null) return scenarioDataViewModel;

            // Get service instance
            using var scope = _scopeFactory.CreateScope();
            serviceInstance = scope.ServiceProvider.GetRequiredService(serviceType);

            //serviceInstance = _serviceProvider.GetService(serviceType);
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
            MapResultsToTables(scenarioDataViewModel, method.ServiceName, result, method.Operation);

            return scenarioDataViewModel;
        }

        private string DetermineTableName(string serviceName)
        {

            return serviceName switch
            {
                "IPredictiveRequestedInfoService" => "RequestedInfo",
                "IPredictiveWorkflowStepResponderService" => "WorkflowStepResponder",
                "IPredictiveWorkflowStepOptionService" => "WorkflowStepOption",
                "IPredictiveEmailNotificationService" => "EmailNotification",
                _ => "UnknownTable"
            };
        }

        private void MapResultsToTables(ScenarioDataViewModel scenarioDataViewModel, string serviceName, object result, CrudOperationType operationType)
        {
            var tableName = DetermineTableName(serviceName);

            if (string.IsNullOrEmpty(tableName)) return;

            if (!scenarioDataViewModel.Tables.ContainsKey(tableName))
            {
                scenarioDataViewModel.Tables[tableName] = new TableData
                {
                    TableName = tableName,
                    Records = new List<RecordEntry>()
                };
            }

            // Add new record with operation type
            scenarioDataViewModel.Tables[tableName].Records.Add(new RecordEntry
            {
                Operation = operationType,
                Data = result
            });
        }

        private async Task<List<PredictiveMethod>> GetScenarioMethodsAsync(ScenarioDetailsViewModel scenarioDetail)
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

                //TODO Remove after debugging
                increment = 0;

                var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();

                var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                               .Where(x => x.IsComplete == false && x.IsTerminate == false)
                               .ToList();

                var email = _userContextService.Email;

                predictiveMethods.Add(
                    new PredictiveMethod { 
                        ServiceName = "IPredictiveRequestedInfoService", 
                        MethodName = "CreateRequestedInfoAsync", 
                        Parameters = new List<object> { proposal, increment }, 
                        Operation = CrudOperationType.Insert 
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod { 
                        ServiceName = "IPredictiveWorkflowStepResponderService", 
                        MethodName = "CreateWorkflowStepResponderAsync", 
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REQUEST }, 
                        Operation = CrudOperationType.Insert 
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod { 
                        ServiceName = "IPredictiveWorkflowStepOptionService", 
                        MethodName = "CloseOptionsAsync", 
                        Parameters = new List<object> { proposal, Guid.Empty, Constants.OPTION_TYPE_VERIFY, null, Constants.OPTION_TYPE_REQUEST }, 
                        Operation = CrudOperationType.Update 
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod { 
                        ServiceName = "IPredictiveWorkflowStepOptionService", 
                        MethodName = "CreateWorkflowStepOptionsAsync", 
                        Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION, proposal.RequestedInfo.Id }, 
                        Operation = CrudOperationType.Insert 
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod { 
                        ServiceName = "IPredictiveEmailNotificationService", 
                        MethodName = "CreateEmailNotificationsAsync", 
                        Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION }, 
                        Operation = CrudOperationType.Insert 
                    }
                );

            }

            return predictiveMethods;
        }
        
    }
}
