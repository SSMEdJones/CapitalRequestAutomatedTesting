using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
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
        private readonly IPredictiveWorkflowStepOptionService _predictiveWorkflowStepOptionService;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepResponderService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IUserContextService userContextService,
            IPredictiveWorkflowStepOptionService predictiveWorkflowStepOptionService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _userContextService = userContextService;
            _predictiveWorkflowStepOptionService = predictiveWorkflowStepOptionService;
            _mapper = mapper;
        }

        public async Task<WorkflowStepResponder> CreateWorkflowStepResponderAsync(vm.Proposal proposal, string responderType)
        {
            // Resolve WorkflowStepOptionId
            var reviewerGroupId = proposal.ReviewerGroupId;
            var reviewerId = proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0;
            var actionType = responderType == Constants.RESPONDER_REQUEST || responderType == Constants.OPTION_TYPE_VERIFY
                ? Constants.OPTION_TYPE_VERIFY
                : Constants.ACTION_TYPE_ADD_INFO;

            var workflowStepOption = await _predictiveWorkflowStepOptionService.FindOrCreateWorkflowStepOptionAsync(proposal, reviewerGroupId, reviewerId, actionType);

            // Generate WorkflowStepResponder object
            var responder = ProperCaseEmail(proposal.Reviewer.Email);

            var workflowStepResponder = _mapper.Map<WorkflowStepResponder>(workflowStepOption);
            workflowStepResponder.ResponderType = responderType;
            workflowStepResponder.Responder = responder;
            workflowStepResponder.CreatedBy = proposal.Reviewer.UserId;
            workflowStepResponder.Created = DateTime.Now;
            workflowStepResponder.WorkflowStepOptionID = workflowStepOption.OptionID;
            workflowStepResponder.WorkflowStepID = Guid.Empty;

            return workflowStepResponder;
        }

        private string ProperCaseEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return email;

            var parts = email.Split('@');
            var nameParts = parts[0].Split('.');

            var properName = string.Join(".",
                nameParts.Select(p =>
                    string.IsNullOrWhiteSpace(p)
                        ? p
                        : char.ToUpper(p[0]) + p.Substring(1).ToLower()
                ));

            return $"{properName}@{parts[1].ToLower()}";
        }
    }

}
