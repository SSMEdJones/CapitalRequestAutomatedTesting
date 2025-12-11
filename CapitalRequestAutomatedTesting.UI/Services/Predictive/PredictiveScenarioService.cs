using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using Infrastructure.ApiDiagnostics;
using Infrastructure.Utilities.Xml;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
using System.Reflection;
using Constants = CapitalRequestAutomatedTesting.UI.Models.Constants;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveScenarioService
    {
        Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail);
        Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail);
    }
    public class PredictiveScenarioService : IPredictiveScenarioService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFormDataContext _formDataContext;
        private readonly IActualReviewerGroupService _actualReviewerGroupService;
        private readonly IPredictiveWorkflowStepService _predictiveWorkflowStepService;
        private readonly IScenarioControllerService _scenarioControllerService;
        private readonly IMapper _mapper;

        public PredictiveScenarioService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IServiceScopeFactory scopeFactory,
            IFormDataContext formDataContext,
            IActualReviewerGroupService actualReviewerGroupService,
            IPredictiveWorkflowStepService predictiveWorkflowStepService,
            IScenarioControllerService scenarioControllerService,
            IMapper mapper)

        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _scopeFactory = scopeFactory;
            _formDataContext = formDataContext;
            _actualReviewerGroupService = actualReviewerGroupService;
            _predictiveWorkflowStepService = predictiveWorkflowStepService;
            _scenarioControllerService = scenarioControllerService;
            _mapper = mapper;
        }

        public async Task<ScenarioDataViewModel> GenerateScenarioDataAsync(ScenarioDetailsViewModel scenarioDetail)
        {

            var scenarioDataViewModel = new ScenarioDataViewModel();
            var scenarioId = scenarioDetail.ScenarioId;
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);

            var proposal = await _capitalRequestServices.GetProposal(detail.ProposalId);

            if (detail.ScenarioId != "SCN003" && detail.ScenarioId != "SCN004")
            {
                var requestingGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
                var reviewer = await _capitalRequestServices.GetReviewer(detail.ReviewerId);

                proposal.RequestingGroupId = detail.RequestingGroupId;
                proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
                proposal.RequestedInfo.RequestedInformation = detail.RequestedInformation;
                proposal.ReviewerId = detail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);

                scenarioDetail.SelectedProperties["Requesting Group"] = requestingGroup.Name;
                scenarioDetail.SelectedProperties["Reviewer"] = reviewer.FullName;
                scenarioDetail.SelectedProperties["Requested Information"] = detail.RequestedInformation;
            }


            scenarioDetail.SelectedProperties["Scenario Name"] = detail.DisplayText;
            scenarioDetail.SelectedProperties["Req Id"] = detail.ProposalId.ToString();

            if (scenarioId == "SCN001")
            {
                var targetGroup = await _capitalRequestServices.GetReviewerGroup(detail.TargetGroupId);

                proposal.RequestedInfo.ReviewerGroupId = detail.TargetGroupId;
                proposal.ReviewerGroupName = targetGroup.Name;

                scenarioDetail.SelectedProperties["Target Group"] = targetGroup.Name;

            }
            else if (scenarioId == "SCN002")
            {
                var replyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);
                proposal.ReviewerGroupName = replyingGroup.Name;

                scenarioDetail.SelectedProperties["Replying Group"] = replyingGroup.Name;

            }
            else if (scenarioId == "SCN003")
            {
                var verifiedGroup = await _capitalRequestServices.GetReviewerGroup(detail.VerifyingGroupId);
                proposal.ReviewerId = detail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);

                scenarioDetail.SelectedProperties["Verified Group"] = verifiedGroup.Name;
                scenarioDetail.SelectedProperties["Verified By"] = proposal.Reviewer.FullName;
                if (scenarioDetail.IsVpOfOps)
                {
                    scenarioDetail.SelectedProperties["Verify And Send To VPFinance"] = $"{scenarioDetail.VerifyAndSendToVPFinance}";
                }


            }
            else if (scenarioId == "SCN004")
            {
                if (!string.IsNullOrWhiteSpace(detail.SubmitUserId))
                {
                    detail.SubmittedBy = (await _scenarioControllerService.GetSubmitUsersAsync(detail.ProposalId))
                                    .FirstOrDefault(u => u.Value == detail.SubmitUserId).Text;

                    if (!string.IsNullOrWhiteSpace(detail.SubmittedBy))
                    {
                        scenarioDetail.SelectedProperties["Submitted By"] = detail.SubmittedBy;
                    }

                }
            }


            var methods = await GetScenarioMethodsAsync(scenarioDetail);

            //scenarioDetail.PredictiveMethods = methods;
            scenarioDataViewModel = await ExecuteScenarioMethodsAsync(methods, scenarioDetail);
            scenarioDataViewModel.ScenarioId = scenarioId;
            scenarioDataViewModel.PredictiveMethods = methods;

            return scenarioDataViewModel;
        }

        public async Task<ScenarioDataViewModel> ExecuteScenarioMethodsAsync(List<PredictiveMethod> methods, ScenarioDetailsViewModel scenarioDetail)
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
            if (serviceName == $"{nameSpace}IPredictiveRequestedInfoService")
                serviceType = typeof(IPredictiveRequestedInfoService);
            else if (serviceName == $"{nameSpace}IPredictiveProvidedInfoService")
                serviceType = typeof(IPredictiveProvidedInfoService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStepResponderService")
                serviceType = typeof(IPredictiveWorkflowStepResponderService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStepOptionService")
                serviceType = typeof(IPredictiveWorkflowStepOptionService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStepService")
                serviceType = typeof(IPredictiveWorkflowStepService);
            else if (serviceName == $"{nameSpace}IPredictiveEmailNotificationService")
                serviceType = typeof(IPredictiveEmailNotificationService);
            else if (serviceName == $"{nameSpace}IPredictiveAttachmentService")
                serviceType = typeof(IPredictiveAttachmentService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowService")
                serviceType = typeof(IPredictiveWorkflowService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowInstanceService")
                serviceType = typeof(IPredictiveWorkflowInstanceService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowStakeHolderService")
                serviceType = typeof(IPredictiveWorkflowStakeHolderService);
            else if (serviceName == $"{nameSpace}IPredictiveWorkflowInstanceHistoryService")
                serviceType = typeof(IPredictiveWorkflowInstanceHistoryService);
            else if (serviceName == $"{nameSpace}IPredictiveWbsService")
                serviceType = typeof(IPredictiveWbsService);

            if (serviceType == null)
            {
                Debug.WriteLine($"Service type for {method.ServiceName} not found.");
                return scenarioDataViewModel;
            }


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
            var exectutionContext = new PredictiveExecutionContext
            {
                StepNumber = method.StepNumber,
                ServiceName = method.ServiceName,
                MethodName = method.MethodName,
                OperationType = method.Operation,
                Result = result
            };

            MapResultsToTables(scenarioDataViewModel, exectutionContext);

            return scenarioDataViewModel;
        }

        private string DetermineTableName(string serviceName)
        {

            return serviceName switch
            {
                "IPredictiveRequestedInfoService" => "RequestedInfo",
                "IPredictiveProvidedInfoService" => "ProvidedInfo",
                "IPredictiveWorkflowStepResponderService" => "WorkflowStepResponder",
                "IPredictiveWorkflowStepOptionService" => "WorkflowStepOption",
                "IPredictiveEmailNotificationService" => "EmailNotification",
                "IPredictiveAttachmentService" => "Attachment",
                "IPredictiveWorkflowService" => "Workflow",
                "IPredictiveWorkflowStepService" => "WorkflowStep",
                "IPredictiveWorkflowInstanceService" => "WorkflowInstance",
                "IPredictiveWorkflowInstanceHistoryService" => "WorkflowInstanceActionHistory",
                "IPredictiveWorkflowStakeHolderService" => "WorkflowStakeHolder",
                "IPredictiveWbsService" => "Wbs",
                _ => "UnknownService"
            };
        }

        private void MapResultsToTables(ScenarioDataViewModel scenarioDataViewModel, PredictiveExecutionContext context)
        {
            var tableName = DetermineTableName(context.ServiceName);

            if (string.IsNullOrEmpty(tableName))
            {
                Debug.WriteLine($"Service Name : {context.ServiceName}");
                return;
            }

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
                Operation = context.OperationType,
                Data = context.Result
            });
        }

        private async Task<List<PredictiveMethod>> GetScenarioMethodsAsync(ScenarioDetailsViewModel scenarioDetail)
        {
            var predictiveMethods = new List<PredictiveMethod>();
            var scenarioId = scenarioDetail.ScenarioId;
            var detail = _mapper.Map<ScenarioDetails>(scenarioDetail);

            var proposal = await _capitalRequestServices.GetProposal(detail.ProposalId);

            if (detail.ReviewerId != 0)
            {
                proposal.ReviewerId = detail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);
            }

            if (detail.ScenarioId != "SCN004")
            {

                var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();


                var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                   .Where(x => !x.IsComplete && !x.IsTerminate)
                   .ToList();

                // simulate AddWorkflowStepOption
                if (!workflowStepOptions.Any(x => x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
                {
                    var newWorkflowStepOption = _mapper.Map<WorkflowStepOption>(proposal.Reviewer);
                    newWorkflowStepOption.OptionType = workflowStepOptions.FirstOrDefault().OptionType;

                    workflowStepOptions.Add(_mapper.Map<WorkFlowStepOptionViewModel>(newWorkflowStepOption));
                }

                proposal.WorkflowStepOptions = workflowStepOptions;

                proposal.WorkflowStepId = workflowStep.WorkflowStepID;
                proposal.WorkflowStep = await _ssmWorkflowServices.GetWorkflowStep(workflowStep.WorkflowStepID);

                proposal.VerifyingGroupId = detail.VerifyingGroupId;

                if (detail.ScenarioId != "SCN003")
                {

                    var requestingGroupId = detail.RequestingGroupId;
                    var replyingGroupId = detail.ReplyingGroupId;

                    proposal.ReviewerGroupId = detail.RequestingGroupId;
                    proposal.ReplyingGroupId = detail.ReplyingGroupId;
                    proposal.RequestingGroupId = detail.RequestingGroupId;
                    proposal.RequestingGroup = await _capitalRequestServices.GetReviewerGroup(detail.RequestingGroupId);
                    proposal.RequestedInfo.RequestingReviewerGroupId = detail.RequestingGroupId;
                    proposal.RequestedInfo.RequestedInformation = detail.RequestedInformation;
                    proposal.ReviewerId = detail.ReviewerId;
                    proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);
                }


            }

            var increment = 1;

            if (scenarioId == "SCN001")
            {
                //proposal.ReviewerGroupId = detail.RequestingGroupId;
                proposal.RequestedInfo.ReviewerGroupId = detail.TargetGroupId;
                proposal.RequestedInfo.Id = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter())).Max(x => x.Id);

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveRequestedInfoService",
                        MethodName = "CreateRequestedInfoAsync",
                        Parameters = new List<object> { proposal, increment },
                        Operation = CrudOperationType.Insert
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepResponderService",
                        MethodName = "CreateWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REQUEST },
                        Operation = CrudOperationType.Insert
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "CloseOptionsAsync",
                        Parameters = new List<object> { proposal, Guid.Empty, Constants.OPTION_TYPE_VERIFY, null },
                        Operation = CrudOperationType.Update
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "CreateWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION, proposal.RequestedInfo.Id },
                        Operation = CrudOperationType.Insert
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveEmailNotificationService",
                        MethodName = "CreateEmailNotificationsAsync",
                        Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION, null },
                        Operation = CrudOperationType.Insert
                    }
                );

            }
            else if (scenarioId == "SCN002")
            {
                proposal.ReviewerGroupId = detail.ReplyingGroupId;
                proposal.ReplyingGroup = await _capitalRequestServices.GetReviewerGroup(detail.ReplyingGroupId);
                var workflowStepId = proposal.WorkflowStep.WorkflowStepID;
                var filter = new RequestedInfoSearchFilter
                {
                    ProposalId = detail.ProposalId,
                    ReviewerGroupId = detail.ReplyingGroupId,
                    RequestingReviewerGroupId = detail.RequestingGroupId,
                    IsOpen = true
                };
                proposal.RequestedInfo = (await _capitalRequestServices.GetAllRequestedInfos(filter)).FirstOrDefault();

                var requestingUser = (await _capitalRequestServices.GetReviewer(proposal.RequestedInfo.RequestingReviewerId)).FullName;

                proposal.ActionType = Constants.ACTION_TYPE_ADD_INFO;
                proposal.ButtonCaption = Constants.BUTTON_CAPTION_REPLY;
                proposal.ExpectedMessage = Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT;

                proposal.AddInfoFiles = detail.AddInfoFiles;
                var actionType = proposal.ActionType;
                var lookupKey = Constants.UPLOAD_DIRECTORY_ATTACHMENTS;

                List<FileUploadData> files = detail.FileUploads;

                if (files.Any())
                {
                    proposal.Attachment = new CapitalRequest.API.Models.Attachment();
                }
                else
                {
                    proposal.Attachment = null;
                }

                // mapping fields
                proposal.ProvidedInfo.RequestedInfoId = proposal.RequestedInfo.Id;
                proposal.ProvidedInfo.ProvidedInformation = detail.ReturnedInformation;
                proposal.ProvidedInfo.ReviewerId = proposal.Reviewer.Id;
                proposal.RequestedInfoId = proposal.RequestedInfo.Id;
                proposal.RequestingGroupId = proposal.RequestedInfo.RequestingReviewerGroupId;


                var stepNumber = 0;

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveProvidedInfoService",
                        MethodName = "CreateProvidedInfoAsync",
                        Parameters = new List<object> { proposal, increment },
                        Operation = CrudOperationType.Insert,
                        StepNumber = ++stepNumber,

                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveAttachmentService",
                        MethodName = "CreateAttachmentsAsync",
                        Parameters = new List<object> { lookupKey, proposal, files },
                        Operation = CrudOperationType.Insert,
                        StepNumber = ++stepNumber,

                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepResponderService",
                        MethodName = "CreateWorkflowStepResponderAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONDER_REPLY },
                        Operation = CrudOperationType.Insert,
                        StepNumber = ++stepNumber,

                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "CloseOptionsAsync",
                        Parameters = new List<object> { proposal, Guid.Empty, Constants.RESPONDER_ADD_INFO, proposal.RequestedInfoId },
                        Operation = CrudOperationType.Update,
                        StepNumber = ++stepNumber,

                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "ReOpenOptionsAsync",
                        Parameters = new List<object> { Constants.OPTION_TYPE_VERIFY, proposal },
                        Operation = CrudOperationType.Update,
                        StepNumber = ++stepNumber,

                    }
                );


                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveRequestedInfoService",
                        MethodName = "UpdateRequestedInfoAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Update,
                        StepNumber = ++stepNumber,

                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveEmailNotificationService",
                        MethodName = "CreateEmailNotificationsAsync",
                        Parameters = new List<object> { proposal, Constants.EMAIL_PROVIDE_MORE_INFORMATION, requestingUser },
                        Operation = CrudOperationType.Insert,
                        StepNumber = ++stepNumber,

                    }
                );

            }
            else if (scenarioId == "SCN003")
            {
                proposal.ReviewerGroupId = proposal.VerifyingGroupId;
                proposal.ReviewerId = detail.ReviewerId;
                proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);
                proposal.VerifyAndSendToVPFinance = detail.VerifyAndSendToVPFinance;
                var workflowStep = proposal.WorkflowStep;
                var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter());
                var currentStepNumber = workflowTemplates
                .Where(x => x.StepName == workflowStep.StepName)
                .First()
                .StepNumber;

                proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                    .Where(x => !x.IsTerminate && x.OptionType == Constants.OPTION_TYPE_VERIFY)
                   .ToList();

                if (scenarioDetail.IsTestMode)
                {

                    workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                        .Where(x => x.IsComplete)
                        .LastOrDefault();

                    var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                       .ToList();

                    currentStepNumber = workflowTemplates
                        .Where(x => x.StepName == workflowStep.StepName)
                        .First()
                        .StepNumber;

                    // simulate AddWorkflowStepOption
                    if (!workflowStepOptions.Any(x => x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
                    {
                        var newWorkflowStepOption = _mapper.Map<WorkflowStepOption>(proposal.Reviewer);
                        newWorkflowStepOption.OptionType = workflowStepOptions.FirstOrDefault().OptionType;

                        workflowStepOptions.Add(_mapper.Map<WorkFlowStepOptionViewModel>(newWorkflowStepOption));
                    }

                    proposal.WorkflowStepOptions = workflowStepOptions;
                    proposal.WorkflowStepId = workflowStep.WorkflowStepID;
                    proposal.WorkflowStep = await _ssmWorkflowServices.GetWorkflowStep(workflowStep.WorkflowStepID);

                }

                var nextStepNumber = currentStepNumber + 1;

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "CloseOptionsAsync",
                        Parameters = new List<object> { proposal, Guid.Empty, Constants.OPTION_TYPE_VERIFY, null },
                        Operation = CrudOperationType.Update
                    }
                );

                predictiveMethods.Add(
                   new PredictiveMethod
                   {
                       ServiceName = "IPredictiveWorkflowStepResponderService",
                       MethodName = "CreateWorkflowStepResponderAsync",
                       Parameters = new List<object> { proposal, Constants.RESPONDER_VERIFY },
                       Operation = CrudOperationType.Insert
                   }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowInstanceHistoryService",
                        MethodName = "CreateWorkflowInstanceHistoryAsync",
                        Parameters = new List<object> { proposal, Constants.RESPONSE_VERIFIED },
                        Operation = CrudOperationType.Insert
                    }
                );

                if (await _predictiveWorkflowStepService.AllGroupsVerifiedAsync(proposal))
                {
                    predictiveMethods.Add(
                        new PredictiveMethod
                        {
                            ServiceName = "IPredictiveWorkflowStepService",
                            MethodName = "MarkStepCompleteAsync",
                            Parameters = new List<object> { proposal },
                            Operation = CrudOperationType.Update
                        }
                    );

                    if (!await _predictiveWorkflowStepService.AllStepsCompleteAsync(proposal))
                    {
                        var createStep = true;

                        while (createStep)
                        {
                            var reviewerGroups = await _actualReviewerGroupService.GetFilteredReviewerGroupsAsync(nextStepNumber);
                            var filteredReviewerGroups = _actualReviewerGroupService.FilterReviewerGroups(reviewerGroups, proposal, nextStepNumber);

                            proposal.ReviewerGroups = filteredReviewerGroups;

                            var workflowTemplate = workflowTemplates
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

                                predictiveMethods.Add(
                                    new PredictiveMethod
                                    {
                                        ServiceName = "IPredictiveWorkflowStepService",
                                        MethodName = "CreateNextStepAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                predictiveMethods.Add(
                                    new PredictiveMethod
                                    {
                                        ServiceName = "IPredictiveWorkflowInstanceService",
                                        MethodName = "CreateNextStepWorkflowInstanceAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                predictiveMethods.Add(
                                    new PredictiveMethod
                                    {
                                        ServiceName = "IPredictiveWorkflowStakeHolderService",
                                        MethodName = "CreateNextStepWorkflowStakeholdersAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                predictiveMethods.Add(
                                    new PredictiveMethod
                                    {
                                        ServiceName = "IPredictiveWorkflowStepOptionService",
                                        MethodName = "CreateVerifyWorkflowStepOptionsAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                predictiveMethods.Add(
                                    new PredictiveMethod
                                    {
                                        ServiceName = "IPredictiveWorkflowInstanceHistoryService",
                                        MethodName = "CreateNextStepWorkflowInstanceHistoryAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

                                if (!string.IsNullOrWhiteSpace(workflowTemplate.AdditionalTask))
                                {
                                    if (workflowTemplate.AdditionalTask == Constants.ADDITIONAL_TASK_CREATE_WBS_NUMBERS)
                                    {
                                        predictiveMethods.Add(
                                            new PredictiveMethod
                                            {
                                                ServiceName = "IPredictiveWbsService",
                                                MethodName = "CreateWBSNumbersAsync",
                                                Parameters = new List<object> { proposal },
                                                Operation = CrudOperationType.Update
                                            }
                                        );

                                    }
                                }

                                predictiveMethods.Add(
                                    new PredictiveMethod
                                    {
                                        ServiceName = "IPredictiveEmailNotificationService",
                                        MethodName = "CreateNextStepEmailNotificationsAsync",
                                        Parameters = new List<object> { proposal },
                                        Operation = CrudOperationType.Insert
                                    }
                                );

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
                var submitUser = (await _scenarioControllerService.GetSubmitUsersAsync(detail.ProposalId))
                    .FirstOrDefault(u => u.Value == detail.SubmitUserId).Text;


                proposal.SubmitUserId = detail.SubmitUserId;
                var reviewerGroups = await _actualReviewerGroupService.GetFilteredReviewerGroupsAsync(Constants.STEP_ONE);
                var filteredReviewerGroups = _actualReviewerGroupService.FilterReviewerGroups(reviewerGroups, proposal, Constants.STEP_ONE);

                proposal.ReviewerGroups = filteredReviewerGroups;

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowService",
                        MethodName = "CreateWorkflow",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepService",
                        MethodName = "CreateWorkflowStepAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowInstanceService",
                        MethodName = "CreateWorkflowInstanceAsync",
                        Parameters = new List<object> { proposal, proposal.SubmitUserId },
                        Operation = CrudOperationType.Insert
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStakeHolderService",
                        MethodName = "CreateWorkflowStakeholders",
                        Parameters = new List<object> { proposal, Constants.STEP_ONE },
                        Operation = CrudOperationType.Insert
                    }
                 );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveWorkflowStepOptionService",
                        MethodName = "CreateSubmitWorkflowStepOptionsAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert
                    }
                );

                predictiveMethods.Add(
                    new PredictiveMethod
                    {
                        ServiceName = "IPredictiveEmailNotificationService",
                        MethodName = "CreateSubmitEmailNotificationsAsync",
                        Parameters = new List<object> { proposal },
                        Operation = CrudOperationType.Insert,

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

            return predictiveMethods;
        }

    }
}
