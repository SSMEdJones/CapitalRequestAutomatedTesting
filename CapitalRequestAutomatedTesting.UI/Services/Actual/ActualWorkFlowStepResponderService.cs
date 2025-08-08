using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowStepResponderService
    {
        Task<WorkflowStepResponder> GetWorkflowStepResponderAsync(vm.Proposal proposal, string responderType);
    }
    public class ActualWorkflowStepResponderService : IActualWorkflowStepResponderService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IUserContextService _userContextService;
        private IMapper _mapper;

        public ActualWorkflowStepResponderService(ISSMWorkflowServices ssmWorkflowServices, IUserContextService userContextService, IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<WorkflowStepResponder> GetWorkflowStepResponderAsync(vm.Proposal proposal, string responderType)
        {

            var workflowStep = proposal.WorkflowStep;
            if (workflowStep == null)
            {
                throw new Exception("No workflow steps found for the given proposal.");
            }

            var workflowStepResponders = await _ssmWorkflowServices.GetAllAddWorkFlowStepResponder(workflowStep.WorkflowStepID);
            var workflowStepOption = GetWorkflowStepOption(proposal);

            if (workflowStepOption == null)
            {
                throw new Exception("No workflow step option found for the given proposal and workflow step.");
            }

            var actual = workflowStepResponders
                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                    x.WorkflowStepOptionID == workflowStepOption.OptionID &&
                    x.ResponderType == responderType &&
                    x.Responder.ToLower() == proposal.Reviewer.Email &&
                    x.CreatedBy == proposal.Reviewer.UserId)
                .FirstOrDefault();

            return _mapper.Map<WorkflowStepResponder>(actual);
        }

        private WorkFlowStepOptionViewModel GetWorkflowStepOption(vm.Proposal proposal)
        {
            var workflowStepOptions = proposal.WorkflowStepOptions;

            WorkFlowStepOptionViewModel workflowStepOption = null;
            if (workflowStepOptions.Any())
            {
                var optionsByGroup = workflowStepOptions
                    .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId);

                if (optionsByGroup.Any())
                {
                    workflowStepOption = optionsByGroup.Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                                         x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower())
                                         .First();

                }
            }

            return workflowStepOption;
        }
    }
}
