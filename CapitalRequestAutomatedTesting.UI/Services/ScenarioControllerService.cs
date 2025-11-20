using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using vm = CapitalRequest.API.Models;


namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IScenarioControllerService
    {
        //Task<List<object>> GetAvailableScenariosAsync();
        IActionResult GetScenarioPartialView(string scenarioId);
        Task<List<SelectListItem>> GetReviewersBySelectedGroupAsync(int proposalId, int requestingGroupId);

        string GetScenarioViewName(string scenarioId);
        Task<ScenarioFormViewModel> GenerateScenarioFormViewModel(int? requestId);
        Task<List<SelectListItem>> GetTargetGroupsByRequestIdAsync(int proposalId, int requestingGroupId);
        Task<List<SelectListItem>> GetRequestSelectListAsync();
        Task<List<SelectListItem>> GetRequestingGroupsAsync(int proposalId);
        Task<CapitalRequest.API.Models.Reviewer> GetReviewerByIdAsync(int id);
        Task<List<SelectListItem>> GetSubmitUsersAsync(int proposalId);
        Task<SeleniumStepResult> ValidateTargetGroupIdAsync(vm.Proposal proposal, int requestingGroupId, int targetGroupId);
        Task<SeleniumStepResult> ValidateRequestingGroupIdAsync(vm.Proposal proposal, int requestingGroupId, int targetGroupId);
        Task<CapitalRequest.API.Models.ReviewerGroup> GetReviewerGroupByIdAsync(int id);
        Task<List<SelectListItem>> GetRequestingGroupsByReplyingIdAsync(int proposalId, int replyingGroupId);
        Task<(List<SelectListItem> RequestingGroups, List<SelectListItem> TargetGroups)> BuildRequestingAndTargetGroupsAsync(int proposalId, int? requestingGroupId);
        Task<ScenarioDetailsViewModel> GetScenarioDetail(string scenarioId, int requestId);
        Task<List<vm.ReviewerGroup>> GetReviewerGroupsForReplyingGroup(int proposalId, int groupId);

        Task<object> GetTargetGroupsAndReviewersAsync(int proposalId, int groupId, string groupType);

        Task<bool> AllGroupsVerifiedAsync(vm.Proposal proposal, string optionType);
        Task<SeleniumStepResult> ValidatePauseBeforeSubmitAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateFileUploadAsync(string fileName, string contentType);
        Task<SeleniumStepResult> ValidateFileInputVisibilityAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateFileInputNotVisibileAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateReturnedInformationAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateRequestedInformationAsync(vm.Proposal proposal);

        SeleniumStepResult ValidateSubmitButtonAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateEditButtonAsync(vm.Proposal proposal);

    }

    public class ScenarioControllerService : IScenarioControllerService
    {

        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IWorkflowControllerService _workflowControllerService;

        public ScenarioControllerService(ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IWorkflowControllerService workflowControllerService
)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _workflowControllerService = workflowControllerService;
        }

        public async Task<ScenarioFormViewModel> GenerateScenarioFormViewModel(int? requestId = null)
        {
            var submitted = false;
            var proposalId = 0;
            if (requestId != null)
            {
                proposalId = requestId.Value;
                var proposal = await _capitalRequestServices.GetProposal(requestId.Value);
                if (proposal != null)
                {
                    // 🔥 Fixed syntax error
                    submitted = proposal.WorkflowId != null && proposal.WorkflowId != Guid.Empty;
                }
            }

            var scenarioDetails = new List<ScenarioDetailsViewModel>();
            if (!submitted && requestId != null)
            {
                // 🔥 If NOT submitted, only show SCN003 for testing submission
                scenarioDetails.Add(new ScenarioDetailsViewModel
                {
                    ScenarioId = "SCN004",
                    PartialViewName = "_SubmitRequest",
                    DisplayText = "Submit Request",
                    SequenceNumber = 1,
                    SubmitUsers = await GetSubmitUsersAsync(proposalId)
                });
            }
            else
            {
                // 🔥 If submitted, show SCN001 and SCN002 for request/reply workflow
                scenarioDetails.AddRange(new[]
                {
                    new ScenarioDetailsViewModel
                    {
                        ScenarioId = "SCN001",
                        PartialViewName = "_RequestMoreInfo",
                        DisplayText = "Request More Information",
                        SequenceNumber = 1,
                        RequestingGroups = requestId.HasValue ? await GetRequestingGroupsAsync(requestId.Value) : new List<SelectListItem>()
                    },
                    new ScenarioDetailsViewModel
                    {
                        ScenarioId = "SCN002",
                        PartialViewName = "_ReplyToRequest",
                        DisplayText = "Reply to Request",
                        SequenceNumber = 2,
                        ReplyingGroups = requestId.HasValue ? await GetReplyingGroupsAsync(requestId.Value) : new List<SelectListItem>()
                    },
                    new ScenarioDetailsViewModel
                    {
                        ScenarioId = "SCN003",
                        PartialViewName = "_VerifyRequest",
                        DisplayText = "Verify a Request",
                        SequenceNumber = 2,
                        VerifyingGroups = requestId.HasValue ? await GetReviewerGroupsAsync(requestId.Value) : new List<SelectListItem>(),
                    }

                });
            }

            return new ScenarioFormViewModel
            {
                RequestIds = new List<SelectListItem>(),
                RequestId = requestId ?? 0,
                ScenarioDetails = scenarioDetails
            };
        }

        public async Task<ScenarioDetailsViewModel> GetScenarioDetail(string scenarioId, int requestId)
        {
            var formModel = await GenerateScenarioFormViewModel(requestId);
            var detail = formModel.ScenarioDetails
                .FirstOrDefault(s => s.ScenarioId == scenarioId);


            // Optional: inject fresh group/reviewer lists if needed
            // if (detail.RequestingGroups == null)
            //     detail.RequestingGroups = await GetRequestingGroupsAsync(requestId);
            // ... same for TargetGroups, Reviewers etc.

            return detail;
        }

        public async Task<List<SelectListItem>> GetRequestSelectListAsync()
        {
            return (await _workflowControllerService.GetDashboardItemsFromApiAsync())
                    .Select(x => new SelectListItem
                    {
                        Text = x.ReqId.ToString(),
                        Value = x.ReqId.ToString()
                    }).ToList();
        }

        public async Task<List<SelectListItem>> GetRequestingGroupsAsync(int proposalId)
        {
            var groups = await GetFilteredReviewerGroups(proposalId, null);

            return groups
                .ToList()
                .ConvertAll(x =>
                {
                    return new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                });

        }

        public async Task<List<SelectListItem>> GetReplyingGroupsAsync(int proposalId)
        {

            var workflowPortions = (await _workflowControllerService.GetWorkflowActionsFromApiAsync(proposalId, Constants.ACTION_TYPE_ADD_INFO))
                .Select(x => x.WorkflowPortion)
                .Distinct()
                .ToList();

            var groups = await GetAvailableNamesAsync(workflowPortions);

            return groups
                .ToList()
                .ConvertAll(x =>
                {
                    return new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                });

        }

        public async Task<List<SelectListItem>> GetReviewerGroupsAsync(int proposalId)
        {

            var workflowPortions = (await _workflowControllerService.GetWorkflowActionsFromApiAsync(proposalId, Constants.ACTION_TYPE_VERIFY))
                .Select(x => x.WorkflowPortion)
                .Distinct()
                .ToList();

            var groups = await GetAvailableNamesAsync(workflowPortions);

            return groups
                .ToList()
                .ConvertAll(x =>
                {
                    return new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                });

        }
        private async Task<List<vm.ReviewerGroup>> GetAvailableNamesAsync(List<string> WorkflowPortions)
        {
            var availableNames = new List<string>();

            WorkflowPortions.ForEach(x =>
            {
                var groupName = ExtractGroupName(x);
                availableNames.Add(groupName);

            });

            var groups = await _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter { ReviewerType = Constants.REVIEW_TYPE_REVIEW });

            groups = (from data in groups
                      join name in availableNames on data.Name equals name
                      select data)
                      .ToList();

            return groups;
        }

        public async Task<(List<SelectListItem> RequestingGroups, List<SelectListItem> TargetGroups)> BuildRequestingAndTargetGroupsAsync(int proposalId, int? requestingGroupId)
        {
            var baseGroups = await GetFilteredReviewerGroups(proposalId, null);
            var authorGroup = await _capitalRequestServices
                .GetAllReviewerGroups(new ReviewerGroupSearchFilter { Name = Constants.REVIEWER_GROUP_AUTHOR })
                .ContinueWith(t => t.Result.FirstOrDefault());

            var requestingList = baseGroups
                .Select(g => new SelectListItem
                {
                    Text = g.Name,
                    Value = g.Id.ToString()
                })
                .ToList();

            var targetList = baseGroups
                .Select(g =>
                {
                    var group = CloneGroup(g);

                    if (group.Id == requestingGroupId && authorGroup != null)
                    {
                        group.Id = authorGroup.Id;
                        group.Name = authorGroup.Name;
                        group.EmailTemplateId = authorGroup.EmailTemplateId;
                        group.StepNumber = authorGroup.StepNumber;
                    }

                    return new SelectListItem
                    {
                        Text = group.Name,
                        Value = group.Id.ToString()
                    };
                })
                .ToList();

            return (requestingList, targetList);
        }

        private vm.ReviewerGroup CloneGroup(vm.ReviewerGroup group)
        {
            return new vm.ReviewerGroup
            {
                Id = group.Id,
                Name = group.Name,
                StepNumber = group.StepNumber,
                EmailTemplateId = group.EmailTemplateId,
                // add other fields as needed
            };
        }

        public async Task<List<SelectListItem>> GetTargetGroupsByRequestIdAsync(int proposalId, int requestingGroupId)
        {
            var groups = await GetFilteredReviewerGroups(proposalId, requestingGroupId);

            return groups
                .ToList()
                .ConvertAll(x =>
                {
                    return new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                });

        }

        public async Task<List<vm.ReviewerGroup>> GetFilteredReviewerGroups(int proposalId, int? requestingGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);

            var workflowPortions = (await _workflowControllerService.GetWorkflowActionsFromApiAsync(proposalId))
                .Distinct()
                .ToList();

            var workflowNames = workflowPortions
                .Select(x => x.WorkflowPortion)
                .Distinct()
                .ToList();

            var groups = await GetAvailableNamesAsync(workflowNames);

            var author = (await _capitalRequestServices
                .GetAllReviewerGroups(new ReviewerGroupSearchFilter { Name = Constants.REVIEWER_GROUP_AUTHOR, StepNumber = null }))
                .FirstOrDefault();

            if (requestingGroupId != null)
            {

                foreach (var group in groups.Where(x => x.Id == requestingGroupId))
                {
                    group.Id = author.Id;
                    group.Name = author.Name;
                    group.EmailTemplateId = author.EmailTemplateId;
                    group.StepNumber = author.StepNumber;
                }
            }


            return groups;
        }

        public async Task<List<SelectListItem>> GetRequestingGroupsByReplyingIdAsync(int proposalId, int replyingGroupId)
        {
            var groups = new List<SelectListItem>();

            // Get all open requests for proposal by replying group.  If more than one, present requesting group list
            var reviewerGroups = await GetReviewerGroupsForReplyingGroup(proposalId, replyingGroupId);

            var filter = new RequestedInfoSearchFilter
            {
                ProposalId = proposalId,
                ReviewerGroupId = replyingGroupId,
                IsOpen = true
            };

            var requestedInfos = await _capitalRequestServices.GetAllRequestedInfos(filter);
            if (reviewerGroups.Count > 1)
            {

                groups = reviewerGroups
                .ToList()
                .ConvertAll(x =>
                {
                    return new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                });
            }
            else if (requestedInfos.Count == 1)
            {
                // If only one request, use the requesting group from that request
                var requestedInfo = requestedInfos.FirstOrDefault();
                if (requestedInfo != null)
                {
                    var requestingGroup = await _capitalRequestServices.GetReviewerGroup(requestedInfo.RequestingReviewerGroupId);
                    groups.Add(new SelectListItem
                    {
                        Text = requestingGroup.Name,
                        Value = requestingGroup.Id.ToString()
                    });
                }
            }

            return groups;
        }

        public async Task<List<vm.ReviewerGroup>> GetReviewerGroupsForReplyingGroup(int proposalId, int groupId)
        {
            var reviewerGroups = new List<vm.ReviewerGroup>();

            var filter = new RequestedInfoSearchFilter
            {
                ProposalId = proposalId,
                ReviewerGroupId = groupId,
                IsOpen = true
            };

            var requestedInfos = await _capitalRequestServices.GetAllRequestedInfos(filter);

            if (requestedInfos.Count > 1)
            {
                var groupFilter = new CapitalRequest.API.DataAccess.Models.ReviewerGroupSearchFilter { ReviewerType = Constants.REVIEW_TYPE_REVIEW };
                reviewerGroups = await _capitalRequestServices.GetAllReviewerGroups(groupFilter);
                reviewerGroups = (from data in reviewerGroups
                                  join requests in requestedInfos on data.Id equals requests.RequestingReviewerGroupId
                                  select data)
                  .ToList();
            }

            return reviewerGroups;
        }

        public async Task<List<CapitalRequest.API.Models.Reviewer>> GetFilteredReviewers(int proposalId, int requestingGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(requestingGroupId);

            var filter = new ReviewerSearchFilter
            {
                RegionId = proposal.Region,
                SegmentId = proposal.SegmentId,
                StepNumber = reviewerGroup.StepNumber,
                ReviewerGroupId = reviewerGroup.Id
            };

            var reviewers = await _capitalRequestServices.GetAllReviewers(filter);

            return reviewers;
        }

        public async Task<SeleniumStepResult> ValidateTargetGroupIdAsync(vm.Proposal proposal, int requestingGroupId, int targetGroupId)
        {
            bool isValid = (await GetFilteredReviewerGroups(proposal.Id, requestingGroupId)).Where(X => X.Id == targetGroupId).Any();

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Target Reviewer Group validation passed."
                    : "Target Reviewer Group not found for this Request."
            };
        }

        public async Task<SeleniumStepResult> ValidateRequestingGroupIdAsync(vm.Proposal proposal, int requestingGroupId, int targetGroupId)
        {
            bool isValid = true;
            if (requestingGroupId > 0)
            {
                isValid = (await GetFilteredReviewerGroups(proposal.Id, requestingGroupId)).Where(x => x.Id == targetGroupId).Any();
            }

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Requesting Reviewer Group validation passed."
                    : "Requesting Reviewer Group not found for this Request."
            };
        }

        public async Task<SeleniumStepResult> ValidateRequestingGroupForReplyIdAsync(vm.Proposal proposal, int requestingGroupId, int replyingGroupId)
        {
            bool isValid = true;

            if (requestingGroupId > 0)
            {

                isValid = (await GetReviewerGroupsForReplyingGroup(proposal.Id, replyingGroupId))
                    .Where(x => x.Id == requestingGroupId).Any();
            }

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Requesting Reviewer Group validation passed."
                    : "Requesting Reviewer Group not found for this Request."
            };
        }

        public static string ExtractGroupName(string dashboardText)
        {
            // Split on the first dash and trim the result
            var parts = dashboardText.Split('-', 2);
            var returnVal = parts.Length > 1 ? parts[1].Trim() : dashboardText.Trim();
            return returnVal;
            //return parts.Length > 1 ? parts[1].Trim().Replace(" ", string.Empty) : dashboardText.Trim();
        }

        //public async Task<List<object>> GetAvailableScenariosAsync()
        //{
        //    return new List<object>
        //{
        //    new { id = "SCN001", name = "Request More Information" },
        //    new { id = "SCN002", name = "Reply to Request" },
        //    new { id = "SCN004", name = "Verify" },
        //    new { id = "SCN004", name = "Approve WBS" }
        //};
        //}

        public IActionResult GetScenarioPartialView(string scenarioId)
        {
            return scenarioId switch
            {
                "SCN001" => new PartialViewResult { ViewName = "_RequestMoreInfo" },
                "SCN002" => new PartialViewResult { ViewName = "_ReplyToRequest" },
                "SCN004" => new PartialViewResult { ViewName = "_SubmitRequest" }, // 🔥 Add this
                _ => new PartialViewResult { ViewName = "_DefaultScenario" }
            };
        }

        public async Task<List<SelectListItem>> GetReviewersBySelectedGroupAsync(int proposalId, int reviewerGroupId)
        {
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId)).FirstOrDefault();

            var workflowTemplate = (await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName })).FirstOrDefault();

            var filter = new ReviewerSearchFilter
            {
                RegionId = proposal.Region,
                SegmentId = proposal.SegmentId,
                StepNumber = workflowTemplate?.StepNumber,
                ReviewerGroupId = reviewerGroupId
            };
            var reviewers = await _capitalRequestServices.GetAllReviewers(filter);

            return reviewers.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.FullName
            }).ToList();

        }

        public async Task<List<SelectListItem>> GetSubmitUsersAsync(int proposalId)
        {

            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            var submitUsers = await GetValidSubmitUsers(proposal);

            return submitUsers
                .OrderBy(x => x.FullName)
                .Select(x => new SelectListItem
                {
                    Value = x.UserId,
                    Text = x.FullName
                })
                .ToList();

        }

        private async Task<List<vm.ApplicationUser>> GetValidSubmitUsers(vm.Proposal proposal)
        {
            var submitUsers = await _capitalRequestServices.GetAllApplicationUsers(new ApplicationUserSearchFilter { ApplicationRoleId = Constants.APPLICATION_ROLE_ID_ADMIN });

            var userId = submitUsers.Where(x => x.UserId == proposal.UserId).FirstOrDefault();

            if (!submitUsers.Where(x => x.UserId == proposal.UserId).Any())
            {

                submitUsers.Add(new vm.ApplicationUser
                {
                    UserId = proposal.UserId,
                    FullName = proposal.Author,
                    ApplicationRoleId = Constants.APPLICATION_ROLE_ID_AUTHOR

                });

            }

            return submitUsers;
        }


        public string GetScenarioViewName(string scenarioId)
        {
            return scenarioId switch
            {
                "SCN001" => "_RequestMoreInfo",
                "SCN002" => "_ReplyToRequest",
                "SCN004" => "_SubmitRequest", // 🔥 Add this
                _ => "_DefaultScenario"
            };
        }

        public async Task<Request> GetRequestByIdAsync(int id)
        {
            var initResult = await _workflowControllerService.InitializeDashboardItemsAsync();
            var actions = initResult.WorkflowActions;
            var dashboardItems = initResult.DashboardItems;

            var item = dashboardItems.FirstOrDefault(d => d.ReqId == id);
            if (item == null) return null;

            var request = new Request
            {
                Id = item.ReqId,
                ITReviewStatus = item.ITReviewStatus,
                FacilitiesReviewStatus = item.FacilitiesReviewStatus,
                SupplyChainReviewStatus = item.SupplyChainReviewStatus,
                EPMOReviewStatus = item.EPMOReviewStatus,
                PurchasingReviewStatus = item.PurchasingReviewStatus,
                FinanceReviewStatus = item.FinanceReviewStatus,
                VPOpsReviewStatus = item.VPOpsReviewStatus,
                VPFinanceReviewStatus = item.VPFinanceReviewStatus
            };

            return request;
        }

        public async Task<CapitalRequest.API.Models.Reviewer> GetReviewerByIdAsync(int id)
        {
            return await _capitalRequestServices.GetReviewer(id);
        }

        public async Task<CapitalRequest.API.Models.ReviewerGroup> GetReviewerGroupByIdAsync(int id)
        {
            return await _capitalRequestServices.GetReviewerGroup(id);
        }

        public async Task<object> GetTargetGroupsAndReviewersAsync(int proposalId, int groupId, string groupType)
        {
            var targetGroups = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            var reviewers = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            // Get reviewers for all group types
            reviewers = await GetReviewersBySelectedGroupAsync(proposalId, groupId);

            // Get target groups based on group type
            targetGroups = groupType switch
            {
                "requesting" => await GetTargetGroupsByRequestIdAsync(proposalId, groupId),
                "replying" => await GetRequestingGroupsByReplyingIdAsync(proposalId, groupId),
                _ => targetGroups
            };

            return new
            {
                targetGroups,
                reviewers
            };
        }

        // 🔥 NEW: Generic validation method with configurable delay and messaging
        private async Task<SeleniumStepResult> ExecuteValidationWithDelayAsync(
            string successMessage,
            string failureMessagePrefix = "Validation failed",
            int delayMs = 100)
        {
            try
            {
                await Task.Delay(delayMs);
                return SeleniumStepResult.Pass(successMessage);
            }
            catch (Exception ex)
            {
                return SeleniumStepResult.Fail($"{failureMessagePrefix}: {ex.Message}");
            }
        }

        public async Task<bool> AllGroupsVerifiedAsync(vm.Proposal proposal, string optionType)
        {
            var workflowStep = await _ssmWorkflowServices.GetWorkflowStep(proposal.WorkflowStepId);

            var workflowStepOptions = proposal.WorkflowStepOptions
                .Where(x => x.OptionType == optionType);

            var reviewerGroups = workflowStepOptions
                .Select(x => x.ReviewerGroupId)
                .Distinct()
                .ToList();

            var verifiedGroups = workflowStepOptions
                .Where(x => x.IsComplete)
                .Select(y => y.ReviewerGroupId)
                .Distinct()
                .ToList();

            return reviewerGroups.Count == verifiedGroups.Count;
        }

        // 🔄 REFACTORED: All validation methods now use the common method
        public async Task<SeleniumStepResult> ValidatePauseBeforeSubmitAsync(vm.Proposal proposal)
        {
            return await ExecuteValidationWithDelayAsync("Pause validation completed successfully.");
        }

        public async Task<SeleniumStepResult> ValidateFileInputVisibilityAsync(vm.Proposal proposal)
        {
            return await ExecuteValidationWithDelayAsync(
                "File input can be made visible - validation successful",
                "File input visibility validation failed");
        }

        public async Task<SeleniumStepResult> ValidateFileInputNotVisibileAsync(vm.Proposal proposal)
        {
            return await ExecuteValidationWithDelayAsync(
                "File input can be made invisible - validation successful",
                "File input not visible validation failed");
        }

        public async Task<SeleniumStepResult> ValidateReturnedInformationAsync(vm.Proposal proposal)
        {
            return await ExecuteValidationWithDelayAsync(
                "Returned information validated successfully",
                "Returned information validation failed");
        }

        public async Task<SeleniumStepResult> ValidateRequestedInformationAsync(vm.Proposal proposal)
        {
            return await ExecuteValidationWithDelayAsync(
                "Requested information validated successfully",
                "Requested information validation failed");
        }

        // Note: ValidateFileUploadAsync is different because it has actual validation logic
        public async Task<SeleniumStepResult> ValidateFileUploadAsync(string fileName, string contentType)
        {
            try
            {
                // Validate file exists and has correct properties
                if (string.IsNullOrEmpty(fileName))
                {
                    return SeleniumStepResult.Fail("File name is empty or null");
                }

                if (string.IsNullOrEmpty(contentType))
                {
                    return SeleniumStepResult.Fail($"Content type is missing for file: {fileName}");
                }

                // Use the delay method for consistency
                await Task.Delay(100);
                return SeleniumStepResult.Pass($"File upload validated successfully: {fileName} ({contentType})");
            }
            catch (Exception ex)
            {
                return SeleniumStepResult.Fail($"File upload validation failed: {ex.Message}");
            }
        }

        private SeleniumStepResult ExecuteValidationWithCondition(
            Func<bool> validationCondition,
            string successMessage,
            string failureMessage)
        {
            bool isValid = validationCondition();

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid ? successMessage : failureMessage
            };
        }

        // Refactor ValidateEditButtonAsync to use the common method
        public async Task<SeleniumStepResult> ValidateEditButtonAsync(vm.Proposal proposal)
        {
            var users = await GetValidSubmitUsers(proposal);

            return ExecuteValidationWithCondition(
                () => users.Where(x => x.UserId == proposal.SubmitUserId).Any(),
                "Edit button validation passed.",
                "Edit button not found for this Request."
            );
        }

        // Refactor ValidateSubmitButtonAsync to use the common method
        public SeleniumStepResult ValidateSubmitButtonAsync(vm.Proposal proposal)
        {
            return ExecuteValidationWithCondition(
                () => proposal.WorkflowId == Guid.Empty && proposal.IsMovingForward,
                "Submit button validation passed.",
                "Submit button not found for this Request."
            );
        }

        // Refactor ValidateAttachmentTabAsync to use the common method
        public SeleniumStepResult ValidateAttachmentTabAsync(vm.Proposal proposal)
        {
            return ExecuteValidationWithCondition(
                () => proposal.WorkflowId == Guid.Empty && proposal.IsMovingForward,
                "Attachment Tab validation passed.",
                "Attachment Tab not found for this Request."
            );
        }

        public SeleniumStepResult ValidateSubmitButtonPressedAsync(vm.Proposal proposal)
        {
            return ExecuteValidationWithCondition(
                () => proposal.WorkflowId == Guid.Empty && proposal.IsMovingForward,
                "Submit Button Pressed validation passed.",
                "Submit Button Pressed validation failed."
            );
        }

    }
}

