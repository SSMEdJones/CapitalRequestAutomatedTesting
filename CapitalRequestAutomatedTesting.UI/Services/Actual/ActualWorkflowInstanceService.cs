using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;


namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowInstanceService
    {
        Task<WorkflowInstance> GetWorkflowInstanceAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowInstanceService : IActualWorkflowInstanceService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private IMapper _mapper;

        public ActualWorkflowInstanceService(ISSMWorkflowServices ssmWorkFlowStepServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkFlowStepServices;
            _mapper = mapper;
        }

        public async Task<WorkflowInstance> GetWorkflowInstanceAsync(vm.Proposal proposal)
        {

            var workflowInstance = (await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId)).FirstOrDefault();

            return _mapper.Map<WorkflowInstance>(workflowInstance);
        }

        
    }
}
