using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Infrastructure.ApiDiagnostics;
using Infrastructure.Utilities.Xml;
using SSMWorkflow.API.DataAccess.Models;
using System.Diagnostics;
using System.Reflection;
using RequestedInfoSearchFilter = CapitalRequest.API.DataAccess.Models.RequestedInfoSearchFilter;

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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFormDataContext _formDataContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IActualReviewerGroupService _actualReviewerGroupService;
        private readonly IActualWorkflowStepService _actualWorkflowStepService;
        private readonly IMapper _mapper;

        public ActualScenarioService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IServiceScopeFactory scopeFactory,
            IFormDataContext formDataContext,
            IWebHostEnvironment environment,
            IActualReviewerGroupService actualReviewerGroupService,
            IActualWorkflowStepService actualWorkflowStepService,
            IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _scopeFactory = scopeFactory;
            _formDataContext = formDataContext;
            _environment = environment;
            _actualReviewerGroupService = actualReviewerGroupService;
            _actualWorkflowStepService = actualWorkflowStepService;
            _mapper = mapper;

        }

        public async Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            if (scenarioDetail.StopWatch != null)
            {
                scenarioDetail.StopWatch.Stop();

                scenarioDetail.ExecutionDuration = scenarioDetail.StopWatch.Elapsed;
                scenarioDetail.ExecutionDurationMinutes = (int)Math.Ceiling(scenarioDetail.StopWatch.Elapsed.TotalMinutes);
            }

            var scenarioDataViewModel = new ScenarioDataViewModel();
            var scenarioId = scenarioDetail.ScenarioId;
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

            // Update predictive data with actual workflow values after all methods are executed
            await UpdatePredictiveDataAsync(scenarioDetail, scenarioDataViewModel);

            return scenarioDataViewModel;
        }

        /// <summary>
        /// Updates predictive data structures with actual workflow values.
        /// This ensures predictive data matches actual execution state regardless of email notifications.
        /// </summary>
        private async Task UpdatePredictiveDataAsync(ScenarioDetailsViewModel scenarioDetail, ScenarioDataViewModel scenarioDataViewModel)
        {
            if (scenarioDetail?.PredictiveData?.Tables == null)
                return;

            var proposal = await _capitalRequestServices.GetProposal(scenarioDetail.ProposalId);
            proposal.VerifyingGroupId = scenarioDetail.VerifyingGroupId.Value;

            // Ensure we have the current workflow step
            if (proposal.WorkflowId != null && proposal.WorkflowId != Guid.Empty)
            {
                proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();
            }

            var workflowID = proposal.WorkflowId;
            var workflowStepId = proposal.WorkflowStep?.WorkflowStepID ?? Guid.Empty;
            var workflowStep = proposal.WorkflowStep;

            // Get email query from scenario data if available
            var emailQuery = GetEmailQueryFromScenarioData(scenarioDataViewModel);

            var predictiveData = scenarioDetail.PredictiveData;
            var tables = predictiveData.Tables;

            // Update EmailNotification table
            if (tables.TryGetValue("EmailNotification", out var emailTable))
            {
                var recordEntry = emailTable.Records.FirstOrDefault();
                if (recordEntry?.Data is List<EmailNotification> predictiveNotifications)
                {
                    foreach (var email in predictiveNotifications)
                    {
                        email.EmailQuery = emailQuery;
                        email.WorkflowStepId = workflowStepId;
                    }
                }
            }

            // Update WorkflowStep table
            if (tables.TryGetValue("WorkflowStep", out var stepTable))
            {
                var recordEntry = stepTable.Records.FirstOrDefault();
                if (recordEntry?.Data is WorkflowStep predictiveStep)
                {
                    predictiveStep.WorkflowID = workflowID;
                }
            }

            var workflowStepOption = (await _ssmWorkflowServices.GetAllAddWorkFlowStepResponder(workflowStepId))
                .OrderByDescending(x => x.Created)
                .FirstOrDefault();

            if (workflowStepOption == null)
            {
                var verifyingGroup = await _capitalRequestServices.GetReviewerGroup(proposal.VerifyingGroupId);

                var currentStepNumber = verifyingGroup.StepNumber;
                var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter());
                var workflowTemplate = workflowTemplates.FirstOrDefault(x => x.StepNumber == currentStepNumber);

                var workflowStepID = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                    .FirstOrDefault(x => x.StepName == workflowTemplate.StepName)?.WorkflowStepID;

                workflowStepOption = (await _ssmWorkflowServices.GetAllAddWorkFlowStepResponder(workflowStepID.Value))
                 .OrderByDescending(x => x.Created)
                 .FirstOrDefault();

            }

            if (workflowStepOption != null)
            {
                var workflowStepOptionID = workflowStepOption.WorkflowStepOptionID;

                if (tables.TryGetValue("WorkflowStepResponder", out var responderTable))
                {
                    var recordEntry = responderTable.Records.FirstOrDefault();
                    if (recordEntry?.Data is WorkflowStepResponder workflowStepResponder)
                    {
                        if (workflowStepResponder.WorkflowStepID == Guid.Empty)
                            workflowStepResponder.WorkflowStepID = workflowStepOption.WorkflowStepID; ;

                        if (workflowStepResponder.WorkflowStepOptionID == Guid.Empty)
                            workflowStepResponder.WorkflowStepOptionID = workflowStepOption.WorkflowStepOptionID;
                    }
                }
            }

            // Update WorkflowStepOption table
            if (tables.TryGetValue("WorkflowStepOption", out var optionTable))
            {
                foreach (var recordEntry in optionTable.Records)
                {
                    if (recordEntry?.Data is List<WorkflowStepOption> predictiveOptions)
                    {
                        foreach (var option in predictiveOptions)
                        {
                            if (option.WorkflowStepID == Guid.Empty)
                            {
                                option.WorkflowStepID = workflowStepId;
                            }
                        }
                    }
                }
            }

            // Update WorkflowInstance table
            if (tables.TryGetValue("WorkflowInstance", out var instanceTable))
            {
                var recordEntry = instanceTable.Records.FirstOrDefault();
                if (recordEntry?.Data is WorkflowInstance predictiveInstance)
                {
                    predictiveInstance.WorkflowID = workflowID;
                    predictiveInstance.CurrentWorkflowStepID = workflowStepId;
                    predictiveInstance.CurrentWorkflowState = workflowStep?.StepName;
                }
            }

            // Update WorkflowStakeHolder table
            if (tables.TryGetValue("WorkflowStakeHolder", out var holderTable))
            {
                var recordEntry = holderTable.Records.FirstOrDefault();
                if (recordEntry?.Data is List<WorkflowStakeholder> predictiveHolders)
                {
                    foreach (var holder in predictiveHolders)
                    {
                        holder.WorkflowID = workflowID;
                    }
                }
            }

            // Update WorkflowInstanceActionHistory table
            if (tables.TryGetValue("WorkflowInstanceActionHistory", out var historyTable))
            {
                foreach (var recordEntry in historyTable.Records)
                {
                    if (recordEntry?.Data is List<WorkflowInstanceActionHistory> predictiveHistory)
                    {
                        if (scenarioDataViewModel.Tables.TryGetValue("WorkflowInstanceActionHistory", out var actualTable))
                        {
                            // Store the record count from predictive (outer loop)
                            var predictiveRecordCount = predictiveHistory.Count;

                            Debug.WriteLine($"[WorkflowInstanceActionHistory] Processing predictive record with {predictiveRecordCount} items");

                            var actualEntry = actualTable.Records.FirstOrDefault();
                            if (actualEntry?.Data is List<WorkflowInstanceActionHistory> actualHistory)
                            {
                                // Find the matching actual entry by record count
                                var matchingActualEntry = actualTable.Records
                                    .FirstOrDefault(entry => entry?.Data is List<WorkflowInstanceActionHistory> list &&
                                                           list.Count == predictiveRecordCount);

                                if (matchingActualEntry?.Data is List<WorkflowInstanceActionHistory> matchingActualHistory)
                                {
                                    Debug.WriteLine($"[WorkflowInstanceActionHistory] Found matching actual record with {matchingActualHistory.Count} items");

                                    // Now align records by index within the matching count group
                                    for (int i = 0; i < predictiveRecordCount; i++)
                                    {
                                        var predictiveRecord = predictiveHistory[i];
                                        var actualRecord = matchingActualHistory[i]; // This uses the matching record count group

                                        Debug.WriteLine($"Aligning record {i} within {predictiveRecordCount}-item group");

                                        // Update predictive record with corresponding actual values
                                        if (predictiveRecord.WorkflowStepID == Guid.Empty)
                                            predictiveRecord.WorkflowStepID = actualRecord.WorkflowStepID;

                                        if (predictiveRecord.WorkflowInstanceID == Guid.Empty)
                                            predictiveRecord.WorkflowInstanceID = actualRecord.WorkflowInstanceID;

                                    }
                                }
                            }
                        }
                    }
                }
            }
            //if (tables.TryGetValue("WorkflowInstanceActionHistory", out var historyTable))
            //{
            //    foreach (var recordEntry in historyTable.Records)
            //    {

            //        if (recordEntry?.Data is List<WorkflowInstanceActionHistory> predictiveHistory)
            //        {
            //            if (scenarioDataViewModel.Tables.TryGetValue("WorkflowInstanceActionHistory", out var actualTable))
            //            {
            //                var actualEntry = actualTable.Records.FirstOrDefault();
            //                if (actualEntry?.Data is List<WorkflowInstanceActionHistory> actualHistory)
            //                {
            //                    // Get the first actual history item to use for backfilling
            //                    var firstActualItem = actualHistory.FirstOrDefault();

            //                    if (firstActualItem != null)
            //                    {
            //                        // Update each predictive item with values from the first actual item
            //                        foreach (var history in predictiveHistory)
            //                        {
            //                            if (history.WorkflowStepID == Guid.Empty)
            //                                history.WorkflowStepID = firstActualItem.WorkflowStepID;

            //                            if (history.WorkflowInstanceID == Guid.Empty)
            //                                history.WorkflowInstanceID = firstActualItem.WorkflowInstanceID;

            //                        }
            //                    }
            //                }

            //            }

            //        }
            //    }
            //}
        }

        /// <summary>
        /// Extracts email query from scenario data for predictive data updates
        /// </summary>
        private string GetEmailQueryFromScenarioData(ScenarioDataViewModel scenarioDataViewModel)
        {
            if (scenarioDataViewModel?.Tables?.TryGetValue("EmailNotification", out var emailTable) == true)
            {
                var recordEntry = emailTable.Records.FirstOrDefault();
                if (recordEntry?.Data is List<SSMWorkflow.API.Models.EmailNotification> notifications)
                {
                    return notifications.FirstOrDefault()?.EmailQuery ?? string.Empty;
                }
                else if (recordEntry?.Data is SSMWorkflow.API.Models.EmailNotification notification)
                {
                    return notification.EmailQuery ?? string.Empty;
                }
            }

            return string.Empty;
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
            else if (serviceName == $"{nameSpace}IActualProvidedInfoService")
                serviceType = typeof(IActualProvidedInfoService);
            else if (serviceName == $"{nameSpace}IActualWorkflowStepResponderService")
                serviceType = typeof(IActualWorkflowStepResponderService);
            else if (serviceName == $"{nameSpace}IActualWorkflowStepOptionService")
                serviceType = typeof(IActualWorkflowStepOptionService);
            else if (serviceName == $"{nameSpace}IActualEmailNotificationService")
                serviceType = typeof(IActualEmailNotificationService);
            else if (serviceName == $"{nameSpace}IActualAttachmentService")
                serviceType = typeof(IActualAttachmentService);
            else if (serviceName == $"{nameSpace}IActualWorkflowService")
                serviceType = typeof(IActualWorkflowService);
            else if (serviceName == $"{nameSpace}IActualWorkflowStepService")
                serviceType = typeof(IActualWorkflowStepService);
            else if (serviceName == $"{nameSpace}IActualWorkflowInstanceService")
                serviceType = typeof(IActualWorkflowInstanceService);
            else if (serviceName == $"{nameSpace}IActualWorkflowInstanceHistoryService")
                serviceType = typeof(IActualWorkflowInstanceHistoryService);
            else if (serviceName == $"{nameSpace}IActualWorkflowStakeHolderService")
                serviceType = typeof(IActualWorkflowStakeHolderService);
            else if (serviceName == $"{nameSpace}IActualWbsService")
                serviceType = typeof(IActualWbsService);

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
                "IActualProvidedInfoService" => "ProvidedInfo",
                "IActualWorkflowStepResponderService" => "WorkflowStepResponder",
                "IActualWorkflowStepOptionService" => "WorkflowStepOption",
                "IActualEmailNotificationService" => "EmailNotification",
                "IActualAttachmentService" => "Attachment",
                "IActualWorkflowService" => "Workflow",
                "IActualWorkflowStepService" => "WorkflowStep",
                "IActualWorkflowInstanceService" => "WorkflowInstance",
                "IActualWorkflowInstanceHistoryService" => "WorkflowInstanceActionHistory",
                "IActualWorkflowStakeHolderService" => "WorkflowStakeHolder",
                "IActualWbsService" => "Wbs",
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

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => !x.IsComplete)
                .FirstOrDefault();

            proposal.WorkflowStepId = proposal.WorkflowStep.WorkflowStepID;

            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID)).ToList();

            if (detail.ScenarioId != "SCN004")
            {
                proposal.ReviewerId = detail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);
            }

            proposal.ExecutionDurationMinutes = scenarioDetail.ExecutionDurationMinutes;

            // Only add 5 minutes in development mode 
            if (_environment.IsDevelopment())
            {
                //allows for debugging time
                proposal.ExecutionDurationMinutes += 5;
            }

            if (scenarioId == "SCN001")
            {
                var isOpen = true;

                var optionType = Constants.OPTION_TYPE_VERIFY;

                proposal.ReviewerGroupId = detail.TargetGroupId;
                proposal.RequestingGroupId = detail.RequestingGroupId;

                var filter = new RequestedInfoSearchFilter
                {
                    ProposalId = proposal.Id,
                    RequestingReviewerGroupId = proposal.RequestingGroupId,
                    ReviewerGroupId = proposal.ReviewerGroupId,
                    IsOpen = isOpen
                };

                proposal.RequestedInfo = (await _capitalRequestServices
                    .GetAllRequestedInfos(filter))
                    .FirstOrDefault();

                detail.RequestedInfoId = proposal.RequestedInfo.Id;

                var requestingUser = (await _capitalRequestServices.GetReviewer(proposal.RequestedInfo.RequestingReviewerId)).FullName;

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualRequestedInfoService",
                        MethodName = "GetRequestedInfoAsync",
                        Parameters = new List<object> { proposal, isOpen },
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
                        Operation = CrudOperationType.Update
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualEmailNotificationService",
                        MethodName = "GetEmailNotificationsAsync",
                        Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION, requestingUser },
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

                proposal.RequestedInfo = await _capitalRequestServices.GetRequestedInfo(detail.RequestedInfoId);
                proposal.RequestedInfoId = proposal.RequestedInfo.Id;
                proposal.ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId;
                proposal.RequestingGroupId = proposal.RequestedInfo.RequestingReviewerGroupId;

                var requestingUser = (await _capitalRequestServices.GetReviewer(proposal.RequestedInfo.RequestingReviewerId)).FullName;

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
                        ServiceName = "IActualAttachmentService",
                        MethodName = "GetAttachments",
                        Parameters = new List<object> { proposal },
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
                        Parameters = new List<object> { Constants.OPTION_TYPE_VERIFY, proposal },
                        Operation = CrudOperationType.Update
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualEmailNotificationService",
                        MethodName = "GetEmailNotificationsAsync",
                        Parameters = new List<object> { proposal, Constants.EMAIL_PROVIDE_MORE_INFORMATION, requestingUser },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualRequestedInfoService",
                        MethodName = "GetRequestedInfoByIdAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Update
                    }
                );

            }
            else if (scenarioId == "SCN003")
            {
                proposal.VerifyAndSendToVPFinance = detail.VerifyAndSendToVPFinance;
                proposal.VerifyingGroupId = detail.VerifyingGroupId;
                var optionType = Constants.OPTION_TYPE_VERIFY;
                var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(proposal.VerifyingGroupId);

                var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter());

                var workflowTemplate = workflowTemplates.FirstOrDefault(x => x.StepNumber == reviewerGroup.StepNumber);

                proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                    .Where(x => x.StepName == workflowTemplate.StepName)
                    .FirstOrDefault();

                proposal.WorkflowStepId = proposal.WorkflowStep.WorkflowStepID;
                proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID)).ToList();


                proposal.ReviewerGroupName = reviewerGroup.Name;
                var workflowStep = proposal.WorkflowStep;
                var currentStepNumber = workflowTemplates
                    .Where(x => x.StepName == workflowStep.StepName)
                    .First()
                    .StepNumber;

                var nextStepNumber = currentStepNumber + 1;
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
                        ServiceName = "IActualWorkflowStepResponderService",
                        MethodName = "GetWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, optionType, optionType },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowInstanceHistoryService",
                        MethodName = "GetWorkflowInstanceHistoryAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                if (await _actualWorkflowStepService.AllGroupsVerifiedAsync(proposal))
                {
                    actualMethods.Add(
                        new ActualMethod
                        {
                            ServiceName = "IActualWorkflowStepService",
                            MethodName = "GetMarkStepCompleteAsync",
                            Parameters = new List<object> { proposal },
                            Operation = CrudOperationType.Update
                        }
                    );

                    if (!await _actualWorkflowStepService.AllStepsCompleteAsync(proposal))
                    {
                        var createStep = true;

                        while (createStep)
                        {
                            var reviewerGroups = await _actualReviewerGroupService.GetFilteredReviewerGroupsAsync(nextStepNumber);
                            var filteredReviewerGroups = _actualReviewerGroupService.FilterReviewerGroups(reviewerGroups, proposal, nextStepNumber);

                            proposal.ReviewerGroups = filteredReviewerGroups;

                            workflowTemplate = workflowTemplates
                                .Where(x => x.StepNumber == nextStepNumber)
                                .FirstOrDefault();

                            if (!proposal.VerifyAndSendToVPFinance)
                            {
                                if (workflowTemplate.Conditional != null && workflowTemplate.Conditional.IndexOf(".") > 0)
                                {
                                    var tableAndColumn = workflowTemplate.Conditional.Split(".");
                                    if (tableAndColumn.Length > 0)
                                    {
                                        createStep = (bool)proposal.GetType().GetProperty(tableAndColumn[1]).GetValue(proposal, null);
                                    }
                                }
                            }

                            if (createStep)
                            {
                                CreateUpdateWorkFlowStep createWorkflowStep;

                                proposal.WorkflowInstanceId = Guid.NewGuid();
                                proposal.NextWorkflowStepId = Guid.NewGuid();
                                proposal.NextStepName = workflowTemplates.FirstOrDefault(x => x.StepNumber == nextStepNumber).StepName;

                                actualMethods.Add(
                                    new ActualMethod
                                    {
                                        ServiceName = "IActualWorkflowStepService",
                                        MethodName = "GetNextStepCreatedAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                actualMethods.Add(
                                   new ActualMethod
                                   {
                                       ServiceName = "IActualWorkflowInstanceService",
                                       MethodName = "GetNextStepWorkflowInstanceAsync",
                                       Parameters = new List<object> { proposal },
                                       Operation = CrudOperationType.Insert
                                   }
                               );
                                actualMethods.Add(
                                    new ActualMethod
                                    {
                                        ServiceName = "IActualWorkflowStakeHolderService",
                                        MethodName = "GetNextStepWorkflowStakeHoldersAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                actualMethods.Add(
                                    new ActualMethod
                                    {
                                        ServiceName = "IActualWorkflowStepOptionService",
                                        MethodName = "GetNextStepWorkflowStepOptionsAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );
                                actualMethods.Add(
                                    new ActualMethod
                                    {
                                        ServiceName = "IActualWorkflowInstanceHistoryService",
                                        MethodName = "GetNextStepWorkflowInstanceHistoryAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                actualMethods.Add(
                                    new ActualMethod
                                    {
                                        ServiceName = "IActualEmailNotificationService",
                                        MethodName = "GetNextStepEmailNotificationsAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                if (!string.IsNullOrWhiteSpace(workflowTemplate.AdditionalTask))
                                {
                                    if (workflowTemplate.AdditionalTask == Constants.ADDITIONAL_TASK_CREATE_WBS_NUMBERS)
                                    {

                                        actualMethods.Add(
                                            new ActualMethod
                                            {
                                                ServiceName = "IActualWbsService",
                                                MethodName = "GetWBSNumbersAsync",
                                                Parameters = new List<object> { proposal },
                                                Operation = CrudOperationType.Update
                                            }
                                        );

                                    }


                                    break;
                                }
                                break;
                            }
                            else
                            {
                                nextStepNumber++;
                            }

                        }
                    }

                }
            }
            else if (scenarioId == "SCN004")
            {
                var reviewerGroups = await _actualReviewerGroupService.GetFilteredReviewerGroupsAsync(Constants.STEP_ONE);
                var filteredReviewerGroups = _actualReviewerGroupService.FilterReviewerGroups(reviewerGroups, proposal, Constants.STEP_ONE);
                proposal.ReviewerGroups = filteredReviewerGroups;

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowService",
                        MethodName = "GetWorkflowAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepService",
                        MethodName = "GetWorkflowStepAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowInstanceService",
                        MethodName = "GetWorkflowInstanceAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStakeHolderService",
                        MethodName = "GetWorkflowStakeHoldersAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualWorkflowStepOptionService",
                        MethodName = "GetWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                actualMethods.Add(
                    new ActualMethod
                    {
                        ServiceName = "IActualEmailNotificationService",
                        MethodName = "GetSubmitEmailNotificationsAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

            }
            else if (scenarioId == "SCN005")
            {

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

            return actualMethods;
        }

    }
}
