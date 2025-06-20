using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using SSMWorkflow.API.DataAccess.Models;
using System.Reflection;
using vm = CapitalRequest.API.Models;
using RequestedInfoSearchFilter = CapitalRequest.API.DataAccess.Models.RequestedInfoSearchFilter;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IActualScenarioService
    {
        Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<ActualMethod> methods, ScenarioDetailsViewModel scenarioDetail);
    }
    public class ActualScenarioService : IActualScenarioService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly IActualRequestedInfoService _actualRequestedInfoService;
        private IActualWorkflowStepResponderService _actualWorkflowStepResponderService;
        private IActualWorkflowStepOptionService _actualWorkflowStepOptionService;
        private IActualEmailNotificationService _actualEmailNotificationService;
        private readonly IUserContextService _userContextService;
        private readonly IServiceScopeFactory _scopeFactory;


        public ActualScenarioService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IActualRequestedInfoService actualRequestedInfoService,
            IActualWorkflowStepResponderService actualWorkflowStepResponderService,
            IActualWorkflowStepOptionService actualWorkflowStepOptionService,
            IActualEmailNotificationService actualEmailNotificationService,
            IUserContextService userContextService,
            IServiceScopeFactory scopeFactory)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _workflowControllerService = workflowControllerService;
            _actualRequestedInfoService = actualRequestedInfoService;
            _actualWorkflowStepResponderService = actualWorkflowStepResponderService;
            _actualWorkflowStepOptionService = actualWorkflowStepOptionService;
            _actualEmailNotificationService = actualEmailNotificationService;
            _userContextService = userContextService;
            _scopeFactory = scopeFactory;
        }

        public async Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioDataViewModel = new ScenarioDataViewModel();
            var scenarioId = scenarioDetail.ScenarioId;

            if (scenarioId == "SCN001")
            {
                var methods = await GetScenarioMethodsAsync(scenarioDetail);
                scenarioDataViewModel = await ExecuteScenarioMethodsAsync(methods, scenarioDetail);
            }

            scenarioDataViewModel.ScenarioId = scenarioId;

            return scenarioDataViewModel;
        }

        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<ActualMethod> methods, ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioData = new ScenarioDataViewModel();

            var scenarioDataViewModel = new ScenarioDataViewModel();

            foreach (var method in methods)
            {
                scenarioDataViewModel = await ExecuteScenarioMethodAsync(method, scenarioDetail, scenarioData);
            }

            return scenarioDataViewModel;
        }


        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodAsync(ActualMethod method, ScenarioDetailsViewModel scenarioDetail, ScenarioDataViewModel scenarioDataViewModel)
        {

            var nameSpace = "CapitalRequestAutomatedTesting.UI.Services.";
            object serviceInstance = null;
            Type serviceType = null;

            var serviceName = $"{nameSpace}{method.ServiceName}";
            // Resolve service type
            if (serviceName == $"{nameSpace}IActualRequestedInfoService")
                serviceType = typeof(IActualRequestedInfoService);
            else if (serviceName == $"{nameSpace}IActualWorkflowStepResponderService")
                serviceType = typeof(IActualWorkflowStepResponderService);
            else if (serviceName == $"{nameSpace}IActualWorkflowStepOptionService")
                serviceType = typeof(IActualWorkflowStepOptionService);
            else if (serviceName == $"{nameSpace}IActualEmailNotificationService")
                serviceType = typeof(IActualEmailNotificationService);
            else if (serviceName == $"{nameSpace}IActualScenarioService")
                serviceType = typeof(IActualScenarioService);

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
                "IActualRequestedInfoService" => "RequestedInfo",
                "IActualWorkflowStepResponderService" => "WorkflowStepResponder",
                "IActualWorkflowStepOptionService" => "WorkflowStepOption",
                "IActualEmailNotificationService" => "EmailNotification",
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

        private async Task<List<ActualMethod>> GetScenarioMethodsAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var actualMethods = new List<ActualMethod>();
            var scenarioId = scenarioDetail.ScenarioId;

            if (scenarioId == "SCN001")
            {
                var proposal = await _capitalRequestServices.GetProposal(scenarioDetail.ProposalId);
                var requestedInfo = (await _capitalRequestServices
                    .GetAllRequestedInfos(new RequestedInfoSearchFilter
                    {
                        ProposalId = proposal.Id,
                        RequestingReviewerGroupId = scenarioDetail.RequestingGroupId,
                        ReviewerGroupId = scenarioDetail.TargetGroupId,
                        IsOpen = true
                    }))
                   .FirstOrDefault();
                    
                proposal.ReviewerGroupId = scenarioDetail.RequestingGroupId;
                proposal.RequestedInfo = requestedInfo ?? new vm.RequestedInfo();
                proposal.ReviewerId = scenarioDetail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);

                var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();

                var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                               .Where(x => x.IsComplete == false && x.IsTerminate == false)
                               .ToList();

                var email = _userContextService.Email;

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualRequestedInfoService",
                        MethodName = "GetRequestedInfoAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepResponderService",
                        MethodName = "GetWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REQUEST },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetRequestTypeClosedWorkflowStepOptionAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Update
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetRequestTypeWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualEmailNotificationService",
                        MethodName = "GetRequestEmailNotificationsAsync",
                        Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION },
                        Operation = CrudOperationType.Insert
                    }
                );

            }

            return actualMethods;
        }

    }
}
