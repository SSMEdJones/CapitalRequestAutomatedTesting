using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequest.API.Enums;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using SSMWorkflow.API.DataAccess.Models;
using System.Linq;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowStepOptionService
    {
        //WorkflowStepOption CreateWorkflowStepOption(vm.Proposal proposal, string OptionType);
        Task<List<WorkflowStepOption>> CloseOptionsAsync(vm.Proposal proposal, Guid optionId, string OptionType, int? requestedInfoId);
        Task<List<WorkflowStepOption>> ReOpenOptionsAsync(string optionType, vm.Proposal proposal);
        Task<List<WorkflowStepOption>> GetFilteredOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId);
        Task<List<WorkflowStepOption>> CreateWorkflowStepOptionsAsync(vm.Proposal proposal, string OptionType, int? requestedInfoId);
        Task<List<WorkflowStepOption>> CreateSubmitWorkflowStepOptionsAsync(vm.Proposal proposal);
        Task<List<WorkflowStepOption>> CreateVerifyWorkflowStepOptionsAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateResponseMessageAsync(vm.Proposal proposal, string actionType, string expectedMessage);
        Task<WorkflowStepOption> FindOrCreateWorkflowStepOptionAsync(vm.Proposal proposal, int reviewerGroupId, int reviewerId, string actionType);
        Task<vm.Proposal> PredictiveMessage(vm.Proposal proposal);
    }

    public class PredictiveWorkflowStepOptionService : IPredictiveWorkflowStepOptionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepOptionService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<List<WorkflowStepOption>> CreateWorkflowStepOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId)
        {
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId);
            var workflowStep = _mapper.Map<WorkflowStep>(workflowSteps.FirstOrDefault(x => !x.IsComplete));

            var reviewerGroups = (await GetReviewerGroupsAsync(proposal, workflowStep))
                .Where(x => x.Id == proposal.RequestedInfo.ReviewerGroupId)
                    .ToList();

            var emailTemplate = (await _capitalRequestServices
                .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = Constants.EMAIL_REQUEST_MORE_INFORMATION }))
                .FirstOrDefault();

            var emailType = emailTemplate?.OptionType ?? string.Empty;

            var workflowStepOptions = new List<WorkflowStepOption>();

            foreach (var rg in reviewerGroups)
            {
                var reviewers = (await GetReviewers(proposal))
                    .Where(x => x.ReviewerGroupId == rg.Id)
                    .Select(z => _mapper.Map<vm.Reviewer>(z))
                    .ToList();

                foreach (var r in reviewers)
                {
                    if (emailType == Constants.EMAIL_TYPE_NOTIFY)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(r.Email))
                    {
                        r.Email = proposal.AuthorEmail;
                    }

                    var workflowStepOption = new WorkflowStepOption
                    {
                        OptionName = r.Email,
                        WorkflowStepID = workflowStep.WorkflowStepID,
                        ReviewerGroupId = rg.Id,
                        OptionType = emailType,
                        RequestedInfoId = requestedInfoId,
                        Created = DateTime.Now,
                        CreatedBy = proposal.Reviewer.UserId
                    };

                    workflowStepOptions.Add(workflowStepOption);
                }
            }


            return workflowStepOptions;
        }

        public async Task<List<WorkflowStepOption>> CreateSubmitWorkflowStepOptionsAsync(vm.Proposal proposal)
        {
            var workflowStepOptions = new List<WorkflowStepOption>();

            var reviewerGroups = proposal.ReviewerGroups;

            foreach (var reviewerGroup in reviewerGroups)
            {
                var reviewers = (await GetFilteredReviewersAsync(proposal.Region, proposal.SegmentId, reviewerGroup.Id))
                            .OrderBy(y => y.Email)
                            .ToList();

                var emailTemplates = await _capitalRequestServices
                    .GetAllEmailTemplates(new EmailTemplateSearchFilter());

                var emailType = emailTemplates
                    .FirstOrDefault(y => y.Id == reviewerGroup.EmailTemplateId)
                    ?.OptionType;


                foreach (var reviewer in reviewers)
                {
                    if (emailType == Constants.EMAIL_TYPE_NOTIFY)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(reviewer.Email))
                    {
                        reviewer.Email = proposal.AuthorEmail;
                    }

                    var workflowStepOption = _mapper.Map<WorkflowStepOption>(reviewer);
                    workflowStepOption.CreatedBy = proposal.SubmitUserId;
                    workflowStepOption.OptionType = emailType;

                    workflowStepOptions.Add(workflowStepOption);
                }
            }

            return workflowStepOptions;
        }

        public async Task<List<WorkflowStepOption>> CreateVerifyWorkflowStepOptionsAsync(vm.Proposal proposal)
        {
            var workflowStepOptions = new List<WorkflowStepOption>();

            var reviewerGroups = proposal.ReviewerGroups;

            foreach (var reviewerGroup in reviewerGroups)
            {
                var reviewers = (await GetFilteredReviewersAsync(proposal.Region, proposal.SegmentId, reviewerGroup.Id))
                            .OrderBy(y => y.Email)
                            .ToList();

                var emailTemplates = await _capitalRequestServices
                    .GetAllEmailTemplates(new EmailTemplateSearchFilter());

                var emailType = emailTemplates
                    .FirstOrDefault(y => y.Id == reviewerGroup.EmailTemplateId)
                    ?.OptionType;

                foreach (var reviewer in reviewers)
                {
                    if (emailType == Constants.EMAIL_TYPE_NOTIFY)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(reviewer.Email))
                    {
                        reviewer.Email = proposal.AuthorEmail;
                    }

                    var workflowStepOption = _mapper.Map<WorkflowStepOption>(reviewer);
                    workflowStepOption.CreatedBy = proposal.VerifyUserId;
                    workflowStepOption.OptionType = emailType;
                    workflowStepOptions.Add(workflowStepOption);
                }
            }

            return workflowStepOptions;
        }

        public async Task<List<vm.Reviewer>> GetFilteredReviewersAsync(int region, int segmentId, int reviewerGroupId)
        {
            return await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter
            {
                RegionId = region,
                SegmentId = segmentId,
                ReviewerGroupId = reviewerGroupId
            });
        }

        private async Task<List<vm.Reviewer>> GetReviewers(vm.Proposal proposal)
        {
            return await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { SegmentId = proposal.SegmentId });
        }

        public async Task<List<WorkflowStepOption>> CloseOptionsAsync(vm.Proposal proposal, Guid optionId, string optionType, int? requestedInfoId)
        {
            var workflowStepOptions = new List<WorkflowStepOption>();

            var reviewerGroupId = proposal.ReviewerGroupId;

            //TODO Make sure to only include proper reviewers and dates
            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => !x.IsComplete)
                .FirstOrDefault();

            var stepCreated = workflowStep.Created;

            var workflowTemplate = (await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep?.StepName }))
                .FirstOrDefault();

            var deletedReviewers = (await _capitalRequestServices.GetAllDeletedReviewers(new DeletedReviewerSearchFilter
            {
                SegmentId = proposal.SegmentId,
                RegionId = proposal.Region,
                ReviewerGroupId = proposal.ReviewerGroupId
            }))
            .Where(x => x.Deleted >= workflowStep.Created &&
                   x.Created <= workflowStep.Created)
            .ToList();

            var stepNumber = workflowTemplate?.StepNumber ?? 0;

            var currentReviewers = await _capitalRequestServices
                .GetAllReviewers(new ReviewerSearchFilter
                {
                    SegmentId = proposal.SegmentId,
                    RegionId = proposal.Region,
                    ReviewerGroupId = reviewerGroupId,
                    StepNumber = stepNumber
                }
                );

            var restoredReviewers = deletedReviewers
                .Where(deleted => !currentReviewers.Any(current =>
                    current.Email.Equals(deleted.Email, StringComparison.OrdinalIgnoreCase) &&
                    current.RegionId == deleted.RegionId &&
                    current.SegmentId == deleted.SegmentId &&
                    current.ReviewerGroupId == deleted.ReviewerGroupId
                ))
                .ToList();

            var deletedReviewerList = deletedReviewers.ToList();

            var reviewers = new List<vm.Reviewer>(currentReviewers);

            foreach (var deleted in deletedReviewerList)
            {
                bool exists = currentReviewers.Any(current =>
                    current.Email.Equals(deleted.Email, StringComparison.OrdinalIgnoreCase) &&
                    current.RegionId == deleted.RegionId &&
                    current.SegmentId == deleted.SegmentId &&
                    current.ReviewerGroupId == deleted.ReviewerGroupId);

                if (!exists)
                    reviewers.Add(_mapper.Map<vm.Reviewer>(deleted));
            }

            reviewers.ForEach(x =>
            {
                if (x.Email == proposal.Reviewer.Email && optionType != Constants.OPTION_TYPE_VERIFY)
                {
                    return;
                }

                var created = workflowStep.Created;
                var createdBy = workflowStep.CreatedBy;
                var optionId = Guid.Empty;

                var workflowStepOption = new WorkflowStepOption();

                if (optionType == Constants.OPTION_TYPE_ADD_INFO)
                {
                    var activeOption = proposal.WorkflowStepOptions.Where(w => w.RequestedInfoId == proposal.RequestedInfoId
                                        && w.OptionName == x.Email)
                    .FirstOrDefault();

                    optionId = activeOption.OptionID;
                    created = activeOption.Created;
                    createdBy = activeOption.CreatedBy;

                    workflowStepOption = new WorkflowStepOption
                    {
                        OptionID = optionId,
                        OptionName = x.Email,
                        WorkflowStepID = workflowStep.WorkflowStepID,
                        ReviewerGroupId = reviewerGroupId,
                        OptionType = optionType,
                        RequestedInfoId = requestedInfoId,
                        Created = created,
                        CreatedBy = createdBy,
                        IsComplete = false,
                        IsTerminate = true,
                        Updated = x.Email.ToLower() == proposal.Reviewer.Email.ToLower() ? null : DateTime.Now,
                        UpdatedBy = x.Email.ToLower() == proposal.Reviewer.Email.ToLower() ? null : proposal.Reviewer.UserId

                    };


                }
                else if (optionType == Constants.OPTION_TYPE_VERIFY)
                {
                    var activeOption = proposal.WorkflowStepOptions
                            .Where(w => w.ReviewerGroupId == proposal.ReviewerGroupId
                                        && !w.IsComplete
                                        && !w.IsTerminate
                                        && w.OptionName.ToLower() == x.Email.ToLower())
                    .FirstOrDefault();

                    var isActiveOption = activeOption == null ? false : activeOption.OptionName.ToLower() == proposal.Reviewer.Email.ToLower();

                    if (isActiveOption || activeOption != null)
                    {
                        optionId = activeOption.OptionID;
                        created = activeOption.Created;
                        createdBy = activeOption.CreatedBy;

                    }
                    else
                    {
                        activeOption = proposal.WorkflowStepOptions
                            .Where(w => w.ReviewerGroupId == proposal.ReviewerGroupId
                                    && !w.IsComplete
                                    && w.OptionName.ToLower() == x.Email.ToLower())
                            .OrderByDescending(w => w.Created)
                            .FirstOrDefault();

                        if (activeOption != null)
                        {
                            optionId = activeOption.OptionID;
                            created = activeOption.Created;
                            createdBy = activeOption.CreatedBy;

                        }
                    }

                    workflowStepOption = new WorkflowStepOption
                    {
                        OptionID = optionId,
                        OptionName = x.Email,
                        WorkflowStepID = workflowStep.WorkflowStepID,
                        ReviewerGroupId = reviewerGroupId,
                        OptionType = optionType,
                        RequestedInfoId = requestedInfoId,
                        Created = created,
                        CreatedBy = createdBy,  
                        IsComplete = !isActiveOption ? false : true,
                        IsTerminate = isActiveOption ? false : true,
                        Updated = DateTime.Now,
                        UpdatedBy = proposal.Reviewer.UserId

                    };
                }

                workflowStepOptions.Add(workflowStepOption);

            });

            return workflowStepOptions;
        }

        public async Task<List<WorkflowStepOption>> GetFilteredOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId)
        {
            // Logic to close options based on the provided parameters
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId);
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);

            var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                             x.OptionType == optionType &&
                            (requestedInfoId == null || x.RequestedInfoId == requestedInfoId)
                          )
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return workflowStepOptions;
        }

        public async Task<List<vm.ReviewerGroup>> GetReviewerGroupsAsync(vm.Proposal proposal, WorkflowStep workflowStep)
        {

            var workflowTemplate = (await _capitalRequestServices
                .GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep.StepName }))
                .FirstOrDefault();

            var allReviewerGroups = (await _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter()))
                .Select(x => _mapper.Map<vm.ReviewerGroup>(x))
                .ToList();

            var filteredReviewerGroups = allReviewerGroups
                .Where(x => x.StepNumber <= workflowTemplate.StepNumber && x.ReviewerType == Constants.REVIEW_TYPE_REVIEW ||
                            x.Name == Constants.REVIEWER_GROUP_AUTHOR && x.StepNumber == null)
                .ToList();

            filteredReviewerGroups = FilterReviewerGroups(filteredReviewerGroups, proposal, workflowTemplate.StepNumber);

            return filteredReviewerGroups;
        }

        public List<vm.ReviewerGroup> FilterReviewerGroups(List<vm.ReviewerGroup> reviewerGroups, vm.Proposal proposal, int stepNumber)
        {
            if (proposal.ReviewerGroupId == 0 || stepNumber == Constants.STEP_SIX)
            {

                return reviewerGroups
                        .Where(reviewerGroup =>
                            reviewerGroup.StepNumber == stepNumber &&
                            stepNumber == Constants.STEP_SIX && reviewerGroup.Name != Constants.PURCHASING_GROUP ||
                            !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                            reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments ||
                            reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup)
                        )
                        .ToList();
            }
            else
            {

                return reviewerGroups
                    .Where(reviewerGroup =>
                        reviewerGroup.Id != proposal.ReviewerGroupId &&
                        !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                         reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments ||
                         reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup)
                    )
                    .ToList();
            }
        }

        public async Task<vm.Proposal> PredictiveMessage(vm.Proposal proposal)
        {
            //var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId);
            //var workflowStep = _mapper.Map<WorkflowStep>(workflowSteps.FirstOrDefault(x => !x.IsComplete));
            var workflowStep = _mapper.Map<WorkflowStep>(proposal.WorkflowStep);

            var reviewerGroups = new List<vm.ReviewerGroup>();

            var allGroups = await GetReviewerGroupsAsync(proposal, workflowStep);

            if (proposal.ExpectedMessage == Constants.RESPONSE_ACTION_VERIFIED)
            {
                reviewerGroups = allGroups;
            }
            else
            {
                reviewerGroups = allGroups
                     .Where(x => x.Id == proposal.RequestedInfo.ReviewerGroupId)
                         .ToList();

            }

            var emailTemplate = new vm.EmailTemplate { OptionType = null };

            if (proposal.ExpectedMessage != Constants.RESPONSE_ACTION_VERIFIED)
            {
                emailTemplate = (await _capitalRequestServices
                    .GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = Constants.EMAIL_REQUEST_MORE_INFORMATION }))
                    .FirstOrDefault();

            }

            var emailType = emailTemplate?.OptionType ?? string.Empty;

            //var workflowStepOptionsViewModel = await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID);
            var workflowStepOptionsViewModel = proposal.WorkflowStepOptions;

            var workflowStepOptions = workflowStepOptionsViewModel
                   .Select(x => _mapper.Map<WorkflowStepOption>(x))
                   .ToList();

            await ValidateProposalAsync(proposal, workflowStep, workflowStepOptions);

            return proposal;
        }

        public async Task<SeleniumStepResult> ValidateResponseMessageAsync(vm.Proposal proposal, string actionType, string expectedMessage)
        {
            proposal = await PredictiveMessage(proposal);

            bool isValid = proposal.ResponseMessage == expectedMessage;

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Response Message validation passed."
                    : "Response Message not valid for this Request."
            };
        }

        public async Task ValidateProposalAsync(vm.Proposal proposal, WorkflowStep workflowStep, List<WorkflowStepOption> workflowStepOptions)
        {

            if (!proposal.IsMovingForward)
            {
                proposal.ResponseMessage = Constants.RESPONSE_CANCELLED;

                return;
            }

            var requests = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                IsOpen = true
            }))
            .ToList();

            var openRequest = requests.Where(x => x.RequestingReviewerGroupId == proposal.RequestingGroupId);

            if (proposal.ActionType == Constants.ACTION_TYPE_VERIFY)
            {
                openRequest = requests.Where(x => x.RequestingReviewerGroupId == proposal.ReviewerGroupId);
            }

            if (proposal.ActionType == Constants.ACTION_TYPE_VERIFY && openRequest.Any())
            {
                proposal.ResponseMessage = Constants.RESPONSE_ACTION_TAKEN;
                return;
            }

            if (workflowStep == null)
            {
                proposal.ResponseMessage = Constants.RESPONSE_ACTION_TAKEN;
                return;

            }
            var workflowStepId = workflowStep.WorkflowStepID;

            var reviewerGroupId = proposal.VerifyingGroupId != 0
                ? proposal.VerifyingGroupId
                : proposal.ActionType == Constants.ACTION_TYPE_VERIFY
                    ? proposal.ReviewerGroupId
                    : proposal.RequestedInfo.RequestingReviewerGroupId;

            workflowStepOptions = workflowStepOptions
                .Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                        x.ReviewerGroupId == reviewerGroupId &&
                        x.IsComplete)
                .ToList();

            var openReply = requests.Where(x => x.ReviewerGroupId == proposal.RequestingGroupId);

            var verified = workflowStepOptions.Any();

            if (verified)
            {
                proposal.ResponseMessage = Constants.RESPONSE_ACTION_TAKEN;
                return;
            }
            else if (!openReply.Any() && proposal.ActionType == Constants.ACTION_TYPE_REPLY)
            {
                proposal.ResponseMessage = Constants.RESPONSE_ACTION_TAKEN;
                return;
            }
            else if (!openRequest.Any() && proposal.ActionType == Constants.ACTION_TYPE_ADD_INFO)
            {
                proposal.ResponseMessage = Constants.RESPONSE_ACTION_TAKEN;
                return;
            }

            await ValidateReviewer(proposal, workflowStep);

            return;
        }

        public async Task ValidateReviewer(vm.Proposal proposal, WorkflowStep workflowStep)
        {

            var reviewerGroupId = proposal.ActionType == Constants.ACTION_TYPE_ADD_INFO
                ? proposal.ReplyingGroupId
                : proposal.ActionType == Constants.ACTION_TYPE_VERIFY
                    ? proposal.VerifyingGroupId
                    : proposal.ReviewerGroupId;

            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(reviewerGroupId);

            //TODO Revisit this. Is this valid since we are poulating reviewer
            if (proposal.Reviewer == null)
            {
                proposal.ResponseMessage = Constants.RESPONSE_ACTION_TAKEN;

                return;
            }

            WorkflowStepOption? workflowStepOption = null;

            var workflowStepId = workflowStep.WorkflowStepID;

            var workflowStepOptionsViewModel = proposal.WorkflowStepOptions;

            var workflowStepOptions = workflowStepOptionsViewModel
                   .Select(x => _mapper.Map<WorkflowStepOption>(x))
                   .ToList();

            var reviewers = (await GetReviewers(proposal))
                .Where(x => x.ReviewerGroupId == reviewerGroupId &&
                 x.RegionId == proposal.Region)
                .Select(z => _mapper.Map<vm.Reviewer>(z))
                .ToList();

            // simulate AddWorkflowStepOption
            if (!workflowStepOptions.Any(x => x.OptionType.ToLower() == proposal.Reviewer.Email.ToLower()))
            {
                var newWorkflowStepOption = _mapper.Map<WorkflowStepOption>(proposal.Reviewer);
                newWorkflowStepOption.OptionType = workflowStepOptions.FirstOrDefault().OptionType;

                workflowStepOptions.Add(newWorkflowStepOption);
            }

            if (workflowStepOptions.Any())
            {
                var optionsByGroup = workflowStepOptions
                    .Where(x => x.ReviewerGroupId == reviewerGroupId &&
                                proposal.ActionType == x.OptionType);

                if (optionsByGroup.Any())
                {
                    workflowStepOption = optionsByGroup
                        .Where(x => x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower())
                        .FirstOrDefault();
                }
            }

            if (workflowStepOption == null)
            {
                proposal.ResponseMessage = Constants.RESPONSE_ACTION_TAKEN;
            }
            else
            {
                if (proposal.ActionType == Constants.ACTION_TYPE_ADD_INFO)
                {
                    proposal.ResponseMessage = Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT;
                }
                if (proposal.ActionType == Constants.ACTION_TYPE_VERIFY)
                {
                    proposal.ResponseMessage = Constants.RESPONSE_ACTION_VERIFIED;
                }
                else
                {
                    proposal.ResponseMessage = Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT;
                }
            }

            return;
        }

        public async Task<WorkflowStepOption> FindOrCreateWorkflowStepOptionAsync(vm.Proposal proposal, int reviewerGroupId, int reviewerId, string actionType)
        {
            WorkflowStepOption workflowStepOption = null;

            var reviewer = await _capitalRequestServices.GetReviewer(reviewerId);

            if (reviewer == null)
            {
                return workflowStepOption;
            }

            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId);
            var workflowStep = _mapper.Map<WorkflowStep>(workflowSteps.FirstOrDefault(x => !x.IsComplete));

            if (workflowStep == null)
            {
                return workflowStepOption;
            }

            var workflowStepId = workflowStep.WorkflowStepID;
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(reviewerGroupId);
            workflowStepOption = await ExistingWorkflowStepOption(workflowStepId, reviewer, actionType, proposal, reviewerGroup);

            if (workflowStepOption == null)
            {
                workflowStepOption = new WorkflowStepOption
                {
                    OptionName = reviewer.Email,
                    Created = DateTime.Now,
                    CreatedBy = reviewer.UserId,
                    WorkflowStepID = workflowStepId,
                    ReviewerGroupId = reviewerGroup.Id,
                    OptionType = actionType
                };
            }

            return workflowStepOption;
        }

        public async Task<WorkflowStepOption> ExistingWorkflowStepOption(Guid workflowStepId, vm.Reviewer reviewer, string actionType, vm.Proposal proposal, vm.ReviewerGroup reviewerGroup)
        {
            var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStepId))
                .Where(x => x.ReviewerGroupId == reviewer.ReviewerGroupId &&
                        !x.IsComplete &&
                        !x.IsTerminate &&
                        x.OptionName == (string.IsNullOrEmpty(reviewer.Email) && reviewerGroup.StepNumber == 0
                           ? proposal.AuthorEmail
                           : reviewer.Email) &&
                        x.OptionType == actionType);


            var workflowStepOption = workflowStepOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .FirstOrDefault();

            return workflowStepOption;
        }

        public async Task<List<WorkflowStepOption>> ReOpenOptionsAsync(string optionType, vm.Proposal proposal)
        {
            var workflowStep = proposal.WorkflowStep;
            var reviewer = proposal.Reviewer;
            var reviewerGroupId = proposal.RequestingGroupId;

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                .Where(x => x.ReviewerGroupId == reviewerGroupId &&
                       x.OptionType == optionType)
                .ToList();

            var deduplicated = allOptions
                .GroupBy(x => new { x.OptionName, x.ReviewerGroupId, x.WorkflowStepID })
                .Select(g =>
                    g.OrderBy(x => x.IsTerminate) // false (active) comes before true
                     .ThenByDescending(x => x.Updated ?? x.Created)
                     .First()
                )
                .ToList();

            var relevantOptions = deduplicated
                .Where(x => x.IsTerminate == true)
                .ToList();

            var workflowstepOptions = relevantOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            workflowstepOptions.ForEach(x =>
            {
                x.Updated = DateTime.Now;
                x.UpdatedBy = reviewer.UserId;
                x.IsTerminate = false;

            });

            return workflowstepOptions.Select(x => _mapper.Map<WorkflowStepOption>(x)).ToList();

            //var requestingOptionId = Guid.Empty;

            //var workflowStepOption = deduplicated.FirstOrDefault(x => x.IsTerminate == false);

            //if (workflowStepOption != null)
            //{
            //    requestingOptionId = workflowStepOption.OptionID;

            //}

            //var workflowTemplate = (await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter { StepName = workflowStep?.StepName }))
            //    .FirstOrDefault();

            //var stepNumber = workflowTemplate?.StepNumber ?? 0;

        }
    }

}
