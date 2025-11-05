using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowStepService
    {
        Task<WorkflowStep> CreateWorkflowStepAsync(vm.Proposal proposal);
        WorkFlowStepViewModel? GetWorkflowStep(vm.Proposal proposal);
    }
    public class PredictiveWorkflowStepService : IPredictiveWorkflowStepService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<WorkflowStep> CreateWorkflowStepAsync(vm.Proposal proposal)
        {
            var workflowTemplate = await _capitalRequestServices.GetWorkflowTemplate(Constants.STEP_ONE);

            var workflowStep = _mapper.Map<WorkflowStep>(workflowTemplate);
            workflowStep.CreatedBy = proposal.SubmitUserId;

            return workflowStep;
        }

        public WorkFlowStepViewModel? GetWorkflowStep(vm.Proposal proposal)
        {
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId).Result;
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);

            return workflowStep;
        }
    }

}
