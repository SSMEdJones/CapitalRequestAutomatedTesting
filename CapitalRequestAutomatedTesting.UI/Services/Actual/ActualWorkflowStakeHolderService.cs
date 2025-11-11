using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowStakeHolderService
    {
        Task<List<WorkflowStakeholder>> GetWorkflowStakeHoldersAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowStakeHolderService : IActualWorkflowStakeHolderService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private IMapper _mapper;

        public ActualWorkflowStakeHolderService(ISSMWorkflowServices ssmWorkFlowStepServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkFlowStepServices;
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


    }
}
