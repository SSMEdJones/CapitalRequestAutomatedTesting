using AutoMapper;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowStakeHolderService
    {
        List<WorkflowStakeholder> CreateWorkflowStakeholders(vm.Proposal proposal, int stepNumber);
    }
    public class PredictiveWorkflowStakeHolderService : IPredictiveWorkflowStakeHolderService
    {
        private readonly IMapper _mapper;

        public PredictiveWorkflowStakeHolderService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public List<WorkflowStakeholder> CreateWorkflowStakeholders(vm.Proposal proposal, int stepNumber)
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

    }

}
