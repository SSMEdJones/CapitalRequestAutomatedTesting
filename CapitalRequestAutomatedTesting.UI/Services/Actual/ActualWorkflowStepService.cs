using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowStepService
    {
        Task<WorkflowStep> GetWorkflowStepAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowStepService : IActualWorkflowStepService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private IMapper _mapper;

        public ActualWorkflowStepService(ISSMWorkflowServices ssmWorkFlowStepServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkFlowStepServices;
            _mapper = mapper;
        }

        public async Task<WorkflowStep> GetWorkflowStepAsync(vm.Proposal proposal)
        {

            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId)).FirstOrDefault();

            return _mapper.Map<WorkflowStep>(workflowStep);
        }

        
    }
}
