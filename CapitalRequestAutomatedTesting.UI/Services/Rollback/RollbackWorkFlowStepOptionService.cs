using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Extensions;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Rollback
{
    public interface IRollbackWorkflowStepOptionService
    {
        Task<List<WorkflowStepOption>> CloseWorkflowStepOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId);
        Task<List<WorkflowStepOption>> OpenedOptionsAsync(string optionType, vm.Proposal proposal);

    }
    public class RollbackWorkflowStepOptionService : IRollbackWorkflowStepOptionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public RollbackWorkflowStepOptionService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;

            _mapper = mapper;
        }

        public async Task<List<WorkflowStepOption>> CloseWorkflowStepOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId = null)
        {
            var workflowStep = proposal.WorkflowStep;

            var useRequestedInfoReviewerGroup = requestedInfoId == null ? false : true;
            if (workflowStep == null)
            {
                throw new Exception("No workflow steps found for the given proposal.");
            }

            // Determine which reviewer group to use
            var reviewerGroupId = useRequestedInfoReviewerGroup && proposal.RequestedInfo != null
                ? proposal.RequestedInfo.ReviewerGroupId
                : proposal.ReviewerGroupId;

            // Get all workflow step options with appropriate filtering
            var allOptionsQuery = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                .Where(x => x.ReviewerGroupId == reviewerGroupId && x.OptionType == optionType);

            // Apply additional filters for RequestedInfo mode
            if (requestedInfoId.HasValue)
            {
                allOptionsQuery = allOptionsQuery.Where(x =>
                    !x.IsComplete &&
                    x.IsTerminate &&
                    x.RequestedInfoId == requestedInfoId.Value);
            }

            var allOptions = allOptionsQuery.ToList();

            var deduplicated = allOptions
                .GroupBy(x => new { x.OptionName, x.ReviewerGroupId, x.WorkflowStepID })
                .Select(g =>
                    g.OrderBy(x => x.IsTerminate) // false (active) comes before true
                     .ThenByDescending(x => x.Updated ?? x.Created)
                     .First()
                )
                .ToList();

            var relevantOptions = deduplicated
                .Where(x => x.Updated.HasValue &&
                x.IsTerminate && !x.IsComplete ||
                !x.Updated.HasValue && !x.IsTerminate && !x.IsComplete &&
                x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower())
                .ToList();

            var Rollback = relevantOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return Rollback;
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
            var Rollback = allOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();


            return Rollback;

        }


        public async Task<List<WorkflowStepOption>> OpenedOptionsAsync(string optionType, vm.Proposal proposal)
        {
            // unlock verify rows for requesting group
            var reviewer = await _capitalRequestServices.GetReviewer(proposal.RequestedInfo.RequestingReviewerId);
            var reviewerGroupId = reviewer.ReviewerGroupId;

            // Identify the current reviewer’s OptionID
            var currentOptionId = proposal.WorkflowStepOptions
                .Where(x =>
                    x.ReviewerGroupId == reviewerGroupId &&
                    x.OptionType == optionType &&
                    x.IsTerminate == false &&
                    x.OptionName?.ToLower() == proposal.Reviewer.Email.ToLower()
                )
                .OrderByDescending(x => x.Created)
                .Select(x => x.OptionID)
                .FirstOrDefault();

            // Find all other unlocked options in the same group and step
            var unlockedOptions = proposal.WorkflowStepOptions
                .Where(x =>
                    x.OptionType == optionType &&
                    x.ReviewerGroupId == reviewerGroupId &&
                    x.IsTerminate == false &&
                    x.OptionID != currentOptionId
                )
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return unlockedOptions;
        }


    }
}
