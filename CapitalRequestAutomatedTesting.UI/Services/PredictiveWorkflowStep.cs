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
    public interface IPredictiveWorkflowStepService
    {
        WorkflowStep CreateWorkflowStep(vm.Proposal proposal);
        WorkFlowStepViewModel? GetWorkflowStep(vm.Proposal proposal);
    }
    public class PredictiveWorkflowStepService : IPredictiveWorkflowStepService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepService(
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

        public WorkflowStep CreateWorkflowStep(vm.Proposal proposal)
        {
            // Resolve WorkflowStepOptionId
            var workflowStep = new WorkflowStep();
            //var reviewerGroupdId = proposal.ReviewerGroupId;
            //var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId).Result;
            //var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);
            //var workflowStepOptions = _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID).Result
            //    .Where(x => !x.IsComplete && !x.IsTerminate)
            //    .ToList();
            //WorkFlowStepOptionViewModel workflowStepOption = null;
            //if (workflowStepOptions.Any())
            //{
            //    var optionsByGroup = workflowStepOptions
            //        .Where(x => x.ReviewerGroupId == reviewerGroupdId);

            //    if (optionsByGroup.Any())
            //    {
            //        workflowStepOption = optionsByGroup
            //            .Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
            //                        x.OptionName.ToLower() == _userContextService.Email.ToLower())
            //            .FirstOrDefault();
            //    }
            //}
            //// Generate WorkflowStep object
            //var WorkflowStep = _mapper.Map<WorkflowStep>(workflowStepOption);
            //WorkflowStep.ResponderType = responderType;
            //WorkflowStep.Responder = _userContextService.Email;
            //WorkflowStep.CreatedBy = _userContextService.UserId;
            //WorkflowStep.WorkflowStepOptionID = workflowStepOption.OptionID;

            return workflowStep;
        }

        public WorkFlowStepViewModel? GetWorkflowStep(vm.Proposal proposal)
        {
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId).Result;
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);
            return workflowStep;
        }
    }

}
