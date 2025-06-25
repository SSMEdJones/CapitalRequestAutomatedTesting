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
using System.ComponentModel.DataAnnotations;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveWorkflowActionService
    {
        Task<SeleniumStepResult> ValidateWorkflowButtonAsync(vm.Proposal proposal);
        Task<SeleniumStepResult> ValidateVerifyButtonAsync(vm.Proposal proposal, int reviewerGroupId);
    }
    public class PredictiveWorkflowActionService : IPredictiveWorkflowActionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public PredictiveWorkflowActionService(
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

        public async Task<SeleniumStepResult> ValidateVerifyButtonAsync(vm.Proposal proposal, int reviewerGroupId)
        {

            bool isValid = (await GetWorkflowActionAsync(proposal)).Any();

            return new SeleniumStepResult
            {
                Success = isValid,
                Message = isValid
                    ? "Verify button validation passed."
                    : "Verify button not found for this Request."
            };

        }

    }

}
