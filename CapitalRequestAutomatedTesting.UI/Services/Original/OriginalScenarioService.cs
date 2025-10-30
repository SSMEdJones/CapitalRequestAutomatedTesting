using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using Infrastructure.ApiDiagnostics;
using Infrastructure.Utilities.Xml;
using System.Reflection;

namespace CapitalRequestAutomatedTesting.UI.Services.Original
{
    public interface IOriginalScenarioService
    {
        Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<OriginalMethod> methods, ScenarioDetailsViewModel scenarioDetail);
    }
    
    public class OriginalScenarioService : IOriginalScenarioService
    {
        private readonly ILogger<OriginalScenarioService> _logger;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IWorkflowControllerService _workflowControllerService;
        private readonly IActualRequestedInfoService _actualRequestedInfoService;
        private readonly IActualWorkflowStepResponderService _actualWorkflowStepResponderService;
        private readonly IActualWorkflowStepOptionService _actualWorkflowStepOptionService;
        private readonly IActualEmailNotificationService _actualEmailNotificationService;
        private readonly IUserContextService _userContextService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFormDataContext _formDataContext;
        private readonly IMapper _mapper;

        public OriginalScenarioService(ILogger<OriginalScenarioService> logger,
            ICapitalRequestServices capitalRequestServices,
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
            _logger = logger;
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
            try
            {
                // Set invocation context first
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

                if (serviceInstance == null) return scenarioDataViewModel;

                // Get method info
                MethodInfo methodInfo = serviceInstance.GetType().GetMethod(method.MethodName);
                if (methodInfo == null) 
                {
                    throw new InvalidOperationException($"Method '{method.MethodName}' not found on service '{method.ServiceName}'");
                }

                // Validate parameter count before invoking
                var expectedParamCount = methodInfo.GetParameters().Length;
                var actualParamCount = method.Parameters?.Count ?? 0;
                
                if (expectedParamCount != actualParamCount)
                {
                    throw new ArgumentException(
                        $"Parameter count mismatch for method '{method.MethodName}'. " + 
                        $"Expected: {expectedParamCount}, Actual: {actualParamCount}");
                }

                object[] formattedParameters = method.Parameters?.ToArray() ?? new object[] { };

                // Invoke with explicit try-catch
                object result;
                try
                {
                    result = methodInfo.Invoke(serviceInstance, formattedParameters);
                }
                catch (TargetInvocationException ex)
                {
                    // Unwrap the inner exception from reflection
                    throw ex.InnerException ?? ex;
                }

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
            catch (Exception ex)
            {
                // Log error with context before letting it propagate
                _logger.LogError(ex, "Error executing method {Method} on {Service} with parameters: {Parameters}", 
                    method.MethodName, 
                    method.ServiceName, 
                    string.Join(", ", method.Parameters ?? new List<object>()));
                
                // Re-throw to preserve stack trace
                throw;
            }
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


            //TODO Map
            proposal.RequestedInfoId = detail.RequestedInfoId;
            proposal.ReviewerGroupId = detail.TargetGroupId;
            proposal.ReviewerId = detail.ReviewerId;
            proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.RequestingGroupId = detail.RequestingGroupId;

            proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
            proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                        .Where(x => !x.IsComplete)
                        .FirstOrDefault();

            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID)).ToList();

            if (scenarioId == "SCN001")
            {
                // For "Request More Information" scenario
                var optionType = Constants.OPTION_TYPE_VERIFY;
                var isOpen = true;
                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualRequestedInfoService",
                        MethodName = "GetRequestedInfoAsync",
                        Parameters = new List<object> { proposal, isOpen },
                        Operation = CrudOperationType.Insert
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepResponderService",
                        MethodName = "GetWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REQUEST, optionType },
                        Operation = CrudOperationType.Insert
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetClosedWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal, optionType, null },
                        Operation = CrudOperationType.Update
                    }
                );
            }
            else if (scenarioId == "SCN002")
            {
                // For "Reply to Request" scenario
                //string fileName = null;
                //var fileType = UploadFileType.Attachment;
                var requestedInfoId = detail.RequestedInfoId;
                var requestedInfo = await _capitalRequestServices.GetRequestedInfo(scenarioDetail.RequestedInfoId);

                var optionType = Constants.OPTION_TYPE_ADD_INFO;
                proposal.ReviewerGroupId = detail.ReplyingGroupId;
                proposal.RequestedInfo = requestedInfo;
                proposal.RequestedInfoId = proposal.RequestedInfo.Id;


                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualProvidedInfoService",
                        MethodName = "GetProvidedInfoAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepResponderService",
                        MethodName = "GetWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REPLY, optionType },
                        Operation = CrudOperationType.Update
                    }
                );

                originalMethods.Add(
                    new OriginalMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetExpectedClosedWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal, optionType, requestedInfoId },
                        Operation = CrudOperationType.Update
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
                        Operation = CrudOperationType.Update
                    }
                );
            }

            var scenarioData = ModelConverter.ToDictionaryExcluding(scenarioDetail);
            var proposalData = ModelConverter.ToDictionaryExcluding(proposal);

            var scenarioLabeled = scenarioData.ToDictionary(kvp => $"Scenario.{kvp.Key}", kvp => kvp.Value);
            var proposalLabeled = proposalData.ToDictionary(kvp => $"Proposal.{kvp.Key}", kvp => kvp.Value);

            var combined = scenarioLabeled
                .Concat(proposalLabeled)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var formDataXml = FormDataXmlBuilder.Build(combined);
            _formDataContext.Set(formDataXml);

            _mapper.Map(detail, scenarioDetail);
            return originalMethods;
        }
    }
}