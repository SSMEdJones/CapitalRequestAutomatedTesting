using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using SSMWorkflow.API.DataAccess.Models;
using System.Reflection;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveScenarioService
    {
        Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(
            List<PredictiveMethod> methods,
            ScenarioDetailsViewModel scenarioDetail);
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
        private readonly IServiceProvider _serviceProvider;

        public PredictiveScenarioService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IPredictiveRequestedInfoService predictiveRequestedInfoService,
            IPredictiveWorkflowStepResponderService predictiveWorkflowStepResponderService,
            IPredictiveWorkflowStepOptionService predictiveWorkflowStepOptionService,
            IPredictiveEmailNotificationService predictiveEmailNotificationService,
            IServiceProvider serviceProvider)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _workflowControllerService = workflowControllerService;
            _predictiveRequestedInfoService = predictiveRequestedInfoService;
            _predictiveWorkflowStepResponderService = predictiveWorkflowStepResponderService;
            _predictiveWorkflowStepOptionService = predictiveWorkflowStepOptionService;
            _predictiveEmailNotificationService = predictiveEmailNotificationService;
            _serviceProvider = serviceProvider;
        }

        public async Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioDataViewModel = new ScenarioDataViewModel();
            var scenarioId = scenarioDetail.ScenarioId;
            if (scenarioId == "SCN001")
            {
                var methods = GetScenarioMethods(scenarioId);
                var results = await ExecuteScenarioMethodsAsync( methods, scenarioDetail);
            }

            return scenarioDataViewModel;
        }

        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(
            List<PredictiveMethod> methods,
            ScenarioDetailsViewModel scenarioDetail)
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
            serviceInstance = _serviceProvider.GetService(serviceType);
            if (serviceInstance == null) return scenarioDataViewModel;

            // Get method info
            MethodInfo methodInfo = serviceInstance.GetType().GetMethod(method.MethodName);
            if (methodInfo == null) return scenarioDataViewModel;

            object[] formattedParameters = method.Parameters?.ToArray() ?? new object[] { };

            object result = methodInfo.Invoke(serviceInstance, formattedParameters);
            var proposal = await _capitalRequestServices.GetProposal(scenarioDetail.ProposalId);

            proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
            proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 

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


        //private void MapResultsToTables(ScenarioDataViewModel scenarioDataViewModel, string serviceName, object result, CrudOperationType operationType)
        //{
        //    var tableName = DetermineTableName(serviceName);

        //    if (string.IsNullOrEmpty(tableName)) return;

        //    if (!scenarioDataViewModel.Tables.ContainsKey(tableName))
        //    {
        //        scenarioDataViewModel.Tables[tableName] = new TableData
        //        {
        //            TableName = tableName,
        //            Operation = operationType,
        //            Records = new List<object> { result }
        //        };
        //    }
        //    else
        //    {
        //        scenarioDataViewModel.Tables[tableName].Records.Add(result);
        //    }
        //}

        //private void MapResultsToTables(ScenarioDataViewModel scenarioDataViewModel, string serviceName, object result)
        //{
        //    var tableName = DetermineTableName(serviceName);

        //    if (string.IsNullOrEmpty(tableName)) return;

        //    // Check if the table already exists
        //    if (!scenarioDataViewModel.Tables.ContainsKey(tableName))
        //    {
        //        // If not, create a new list and add the result
        //        scenarioDataViewModel.Tables[tableName] = new List<object> { result };
        //    }
        //    else
        //    {
        //        // If it exists, add to the existing list
        //        (scenarioDataViewModel.Tables[tableName] as List<object>)?.Add(result);
        //    }
        //}

        private List<PredictiveMethod> GetScenarioMethods(string scenarioId)
        {
            var predictiveMethods = new List<PredictiveMethod>();

            if (scenarioId == "SCN001")
            {
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 1,
                    Order = 1,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveRequestedInfoService",
                    MethodName = "CreateRequestedInfoAsync",
                    Parameters = new List<object> { new vm.Proposal(), 0},
                    Operation = CrudOperationType.Insert
                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 2,
                    Order = 2,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveWorkflowStepResponderService",
                    MethodName = "CreateWorkflowStepResponderAsync",
                    Parameters = new List<object> { new vm.Proposal(), string.Empty },
                    Operation = CrudOperationType.Insert

                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 3,
                    Order = 3,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveWorkflowStepOptionService",
                    MethodName = "CloseOptionsAsync",
                    Parameters = new List<object> { new vm.Proposal(), Guid.Empty, string.Empty, null, string.Empty },
                    Operation = CrudOperationType.Update


                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 4,
                    Order = 4,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveWorkflowStepOptionService",
                    MethodName = "CreateWorkflowStepOptionsAsync",
                    Parameters = new List<object> { new vm.Proposal(), string.Empty, 0},
                    Operation = CrudOperationType.Insert
                    
                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 5,
                    Order = 5,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveEmailNotificationService",
                    MethodName = "CreateEmailNotificationsAsync",
                    Parameters = new List<object> { new vm.Proposal(), string.Empty },
                    Operation = CrudOperationType.Insert

                });


            }

            return predictiveMethods;
        }

        
    }
}
