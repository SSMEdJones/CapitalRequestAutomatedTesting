using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IActualWorkflowStepOptionService
    {
        Task<List<WorkflowStepOption>> GetRequestTypeClosedWorkflowStepOptionAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowStepOptionService : IActualWorkflowStepOptionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IUserContextService _userContextService;
        private IMapper _mapper;

        public ActualWorkflowStepOptionService(ISSMWorkflowServices ssmWorkflowServices, IUserContextService userContextService, IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<List<WorkflowStepOption>> GetRequestTypeClosedWorkflowStepOptionAsync(vm.Proposal proposal)
        {
            WorkFlowStepViewModel? workflowStep = await GetActiveWorkflowStepAsync(proposal);

            if (workflowStep == null)
            {
                throw new Exception("No workflow steps found for the given proposal.");
            }

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                                            .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                                                   x.OptionType == Constants.OPTION_TYPE_VERIFY)
                                            .ToList();

            var workflowStepOptionsActive = allOptions.Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                              x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower())
                              .First();

            var activeOptionId = workflowStepOptionsActive.OptionID;
            //TODO better value for fuzzyMatch than hardcoded 3 minutes
            var workflowStepOptionsTerminated = allOptions
                            .Where(x => x.IsTerminate &&
                                   x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                                   x.UpdatedBy == proposal.Reviewer.UserId &&
                                   x.Updated.HasValue &&
                                   x.OptionID != activeOptionId 
                                   //x.Updated.Value.IsFuzzyMatch(DateTime.Now, 3)
                                   )
                            .ToList();

            var actual = workflowStepOptionsTerminated
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return actual;
        }

        public async Task<List<WorkflowStepOption>> GetRequestTypeWorkflowStepOptionsAsync(vm.Proposal proposal)
        {
            WorkFlowStepViewModel? workflowStep = await GetActiveWorkflowStepAsync(proposal);

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                                .Where(x => x.ReviewerGroupId == proposal.RequestedInfo.ReviewerGroupId &&
                                       x.OptionType == Constants.OPTION_TYPE_ADD_INFO &&
                                       x.CreatedBy == proposal.Reviewer.UserId &&
                                       x.RequestedInfoId == proposal.RequestedInfo.Id
                                       )
                                .ToList();
            var actual = allOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();


            return actual;

        }

        private async Task<WorkFlowStepViewModel?> GetActiveWorkflowStepAsync(vm.Proposal proposal)
        {
            var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId);
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);

            return workflowStep;
        }


    }
}
