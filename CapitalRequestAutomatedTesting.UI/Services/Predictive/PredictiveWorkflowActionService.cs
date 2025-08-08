using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowActionService
    {
        Task<SeleniumStepResult> ValidateWorkflowButtonAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateVerifyButtonAsync(vm.Proposal proposal, int reviewerGroupId, string expectedMessage);
    }
    public class PredictiveWorkflowActionService : IPredictiveWorkflowActionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IPredictiveWorkflowStepOptionService _predictiveWorkflowStepOptionService;
        private readonly IMapper _mapper;

        public PredictiveWorkflowActionService(
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

        public async Task<List<vm.WorkflowAction>> GetWorkflowActionAsync(vm.Proposal proposal)
        {

            var workflowActions = await _capitalRequestServices.GetAllWorkflowActions(new WorkflowActionSearchFilter
            {
                Id = proposal.Id,
                UserId = proposal.Reviewer.UserId,
                Email = proposal.Reviewer.Email,
            });

            return workflowActions;
        }

        public async Task<SeleniumStepResult> ValidateWorkflowButtonAsync(vm.Proposal proposal)
        {
            bool isValid = (await GetWorkflowActionAsync(proposal)).Any();

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Workflow button validation passed."
                    : "Workflow button not found for this Request."

            };
        }

        public async Task<SeleniumStepResult> ValidateVerifyButtonAsync(vm.Proposal proposal, int reviewerGroupId, string expectedMessage)
        {
            var responseMessage = string.Empty;
            var buttonIsValid = (await GetWorkflowActionAsync(proposal))
                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId)
                .FirstOrDefault();

            var actionType = buttonIsValid != null ? buttonIsValid.ActionType : string.Empty;
            if (buttonIsValid != null)
            {
                responseMessage = (await _predictiveWorkflowStepOptionService.PredictiveMessage(proposal)).ResponseMessage;
            }

            bool isValid = responseMessage == expectedMessage;

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Verify button validation passed."
                    : "Verify button not found for this Request."
            };

        }

        public async Task<SeleniumStepResult> ValidateActionButtonAsync(vm.Proposal proposal, int reviewerGroupId, string expectedMessage)
        {
            var responseMessage = string.Empty;

            //var workflowAction = (await GetWorkflowActionAsync(proposal))
            //    .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId && 
            //            x.ActionType == proposal.ActionType)
            //    .FirstOrDefault();

            var workflowAction = (await GetWorkflowActionAsync(proposal))
                .Where(x => x.RequestedInfoId == proposal.RequestedInfo.Id &&
                        x.ActionType == proposal.ActionType)
                .FirstOrDefault();

            if (workflowAction != null)
            {
                responseMessage = (await _predictiveWorkflowStepOptionService.PredictiveMessage(proposal)).ResponseMessage;
            }

            bool isValid = responseMessage == expectedMessage;

            var buttonCaption = proposal.ButtonCaption;
            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? $"{buttonCaption} button validation passed."
                    : $"{buttonCaption} button not found for this Request."
            };

        }

        

    }

}
