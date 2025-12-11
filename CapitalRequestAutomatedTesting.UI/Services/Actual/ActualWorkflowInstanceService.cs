using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;


namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowInstanceService
    {
        Task<WorkflowInstance> GetWorkflowInstanceAsync(vm.Proposal proposal);
        Task<WorkflowInstance> GetNextStepWorkflowInstanceAsync(vm.Proposal proposal);
    }

    public class ActualWorkflowInstanceService : IActualWorkflowInstanceService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IActualWorkflowStepService _actualWorkflowStepService;
        private IMapper _mapper;

        public ActualWorkflowInstanceService(
            ISSMWorkflowServices ssmWorkFlowStepServices,
            IActualWorkflowStepService actualWorkflowStepService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkFlowStepServices;
            _actualWorkflowStepService = actualWorkflowStepService;
            _mapper = mapper;
        }

        public async Task<WorkflowInstance> GetWorkflowInstanceAsync(vm.Proposal proposal)
        {

            var workflowInstance = (await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId)).FirstOrDefault();

            return _mapper.Map<WorkflowInstance>(workflowInstance);
        }

        public async Task<WorkflowInstance> GetNextStepWorkflowInstanceAsync(vm.Proposal proposal)
        {
            var workflowstep = await _actualWorkflowStepService.GetNextStepCreatedAsync(proposal); 

            var workflowInstance = (await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId))
                                .Where(x => x.CurrentWorkflowStepID == workflowstep.WorkflowStepID)
                                .FirstOrDefault();

            return _mapper.Map<WorkflowInstance>(workflowInstance);
        }
        
    }
}
