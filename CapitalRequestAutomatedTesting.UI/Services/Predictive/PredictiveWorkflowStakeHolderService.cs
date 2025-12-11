using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowStakeHolderService
    {
        List<WorkflowStakeholder> CreateWorkflowStakeholders(vm.Proposal proposal);
        Task<List<WorkflowStakeholder>> CreateNextStepWorkflowStakeholdersAsync(vm.Proposal proposal);
    }
    public class PredictiveWorkflowStakeHolderService : IPredictiveWorkflowStakeHolderService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStakeHolderService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper
)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public List<WorkflowStakeholder> CreateWorkflowStakeholders(vm.Proposal proposal)
        {
            var workflowStakeholders = new List<WorkflowStakeholder>();
            var filteredReviewerGroups = proposal.ReviewerGroups;

            filteredReviewerGroups.ForEach(reviewerGroup =>
            {
                var workFlowStakeholder = _mapper.Map<WorkflowStakeholder>(reviewerGroup);
                workFlowStakeholder.CreatedBy = proposal.SubmitUserId;
                workflowStakeholders.Add(workFlowStakeholder);
            });

            return workflowStakeholders;
        }

        public async Task<List<WorkflowStakeholder>> CreateNextStepWorkflowStakeholdersAsync(vm.Proposal proposal)
        {

            var workflowStakeholders = new List<WorkflowStakeholder>();

            var verifyingGroup = await _capitalRequestServices.GetReviewerGroup(proposal.VerifyingGroupId);

            var currentStepNumber = verifyingGroup.StepNumber;

            var allGroups = await _capitalRequestServices.GetAllReviewerGroups(
                new CapitalRequest.API.DataAccess.Models.ReviewerGroupSearchFilter
                {
                    ReviewerType = Constants.REVIEW_TYPE_REVIEW
                });

            var previousStepGroups = allGroups
                .Where(x => x.StepNumber < currentStepNumber + 1 || x.Name == Constants.REVIEWER_GROUP_AUTHOR)
                .ToList();

            var filteredReviewerGroups = proposal.ReviewerGroups;

            filteredReviewerGroups.ForEach(reviewerGroup =>
            {
                if (workflowStakeholders.Any(x => x.Stakeholder == reviewerGroup.Name))
                {
                    return;
                }

                var workFlowStakeholder = _mapper.Map<WorkflowStakeholder>(reviewerGroup);
                workFlowStakeholder.Created = DateTime.Now;
                workFlowStakeholder.CreatedBy = proposal.Reviewer.UserId;
                workFlowStakeholder.WorkflowID = proposal.WorkflowId;

                workflowStakeholders.Add(workFlowStakeholder);
            });


            return workflowStakeholders;
        }

    }

}
