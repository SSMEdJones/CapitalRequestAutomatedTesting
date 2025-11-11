using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowService
    {
        Task<Workflow> GetWorkflowAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowService : IActualWorkflowService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private IMapper _mapper;

        public ActualWorkflowService(ISSMWorkflowServices ssmWorkflowServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _mapper = mapper;
        }

        public async Task<Workflow> GetWorkflowAsync(vm.Proposal proposal)
        {

            var workflow = await _ssmWorkflowServices.GetWorkflowStep(proposal.WorkflowId);

            return _mapper.Map<Workflow>(workflow);
        }

        
    }
}
