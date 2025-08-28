using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Infrastructure.ApiDiagnostics;
using Infrastructure.Utilities.Xml;
using SSMWorkflow.API.DataAccess.Models;
using System.Reflection;
using RequestedInfoSearchFilter = CapitalRequest.API.DataAccess.Models.RequestedInfoSearchFilter;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
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
        private readonly IFormDataContext _formDataContext;
        private readonly IMapper _mapper;

        public ActualScenarioService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService,
            IActualRequestedInfoService actualRequestedInfoService,
            IActualWorkflowStepResponderService actualWorkflowStepResponderService,
            IActualWorkflowStepOptionService actualWorkflowStepOptionService,
            IActualEmailNotificationService actualEmailNotificationService,
            IUserContextService userContextService,
            IServiceScopeFactory scopeFactory,
            IFormDataContext formDataContext,
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
            _formDataContext = formDataContext;
            _mapper = mapper;

        }

        public async Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail)
        {

            var scenarioDataViewModel = new ScenarioDataViewModel();
            var scenarioId = scenarioDetail.ScenarioId;

            //if (scenarioId == "SCN001")
            //{
            //    var methods = await GetScenarioMethodsAsync(scenarioDetail);
            //    scenarioDataViewModel = await ExecuteScenarioMethodsAsync(methods, scenarioDetail);
            //}

            var methods = await GetScenarioMethodsAsync(scenarioDetail);
            scenarioDataViewModel = await ExecuteScenarioMethodsAsync(methods, scenarioDetail);

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

            _formDataContext.SetInvocationContext(new MethodInvocationContext
            {
                ServiceName = method.ServiceName,
                MethodName = method.MethodName,
                Parameters = method.Parameters?.ToList() ?? new List<object>()
            });

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
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);
            var proposal = await _capitalRequestServices.GetProposal(scenarioDetail.ProposalId);


            proposal.ReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                        .Where(x => !x.IsComplete)
                        .FirstOrDefault();
            proposal.ReviewerId = detail.ReviewerId;
            proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);

            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID))
                        .ToList();


            if (scenarioId == "SCN001")
            {
                var optionType = Constants.OPTION_TYPE_VERIFY;

                var scenarioData = ModelConverter.ToDictionaryExcluding(scenarioDetail);
                var proposalData = ModelConverter.ToDictionaryExcluding(proposal);

                var scenarioLabeled = scenarioData.ToDictionary(kvp => $"Scenario.{kvp.Key}", kvp => kvp.Value);
                var proposalLabeled = proposalData.ToDictionary(kvp => $"Proposal.{kvp.Key}", kvp => kvp.Value);

                var combined = scenarioLabeled
                    .Concat(proposalLabeled)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var formDataXml = FormDataXmlBuilder.Build(combined);
                _formDataContext.Set(formDataXml);

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
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REQUEST, optionType },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetClosedWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal, optionType, null },
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
            else if (scenarioId == "SCN002")
            {
                string fileName = null;
                var fileType = UploadFileType.Attachment;
                var requestedInfoId = detail.RequestedInfoId;
                var optionType = Constants.OPTION_TYPE_ADD_INFO;

                var requestedInfo = await _capitalRequestServices.GetRequestedInfo(scenarioDetail.RequestedInfoId);
                proposal.RequestedInfo = requestedInfo;
                proposal.RequestedInfoId = proposal.RequestedInfo.Id;
                proposal.ReviewerGroupId = detail.ReplyingGroupId;

                var scenarioData = ModelConverter.ToDictionaryExcluding(scenarioDetail);
                var proposalData = ModelConverter.ToDictionaryExcluding(proposal);

                var scenarioLabeled = scenarioData.ToDictionary(kvp => $"Scenario.{kvp.Key}", kvp => kvp.Value);
                var proposalLabeled = proposalData.ToDictionary(kvp => $"Proposal.{kvp.Key}", kvp => kvp.Value);

                var combined = scenarioLabeled
                    .Concat(proposalLabeled)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var formDataXml = FormDataXmlBuilder.Build(combined);
                _formDataContext.Set(formDataXml);

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualProvidedInfoService",
                        MethodName = "GetProvidedInfoAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualFileService",
                        MethodName = "DownloadFile",
                        Parameters = new List<object> { fileName, proposal.Id, fileType },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepResponderService",
                        MethodName = "GetWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REPLY, optionType },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetClosedWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal, optionType, requestedInfoId },
                        Operation = CrudOperationType.Update
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetReOpenedOptionsAsync",
                        Parameters = new List<object> {Constants.OPTION_TYPE_VERIFY, proposal },
                        Operation = CrudOperationType.Update
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualEmailNotificationService",
                        MethodName = "GetEmailNotificationsAsync",
                        Parameters = new List<object> { proposal, Constants.EMAIL_PROVIDE_MORE_INFORMATION },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualRequestedInfoService",
                        MethodName = "GetRequestedInfoAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Update
                    }
                );

            }

            _mapper.Map(detail, scenarioDetail); 

            return actualMethods;
        }
       
    }
}
