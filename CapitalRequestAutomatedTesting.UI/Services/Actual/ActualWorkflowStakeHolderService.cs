using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowStakeHolderService
    {
        Task<List<WorkflowStakeholder>> GetWorkflowStakeHoldersAsync(vm.Proposal proposal);
        Task<List<WorkflowStakeholder>> GetNextStepWorkflowStakeHoldersAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowStakeHolderService : IActualWorkflowStakeHolderService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;

        private IMapper _mapper;

        public ActualWorkflowStakeHolderService(
            ISSMWorkflowServices ssmWorkFlowStepServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkFlowStepServices;
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<List<WorkflowStakeholder>> GetWorkflowStakeHoldersAsync(vm.Proposal proposal)
        {
            var workFlowStakeholderViewModels = (await _ssmWorkflowServices.GetAllWorkFlowStakeholders(proposal.WorkflowId));

            var workflowStakeHolders = workFlowStakeholderViewModels
                  .Select(x => _mapper.Map<WorkflowStakeholder>(x))
                  .ToList();

            return workflowStakeHolders;
        }

        public async Task<List<WorkflowStakeholder>> GetNextStepWorkflowStakeHoldersAsync(vm.Proposal proposal)
        {
            var workflowStakeHolders = new List<WorkflowStakeholder>();

            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .FirstOrDefault(x => !x.IsComplete);

            var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(
                    new CapitalRequest.API.DataAccess.Models.WorkflowTemplateSearchFilter()
                );

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


            workFlowStakeholderViewModels.ForEach(x =>
            {
                var Name = x.Stakeholder;
                if (previousStepGroups.Any(g => g.Name == Name))
                {
                    return;

                }

                workflowStakeHolders.Add(_mapper.Map<WorkflowStakeholder>(x));
            });

            return workflowStakeHolders;
        }


    }

   
}
