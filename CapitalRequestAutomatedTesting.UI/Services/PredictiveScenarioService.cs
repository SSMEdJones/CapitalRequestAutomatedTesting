using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
using System.Reflection;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveScenarioService
    {
        ScenarioDataViewModel ExecuteScenarioMethods(List<PredictiveTable> tables, List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail);
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
            IPredictiveRequestedInfoService  predictiveRequestedInfoService,
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

        public ScenarioDataViewModel GenerateScenarioData(ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioDataViewModel = new ScenarioDataViewModel();
            var scenarioId = scenarioDetail.ScenarioId;
            if (scenarioId == "SCN001")
            {

                var tables = GetScenarioTables(scenarioId);
                var methods = GetScenarioMethods(scenarioId);

                var results = ExecuteScenarioMethods(tables, methods, scenarioDetail);
            }

            return scenarioDataViewModel;
        }

        //private ScenarioDataViewModel ExecuteScenarioMethods(List<PredictiveMethod> methods)
        public ScenarioDataViewModel ExecuteScenarioMethods(List<PredictiveTable> tables, List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioDataViewModel = new ScenarioDataViewModel();

            methods.ForEach(method =>
            {
                // Resolve service dynamically via DI
                Debug.WriteLine($"Resolved Type: {typeof(IPredictiveRequestedInfoService)}");
                Debug.WriteLine($"Service Instance: {_serviceProvider.GetService(typeof(IPredictiveRequestedInfoService))}");

                var debug = _serviceProvider.GetService(Type.GetType(method.ServiceName));

                var serviceInstance = _serviceProvider.GetService(typeof(IPredictiveRequestedInfoService));

                if (serviceInstance == null) return;

                // Get method info
                MethodInfo methodInfo = serviceInstance.GetType().GetMethod(method.MethodName);
                if (methodInfo == null) return;

                // Convert parameters to an object array
                object[] parameters = method.Parameters?.Select(p => Convert.ChangeType(p, methodInfo.GetParameters()[0].ParameterType)).ToArray() ?? new object[] { };

                // Invoke the method dynamically
                object result = methodInfo.Invoke(serviceInstance, parameters);

                // Map results dynamically to ViewModel
                MapResultsToViewModel(scenarioDataViewModel, method.ServiceName, result);
            });

            return scenarioDataViewModel;
        }
        private void MapResultsToViewModel(ScenarioDataViewModel scenarioDataViewModel, string serviceName, object result)
        {
            switch (serviceName)
            {
                case "_predictiveRequestedInfoService":
                    scenarioDataViewModel.RequestedInfo = (dto.RequestedInfo)result;
                    break;
                case "_predictiveWorkflowStepResponderService":
                    scenarioDataViewModel.WorkflowStepResponder = (WorkflowStepResponder)result;
                    break;
                case "_predictiveWorkflowStepOptionService":
                    scenarioDataViewModel.WorkflowStepOptions = (List<WorkFlowStepOptionViewModel>)result;
                    break;
                case "_predictiveEmailNotificationService":
                    scenarioDataViewModel.EmailNotifications = (List<SSMWorkflow.API.DataAccess.Models.EmailNotification>)result;
                    break;
            }
        }
        //private ScenarioDataViewModel ExecuteScenarioMethods(List<PredictiveTable> tables, List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail)
        //{
        //    var scenarioDataViewModel = new ScenarioDataViewModel();

        //    methods.ForEach(x =>
        //    {
        //        switch (x.ServiceName)
        //        {
        //            case "PredictiveRequestedInfoService":
        //                scenarioDataViewModel.RequestedInfo = x.ServiceName + x.MethodName + ParameterList
        //                break;
        //            case "PredictiveWorkflowStepResponderService":
        //                scenarioDataViewModel.WorkflowStepResponder = _ssmWorkflowServices.CreateWorkflowStepResponderAsync(x.MethodName).Result;
        //                break;
        //            case "PredictiveWorkflowStepOptionService":
        //                if (x.MethodName == "CloseOptionsAsync")
        //                {
        //                    scenarioDataViewModel.WorkflowStepOptions = _ssmWorkflowServices.CloseOptionsAsync().Result;
        //                }
        //                else if (x.MethodName == "CreateWorkflowStepOptionsAsync")
        //                {
        //                    scenarioDataViewModel.WorkflowStepOptions = _ssmWorkflowServices.CreateWorkflowStepOptionsAsync().Result;
        //                }
        //                break;
        //            case "PredictiveEmailNotificationService":
        //                scenarioDataViewModel.EmailNotifications = _ssmWorkflowServices.CreateEmailNotificationsAsync().Result;
        //                break;
        //        }

        //    });
        //    return scenarioDataViewModel;
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
                    Parameters = new string[] { "proposal", "increment" }
                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 2,
                    Order = 2,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveWorkflowStepResponderService",
                    MethodName = "CreateWorkflowStepResponderAsync",
                    Parameters = new string[] { "proposal", "responderType" }
                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 3,
                    Order = 3,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveWorkflowStepOptionService",
                    MethodName = "CloseOptionsAsync",
                    Parameters = new string[] { "proposal, optionId, OptionType, requestedInfoId, actionType" }
                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 4,
                    Order = 4,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveWorkflowStepOptionService",
                    MethodName = "CreateWorkflowStepOptionsAsync",
                    Parameters = new string[] { "proposal,OptionType, requestedInfoId " }
                });
                predictiveMethods.Add(new PredictiveMethod
                {
                    Id = 5,
                    Order = 5,
                    ScenarioId = "SCN001",
                    ServiceName = "_predictiveEmailNotificationService",
                    MethodName = "CreateEmailNotificationsAsync",
                    Parameters = new string[] { "proposal, emailType" }
                });


            }

            return predictiveMethods;
        }

        private List<PredictiveTable> GetScenarioTables(string scenarioId)
        {
            var predictiveTables = new List<PredictiveTable>();

            if (scenarioId == "SCN001")
            {
                predictiveTables.Add(new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "EmailNotifications" });
                predictiveTables.Add(new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "WorkflowInstanceActionHistory" });
                predictiveTables.Add(new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "WorkflowStepOption" });
                predictiveTables.Add(new PredictiveTable { DatabaseName = "SSMWorkflow", TableName = "WorkflowStepResponder" });
                predictiveTables.Add(new PredictiveTable { DatabaseName = "CapitalRequest", TableName = "RequestedInfo" });

            }

            return predictiveTables;
        }
    }
}
