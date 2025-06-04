using SSMWorkflow.API.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using AutoMapper;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequest.API.DataAccess.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMAuthenticationCore.Models;
using CapitalRequest.API.DataAccess.Services.Api;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveWorkflowStepResponderService
    {
        WorkflowStepResponder CreateWorkflowStepResponder(vm.Proposal proposal, string responderType);
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

        public WorkflowStepResponder CreateWorkflowStepResponder(vm.Proposal proposal, string responderType)
        {
            // Resolve WorkflowStepOptionId
            var reviewerGroupdId = proposal.ReviewerGroupId;
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId).Result;
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);
            var workflowStepOptions = _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID).Result
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
