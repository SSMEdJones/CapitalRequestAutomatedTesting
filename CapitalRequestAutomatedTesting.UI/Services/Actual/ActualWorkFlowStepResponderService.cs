using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowStepResponderService
    {
        Task<WorkflowStepResponder> GetWorkflowStepResponderAsync(vm.Proposal proposal, string responderType, string optionType);
    }
    public class ActualWorkflowStepResponderService : IActualWorkflowStepResponderService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private IMapper _mapper;

        public ActualWorkflowStepResponderService(ISSMWorkflowServices ssmWorkflowServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _mapper = mapper;
        }

        public async Task<WorkflowStepResponder> GetWorkflowStepResponderAsync(vm.Proposal proposal, string responderType, string optionType)
        {

            var workflowStep = proposal.WorkflowStep;
            var actual = new WorkFlowStepResponderViewModel();
            var workflowStepResponders = await _ssmWorkflowServices.GetAllAddWorkFlowStepResponder(workflowStep.WorkflowStepID);
            var workflowStepOption = GetWorkflowStepOption(proposal, optionType);

            var reviewerGroupId = responderType == Constants.ACTION_TYPE_REQUEST
                ? proposal.RequestingGroupId
                : proposal.ReviewerGroupId;

            var durationMinutes = proposal.ExecutionDurationMinutes ?? 3;

            actual = workflowStepResponders
                .Where(x => x.ReviewerGroupId == reviewerGroupId &&
                    x.WorkflowStepOptionID == workflowStepOption.OptionID &&
                    x.ResponderType == responderType &&
                    x.Responder.ToLower() == proposal.Reviewer.Email &&
                    x.Created.IsFuzzyMatch(DateTime.Now, durationMinutes) &&
                    x.CreatedBy == proposal.Reviewer.UserId)
                .FirstOrDefault();

            if (actual == null)
            {
                actual = new WorkFlowStepResponderViewModel();
            }

            return _mapper.Map<WorkflowStepResponder>(actual);
        }

        private WorkFlowStepOptionViewModel GetWorkflowStepOption(vm.Proposal proposal, string optionType)
        {
            var workflowStepOptions = proposal.WorkflowStepOptions;
            WorkFlowStepOptionViewModel? workflowStepOption = null;

            var reviewerGroupId = optionType == Constants.ACTION_TYPE_ADD_INFO ? proposal.ReviewerGroupId : proposal.RequestingGroupId;
            if (workflowStepOptions.Any())
            {

                var optionsByGroup = workflowStepOptions
                    .Where(x => x.ReviewerGroupId == reviewerGroupId);

                int? requestedInfoId = optionType == Constants.ACTION_TYPE_ADD_INFO ? proposal.RequestedInfoId : null;
                if (optionsByGroup.Any())
                {
                    workflowStepOption = optionsByGroup
                        .Where(x => x.OptionType == optionType &&
                            x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower() &&
                            x.RequestedInfoId == requestedInfoId)
                        .FirstOrDefault();

                }
            }

            if (workflowStepOption == null)
            {
                workflowStepOption = new WorkFlowStepOptionViewModel();
            }
                

            return workflowStepOption;
        }
    }
}
