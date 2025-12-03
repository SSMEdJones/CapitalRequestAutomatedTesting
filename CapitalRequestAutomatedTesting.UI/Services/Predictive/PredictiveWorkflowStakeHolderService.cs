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
            var stakeholderViewModels = await _ssmWorkflowServices.GetAllWorkFlowStakeholders(proposal.WorkflowId);

            var workflowStakeholders = stakeholderViewModels
                .Select(x => _mapper.Map<WorkflowStakeholder>(x))
                .ToList();

            var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(
                    new CapitalRequest.API.DataAccess.Models.WorkflowTemplateSearchFilter()
                );

            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .FirstOrDefault(x => !x.IsComplete);

            var currentStepNumber = workflowTemplates.FirstOrDefault(x => x.StepName == workflowStep.StepName).StepNumber;

            var allGroups = await _capitalRequestServices.GetAllReviewerGroups(
                new CapitalRequest.API.DataAccess.Models.ReviewerGroupSearchFilter
                {
                    ReviewerType = Constants.REVIEW_TYPE_REVIEW
                });

            var previousStepGroups = allGroups
                .Where(x => x.StepNumber < currentStepNumber || x.Name == Constants.REVIEWER_GROUP_AUTHOR)
                .ToList();

            var workFlowStakeholderViewModels = (await _ssmWorkflowServices.GetAllWorkFlowStakeholders(proposal.WorkflowId))
                .Where(x => x.WorkflowID == proposal.WorkflowId && x.Stakeholder != Constants.REVIEWER_GROUP_AUTHOR)
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
