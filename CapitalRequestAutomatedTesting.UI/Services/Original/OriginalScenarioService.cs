using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using SSMWorkflow.API.DataAccess.Models;
using System.Reflection;
using RequestedInfoSearchFilter = CapitalRequest.API.DataAccess.Models.RequestedInfoSearchFilter;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Original
{
    public interface IOriginalScenarioService
    {
        Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<OriginalMethod> methods, ScenarioDetailsViewModel scenarioDetail);
    }
    
    public class OriginalScenarioService : IOriginalScenarioService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly IActualRequestedInfoService _actualRequestedInfoService;
        private readonly IActualWorkflowStepResponderService _actualWorkflowStepResponderService;
        private readonly IActualWorkflowStepOptionService _actualWorkflowStepOptionService;
        private readonly IActualEmailNotificationService _actualEmailNotificationService;
        private readonly IUserContextService _userContextService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public OriginalScenarioService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IActualRequestedInfoService actualRequestedInfoService,
            IActualWorkflowStepResponderService actualWorkflowStepResponderService,
            IActualWorkflowStepOptionService actualWorkflowStepOptionService,
            IActualEmailNotificationService actualEmailNotificationService,
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
            _userContextService = userContextService;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public async Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioDataViewModel = new ScenarioDataViewModel();
            var scenarioId = scenarioDetail.ScenarioId;

            var methods = await GetScenarioMethodsAsync(scenarioDetail);
            scenarioDataViewModel = await ExecuteScenarioMethodsAsync(methods, scenarioDetail);

            scenarioDataViewModel.ScenarioId = scenarioId;
            scenarioDataViewModel.IsOriginalData = true;

            return scenarioDataViewModel;
        }

        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<OriginalMethod> methods, ScenarioDetailsViewModel scenarioDetail)
        {
            var scenarioData = new ScenarioDataViewModel();
            var scenarioDataViewModel = new ScenarioDataViewModel();

            foreach (var method in methods)
            {
                scenarioDataViewModel = await ExecuteScenarioMethodAsync(method, scenarioDetail, scenarioData);
            }

            return scenarioDataViewModel;
        }

        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodAsync(OriginalMethod method, ScenarioDetailsViewModel scenarioDetail, ScenarioDataViewModel scenarioDataViewModel)
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

            if (serviceInstance == null) return scenarioDataViewModel;

            // Get method info
            MethodInfo methodInfo = serviceInstance.GetType().GetMethod(method.MethodName);
            if (methodInfo == null) return scenarioDataViewModel;

            object[] formattedParameters = method.Parameters?.ToArray() ?? new object[] { };

            object result = methodInfo.Invoke(serviceInstance, formattedParameters);
            if (result is Task taskResult) // If method returns a Task
            {
                await taskResult.ConfigureAwait(false); // Await task completion
                
                // If Task<T>, retrieve the actual result
                var resultProperty = taskResult.GetType().GetProperty("Result");
                result = resultProperty?.GetValue(taskResult);
            }

            // Ensure inner async methods are awaited properly
            if (result is Task innerTaskResult)
            {
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
                "IActualProvidedInfoService" => "ProvidedInfo",
                "IActualFileService" => "File",
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
                Data = result,
                IsOriginal = true
            });
        }

        private async Task<List<OriginalMethod>> GetScenarioMethodsAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var originalMethods = new List<OriginalMethod>();
            var scenarioId = scenarioDetail.ScenarioId;
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);
            var proposal = await _capitalRequestServices.GetProposal(scenarioDetail.ProposalId);
            var requestedInfo = await _capitalRequestServices.GetRequestedInfo(scenarioDetail.RequestedInfoId);

            //TODO Map
            proposal.ReviewerGroupId = detail.RequestingGroupId;
            proposal.ReviewerId = detail.ReviewerId;
            proposal.RequestedInfo = requestedInfo;
            proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                        .Where(x => !x.IsComplete)
                        .FirstOrDefault();

            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID))
                        .ToList();
            proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);

            if (scenarioId == "SCN001")
            {
                // For "Request More Information" scenario
                var optionType = Constants.OPTION_TYPE_VERIFY;
                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualRequestedInfoService",
                        MethodName = "GetRequestedInfoAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Select
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepResponderService",
                        MethodName = "GetWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REQUEST },
                        Operation = CrudOperationType.Select
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetClosedWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal, optionType },
                        Operation = CrudOperationType.Select
                    }
                );
            }
            else if (scenarioId == "SCN002")
            {
                // For "Reply to Request" scenario
                string fileName = null;
                var fileType = UploadFileType.Attachment;
                var requestedInfoId = detail.RequestedInfoId;
                var optionType = Constants.OPTION_TYPE_ADD_INFO;
                proposal.RequestedInfoId = proposal.RequestedInfo.Id;
                proposal.ReviewerGroupId = detail.ReplyingGroupId;
                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualProvidedInfoService",
                        MethodName = "GetProvidedInfoAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Select
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepResponderService",
                        MethodName = "GetWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REPLY, optionType },
                        Operation = CrudOperationType.Select
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetExpectedClosedWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal, optionType, requestedInfoId },
                        Operation = CrudOperationType.Select
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetExpectedReOpenedOptionsAsync",
                        Parameters = new List<object> { Constants.OPTION_TYPE_VERIFY, proposal },
                        Operation = CrudOperationType.Update
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualRequestedInfoService",
                        MethodName = "GetRequestedInfoByIdAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Select
                    }
                );
            }

            _mapper.Map(detail, scenarioDetail);
            return originalMethods;
        }
    }
}