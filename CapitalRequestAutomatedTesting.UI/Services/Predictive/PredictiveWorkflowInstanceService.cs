using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowInstanceService
    {
        Task<WorkflowInstance> CreateWorkflowInstanceAsync(vm.Proposal proposal);
        Task<WorkflowInstance> CreateNextStepWorkflowInstanceAsync(vm.Proposal proposal);
    }
    public class PredictiveWorkflowInstanceService : IPredictiveWorkflowInstanceService
    {
        private readonly IPredictiveWorkflowStepService _predictiveWorkflowStepService;
        private readonly IMapper _mapper;

        public PredictiveWorkflowInstanceService(
            IPredictiveWorkflowStepService predictiveWorkflowStepService,
            IMapper mapper)
        {
            _predictiveWorkflowStepService = predictiveWorkflowStepService;
            _mapper = mapper;
        }

        public async Task<WorkflowInstance> CreateWorkflowInstanceAsync(vm.Proposal proposal)
        {
            var workflowStep = await _predictiveWorkflowStepService.CreateWorkflowStepAsync(proposal);
            var workflowInstance = _mapper.Map<WorkflowInstance>(workflowStep);

            return workflowInstance;
        }

        public async Task<WorkflowInstance> CreateNextStepWorkflowInstanceAsync(vm.Proposal proposal)
        {
            var workflowStep = await _predictiveWorkflowStepService.CreateNextStepAsync(proposal);
            var workflowInstance = _mapper.Map<WorkflowInstance>(workflowStep);

            return workflowInstance;
        }
    }

}
