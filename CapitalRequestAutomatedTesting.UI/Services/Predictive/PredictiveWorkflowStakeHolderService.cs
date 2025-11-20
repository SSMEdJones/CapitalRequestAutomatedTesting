using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
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
        private readonly IMapper _mapper;

        public PredictiveWorkflowStakeHolderService(
            ISSMWorkflowServices ssmWorkflowServices,
            IMapper mapper
)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
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
            var stakeholderViewModels = await _ssmWorkflowServices.GetAllWorkFlowStakeholders(proposal.WorkflowId);

            var workflowStakeholders = stakeholderViewModels
                .Select(x => _mapper.Map<WorkflowStakeholder>(x))
                .ToList();

            var filteredReviewerGroups = proposal.ReviewerGroups;

            filteredReviewerGroups.ForEach(reviewerGroup =>
            {
                if (workflowStakeholders.Any(x => x.Stakeholder == reviewerGroup.Name))
                {
                    return;
                }

                var workFlowStakeholder = _mapper.Map<WorkflowStakeholder>(reviewerGroup);
                workFlowStakeholder.CreatedBy = proposal.VerifyUserId;

                workflowStakeholders.Add(workFlowStakeholder);
            });

            return workflowStakeholders;
        }

    }

}
