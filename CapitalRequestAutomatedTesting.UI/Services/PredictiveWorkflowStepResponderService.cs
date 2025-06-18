using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveWorkflowStepResponderService
    {
        Task<WorkflowStepResponder> CreateWorkflowStepResponderAsync(vm.Proposal proposal, string responderType);
    }
    public class PredictiveWorkflowStepResponderService : IPredictiveWorkflowStepResponderService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepResponderService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<WorkflowStepResponder> CreateWorkflowStepResponderAsync(vm.Proposal proposal, string responderType)
        {
            // Resolve WorkflowStepOptionId
            var reviewerGroupdId = proposal.ReviewerGroupId;
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId);
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);
            var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                .Where(x => !x.IsComplete && !x.IsTerminate)
                .ToList();

            WorkFlowStepOptionViewModel workflowStepOption = null;
            if (workflowStepOptions.Any())
            {
                var optionsByGroup = workflowStepOptions
                    .Where(x => x.ReviewerGroupId == reviewerGroupdId);

                if (optionsByGroup.Any())
                {
                    workflowStepOption = optionsByGroup
                        .Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                                    x.OptionName.ToLower() == _userContextService.Email.ToLower())
                        .FirstOrDefault();
                }
            }
            // Generate WorkflowStepResponder object
            var workflowStepResponder = _mapper.Map<WorkflowStepResponder>(workflowStepOption);
            workflowStepResponder.ResponderType = responderType;
            workflowStepResponder.Responder = _userContextService.Email;
            workflowStepResponder.CreatedBy = _userContextService.UserId;
            workflowStepResponder.WorkflowStepOptionID = workflowStepOption.OptionID;

            return workflowStepResponder;
        }
    }

}
