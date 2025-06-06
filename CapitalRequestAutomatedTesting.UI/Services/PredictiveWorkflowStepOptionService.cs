using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Enums;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

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
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepOptionService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IPredictiveRequestedInfoService predictiveRequestedInfoService,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _predictiveRequestedInfoService = predictiveRequestedInfoService;
            _userContextService = userContextService;
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
                        CreatedBy = _userContextService.UserId,
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
            var reviewerGroupId = proposal.ReviewerGroupId;

            var workflowStepOptions = await GetFilteredOptionsAsync(proposal, optionType, requestedInfoId);

            if (actionType == Constants.OPTION_TYPE_REQUEST)
            {
                var workflowStepOption = workflowStepOptions
                    .OrderByDescending(x => x.Created)
                    .Where(x => x.ReviewerGroupId == reviewerGroupId &&
                    x.OptionType == optionType &&
                    x.OptionName.ToLower() == _userContextService.Email.ToLower())
                    .FirstOrDefault();

                optionId = workflowStepOption.OptionID;
            }


            workflowStepOptions.ForEach(x =>
            {
                x.IsComplete = false;
                x.IsTerminate = false;

                if (x.OptionName.ToLower() == _userContextService.Email.ToLower())
                {
                    if (optionId == Guid.Empty && requestedInfoId == null)
                    {
                        return;
                    }
                    if (x.OptionID == optionId && actionType == Constants.OPTION_TYPE_REQUEST)
                    {
                        return;
                    }
                    if (x.OptionID == optionId)
                    {
                        x.IsComplete = true;
                    }
                    else if (x.OptionID == Guid.Empty)
                    {
                        x.IsTerminate = true;
                    }
                    else
                    {
                        x.IsTerminate = true;
                    }
                }
                else
                {
                    x.IsTerminate = true;
                }
                x.Updated = DateTime.Now;
                x.UpdatedBy = _userContextService.UserId;

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
