using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowStepService
    {
        Task<WorkflowStep> GetWorkflowStepAsync(vm.Proposal proposal);
        Task<bool> AllGroupsVerifiedAsync(vm.Proposal proposal);
        Task<WorkflowStep> GetMarkStepCompleteAsync(vm.Proposal proposal);
        Task<bool> AllStepsCompleteAsync(vm.Proposal proposal);
        Task<WorkflowStep> GetNextStepCreatedAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowStepService : IActualWorkflowStepService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public ActualWorkflowStepService(
            ISSMWorkflowServices ssmWorkFlowStepServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkFlowStepServices;
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<WorkflowStep> GetWorkflowStepAsync(vm.Proposal proposal)
        {

            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId)).FirstOrDefault();

            return _mapper.Map<WorkflowStep>(workflowStep);
        }

        public async Task<bool> AllGroupsVerifiedAsync(vm.Proposal proposal)
        {
            var workflowStep = await _ssmWorkflowServices.GetWorkflowStep(proposal.WorkflowStepId);

            var workflowStepOptions = proposal.WorkflowStepOptions
                .Where(x => x.OptionType == Constants.ACTION_TYPE_VERIFY);

            var reviewerGroups = workflowStepOptions
                .Select(x => x.ReviewerGroupId)
                .Distinct()
                .ToList();

            var verifiedGroups = workflowStepOptions
                .Where(x => x.IsComplete || x.ReviewerGroupId == proposal.VerifyingGroupId)
                .Select(y => y.ReviewerGroupId)
                .Distinct()
                .ToList();

            return reviewerGroups.Count == verifiedGroups.Count;
        }

        public async Task<WorkflowStep> GetMarkStepCompleteAsync(vm.Proposal proposal)
        {
            var workflowSteps = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .OrderByDescending(x => x.Created);

            var workflowStep = workflowSteps.FirstOrDefault(x => x.IsComplete);

            return _mapper.Map<WorkflowStep>(workflowStep);
        }

        public async Task<bool> AllStepsCompleteAsync(vm.Proposal proposal)
        {
            var workflowStep = proposal.WorkflowStep;
            var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter());
            var lastStepNumber = workflowTemplates.Max(x => x.StepNumber);

            var currentStepNumber = workflowTemplates
                .Where(x => x.StepName == workflowStep.StepName)
                .First()
                .StepNumber;

            return currentStepNumber == lastStepNumber;

        }

        public async Task<WorkflowStep> GetNextStepCreatedAsync(vm.Proposal proposal)
        {
            var workflowSteps = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .OrderByDescending(x => x.Created);

            var nextWorkflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);

            return _mapper.Map<WorkflowStep>(nextWorkflowStep);

        }
    }
}
