using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Rollback
{
    public interface IRollbackWorkflowStepResponderService
    {
        Task<WorkflowStepResponder> DeleteWorkflowStepResponderAsync(vm.Proposal proposal, string responderType);
    }
    public class RollbackWorkflowStepResponderService : IRollbackWorkflowStepResponderService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IActualWorkflowStepResponderService _actualWorkflowStepResponderService;
        private readonly IMapper _mapper;

        public RollbackWorkflowStepResponderService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IUserContextService userContextService,
            IActualWorkflowStepResponderService actualWorkflowStepResponderService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _userContextService = userContextService;
            _actualWorkflowStepResponderService = actualWorkflowStepResponderService;
            _mapper = mapper;
        }

        public async Task<WorkflowStepResponder> DeleteWorkflowStepResponderAsync(vm.Proposal proposal, string responderType)
        {
            // Resolve WorkflowStepOptionId
            var reviewerGroupId = proposal.ReviewerGroupId;
            var reviewerId = proposal.ReviewerId;
            var actionType = responderType == Constants.RESPONDER_REQUEST ? Constants.OPTION_TYPE_VERIFY : Constants.ACTION_TYPE_ADD_INFO;
            var workflowStepOption = proposal.WorkflowStepOptions
                .FirstOrDefault(x => x.IsComplete == true && x.IsTerminate == false && x.ReviewerGroupId == reviewerGroupId);

            var workflowStepResponder = await _actualWorkflowStepResponderService.GetWorkflowStepResponderAsync(proposal, responderType);

            _ssmWorkflowServices.DeleteWorkflowStepResponder(workflowStepResponder.ResponderID);

            return workflowStepResponder;
        }

        
    }

}
