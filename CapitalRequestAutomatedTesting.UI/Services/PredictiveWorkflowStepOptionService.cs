using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequest.API.Enums;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;
using System.Linq;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveWorkflowStepOptionService
    {
        //WorkflowStepOption CreateWorkflowStepOption(vm.Proposal proposal, string OptionType);
        Task<List<WorkflowStepOption>> CloseOptionsAsync(vm.Proposal proposal, Guid optionId, string OptionType, int? requestedInfoId, string actionType);
        Task<List<WorkflowStepOption>> GetFilteredOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId);
        Task<List<WorkflowStepOption>> CreateWorkflowStepOptionsAsync(vm.Proposal proposal, string OptionType, int? requestedInfoId);
    }

    public class PredictiveWorkflowStepOptionService : IPredictiveWorkflowStepOptionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveRequestedInfoService _predictiveRequestedInfoService;
        private readonly IUserContextService _userContextService;
        private readonly IDeletedReviewers _deletedReviewers;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepOptionService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IPredictiveRequestedInfoService predictiveRequestedInfoService,
            IUserContextService userContextService,
            IDeletedReviewers deletedReviewers,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _predictiveRequestedInfoService = predictiveRequestedInfoService;
            _userContextService = userContextService;
            _deletedReviewers = deletedReviewers;
            _mapper = mapper;
        }

        public async Task<List<WorkflowStepOption>> CreateWorkflowStepOptionsAsync(vm.Proposal proposal, string OptionType, int? requestedInfoId)
        {
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId);
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

        private async Task<List<vm.Reviewer>> GetReviewers(vm.Proposal proposal)
        {
            return await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { SegmentId = proposal.SegmentId });
        }

        public async Task<List<WorkflowStepOption>> CloseOptionsAsync(vm.Proposal proposal, Guid optionId, string optionType, int? requestedInfoId, string actionType)
        {
            var workflowStepOptions = new List<WorkflowStepOption>();

            var reviewerGroupId = proposal.ReviewerGroupId;

            //TODO Make sure to only include proper reviewers and dates
            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId))
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
                    reviewers.Add(_mapper.Map<vm.Reviewer>(deleted)); // you'll need a cast if types differ
            }


            reviewers.ForEach(x =>
            {
                var workflowStepOption = new WorkflowStepOption
                {
                    OptionName = x.Email,
                    WorkflowStepID = workflowStep.WorkflowStepID,
                    ReviewerGroupId = reviewerGroupId,
                    OptionType = optionType,
                    RequestedInfoId = requestedInfoId,
                    Created = workflowStep.Created,
                    CreatedBy = workflowStep.CreatedBy,
                    IsComplete = false,
                    IsTerminate = x.Email.ToLower() == proposal.Reviewer.Email.ToLower() ? false : true,
                    Updated = x.Email.ToLower() == proposal.Reviewer.Email.ToLower() ? null : DateTime.Now,
                    UpdatedBy = x.Email.ToLower() == proposal.Reviewer.Email.ToLower() ? null : proposal.Reviewer.UserId

                };
                workflowStepOptions.Add(workflowStepOption);

            });
           
            return workflowStepOptions;
        }

        public async Task <List<WorkflowStepOption>> GetFilteredOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId)
        {
            // Logic to close options based on the provided parameters
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId);
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
                .Where(x => (x.StepNumber <= workflowTemplate.StepNumber && x.ReviewerType == Constants.REVIEW_TYPE_REVIEW) ||
                            (x.Name == Constants.REVIEWER_GROUP_AUTHOR && x.StepNumber == 0))
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
                            (stepNumber == Constants.STEP_SIX && reviewerGroup.Name != Constants.PURCHASING_GROUP) ||
                            !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                            (reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments) ||
                            (reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup))
                        )
                        .ToList();
            }
            else
            {

                return reviewerGroups
                    .Where(reviewerGroup =>
                        reviewerGroup.Id != proposal.ReviewerGroupId &&
                        !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                         (reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments) ||
                         (reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup))
                    )
                    .ToList();
            }
        }
    }

}
