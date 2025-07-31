using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.DataAccess.Services.Api;
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
            var workflowStep = proposal.WorkflowStep;

            if (workflowStep == null)
            {
                throw new Exception("No workflow steps found for the given proposal.");
            }

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                                            .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                                                   x.OptionType == Constants.OPTION_TYPE_VERIFY)
                                            .ToList();

            var deduplicated = allOptions
                .Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                            x.ReviewerGroupId == proposal.ReviewerGroupId)
                .GroupBy(x => new { x.OptionName, x.ReviewerGroupId, x.WorkflowStepID })
                .Select(g =>
                    g.OrderBy(x => x.IsTerminate) // false (active) comes before true
                     .ThenByDescending(x => x.Updated ?? x.Created)
                     .First()
                )
                .ToList();

            var relevantOptions = deduplicated
                .Where(x => x.Updated.HasValue && x.Updated.Value.ToShortDateString() == DateTime.Now.ToShortDateString() &&
                x.IsTerminate && !x.IsComplete ||
                (!x.Updated.HasValue && !x.IsTerminate && !x.IsComplete &&
                x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
                .ToList();

            var actual = relevantOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return actual;
        }

        public async Task<List<WorkflowStepOption>> GetReplyTypeClosedWorkflowStepOptionAsync(vm.Proposal proposal)
        {
            //left off here
            var workflowStep = proposal.WorkflowStep;

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                                            .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                                                   x.OptionType == Constants.OPTION_TYPE_VERIFY)
                                            .ToList();

            var deduplicated = allOptions
                .Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
                            x.ReviewerGroupId == proposal.ReviewerGroupId)
                .GroupBy(x => new { x.OptionName, x.ReviewerGroupId, x.WorkflowStepID })
                .Select(g =>
                    g.OrderBy(x => x.IsTerminate) // false (active) comes before true
                     .ThenByDescending(x => x.Updated ?? x.Created)
                     .First()
                )
                .ToList();

            var relevantOptions = deduplicated
                .Where(x => x.Updated.HasValue && x.Updated.Value.ToShortDateString() == DateTime.Now.ToShortDateString() &&
                x.IsTerminate && !x.IsComplete ||
                (!x.Updated.HasValue && !x.IsTerminate && !x.IsComplete &&
                x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
                .ToList();

            var actual = relevantOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return actual;
        }
        public async Task<List<WorkflowStepOption>> GetRequestTypeWorkflowStepOptionsAsync(vm.Proposal proposal)
        {
            var workflowStep = proposal.WorkflowStep;

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

        public async Task<List<WorkflowStepOption>> GetReOpenedOptionsAsync(string optionType, vm.Proposal proposal)
        {
            var workflowStep = proposal.WorkflowStep;
            var workflowstepOptions = proposal.WorkflowStepOptions;
            var optionId = Guid.Empty;

            var reviewerGroupId = proposal.ReplyingGroup.Id;

            var filteredOptions = proposal.WorkflowStepOptions
                .Where(x => x.ReviewerGroupId == reviewerGroupId && x.OptionType == optionType)
                .ToList();

            var workflowStepOption = filteredOptions
                .Where(x => x.IsTerminate == false)
                 .OrderByDescending(x => x.Created)
                 .FirstOrDefault();

            if (workflowStepOption != null)
            {
                optionId = workflowStepOption.OptionID;
            }
            

            return workflowstepOptions.Select(x => _mapper.Map<WorkflowStepOption>(x)).ToList();
        }


    }
}
