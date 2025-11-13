using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using System.Diagnostics;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowStepOptionService
    {
        Task<List<WorkflowStepOption>> GetRequestTypeWorkflowStepOptionsAsync(vm.Proposal proposal);
        //Task<List<WorkflowStepOption>> GetRequestTypeClosedWorkflowStepOptionAsync(vm.Proposal proposal);
        Task<List<WorkflowStepOption>> GetClosedWorkflowStepOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId);
        Task<List<WorkflowStepOption>> GetReOpenedOptionsAsync(string optionType, vm.Proposal proposal);

    }
    public class ActualWorkflowStepOptionService : IActualWorkflowStepOptionService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IActualWorkflowStepService _actualWorkflowStepService;
        private IMapper _mapper;

        public ActualWorkflowStepOptionService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IActualWorkflowStepService actualWorkflowStepService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _actualWorkflowStepService = actualWorkflowStepService;

            _mapper = mapper;
        }

        public async Task<List<WorkflowStepOption>> GetClosedWorkflowStepOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId = null)
        {
            var workflowStep = proposal.WorkflowStep;

            //TODO Verify which reviewer group to use
            var reviewerGroupId = requestedInfoId == null ? proposal.RequestingGroupId : proposal.ReviewerGroupId;

                // Get all workflow step options with appropriate filtering
            var allOptionsQuery = proposal.WorkflowStepOptions
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

            var actual = relevantOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return actual;
        }

        public async Task<List<WorkflowStepOption>> GetExpectedClosedWorkflowStepOptionsAsync(vm.Proposal proposal, string optionType, int? requestedInfoId = null)
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
                    !x.IsTerminate &&
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
                x.OptionName.ToLower() != proposal.Reviewer.Email.ToLower())
                .ToList();

            var actual = relevantOptions
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return actual;
        }

        public async Task<List<WorkflowStepOption>> GetWorkflowStepOptionsAsync(vm.Proposal proposal)
        {
            var workflowStep = await _actualWorkflowStepService.GetWorkflowStepAsync(proposal);
            var workflowStepOptionsViewModels = await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID);

            var workflowStepOptions = workflowStepOptionsViewModels
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return workflowStepOptions;
        }
        //public async Task<List<WorkflowStepOption>> GetRequestTypeClosedWorkflowStepOptionAsync(vm.Proposal proposal)
        //{
        //    var workflowStep = proposal.WorkflowStep;

        //    if (workflowStep == null)
        //    {
        //        throw new Exception("No workflow steps found for the given proposal.");
        //    }

        //    var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
        //                                    .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
        //                                           x.OptionType == Constants.OPTION_TYPE_VERIFY)
        //                                    .ToList();

        //    var deduplicated = allOptions
        //        .GroupBy(x => new { x.OptionName, x.ReviewerGroupId, x.WorkflowStepID })
        //        .Select(g =>
        //            g.OrderBy(x => x.IsTerminate) // false (active) comes before true
        //             .ThenByDescending(x => x.Updated ?? x.Created)
        //             .First()
        //        )
        //        .ToList();

        //    var relevantOptions = deduplicated
        //        .Where(x => x.Updated.HasValue && x.Updated.Value.ToShortDateString() == DateTime.Now.ToShortDateString() &&
        //        x.IsTerminate && !x.IsComplete ||
        //        (!x.Updated.HasValue && !x.IsTerminate && !x.IsComplete &&
        //        x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
        //        .ToList();

        //    var actual = relevantOptions
        //        .Select(x => _mapper.Map<WorkflowStepOption>(x))
        //        .ToList();

        //    return actual;
        //}

        //public async Task<List<WorkflowStepOption>> GetReplyTypeClosedWorkflowStepOptionAsync(vm.Proposal proposal, string optionType)
        //{
        //    //request
        //    //CloseOptions(workflowStepOptions, requestedInfo.RequestingReviewerGroupId, Guid.Empty, Constants.OPTION_TYPE_VERIFY, _controllerSharedService.AuthUser, null);
        //    //var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
        //    //                                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
        //    //                                       x.OptionType == Constants.OPTION_TYPE_VERIFY)
        //    //                                .ToList();

        //    //left off here
        //    //CloseOptions(workflowStepOptions, providedInfo.ReviewerGroupId, workflowStepOption.OptionID, Constants.RESPONDER_ADD_INFO, _controllerSharedService.AuthUser, requestedInfo.Id);

        //    var workflowStep = proposal.WorkflowStep;

        //    var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
        //                .Where(x => x.ReviewerGroupId == proposal.RequestedInfo.ReviewerGroupId && x.OptionType == optionType &&
        //                !x.IsComplete &&
        //                !x.IsTerminate &&
        //                x.RequestedInfoId == proposal.RequestedInfo.Id)
        //                .ToList();

        //    var deduplicated = allOptions
        //        .GroupBy(x => new { x.OptionName, x.ReviewerGroupId, x.WorkflowStepID })
        //        .Select(g =>
        //            g.OrderBy(x => x.IsTerminate) // false (active) comes before true
        //             .ThenByDescending(x => x.Updated ?? x.Created)
        //             .First()
        //        )
        //        .ToList();

        //    var relevantOptions = deduplicated
        //        .Where(x => x.Updated.HasValue && x.Updated.Value.ToShortDateString() == DateTime.Now.ToShortDateString() &&
        //        x.IsTerminate && !x.IsComplete ||
        //        (!x.Updated.HasValue && !x.IsTerminate && !x.IsComplete &&
        //        x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
        //        .ToList();

        //    var actual = relevantOptions
        //        .Select(x => _mapper.Map<WorkflowStepOption>(x))
        //        .ToList();

        //    return actual;
        //}



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
        

        //public async Task<List<WorkflowStepOption>> GetReOpenedOptionsAsync(string optionType, vm.Proposal proposal)
        //{
        //    var workflowStep = proposal.WorkflowStep;
        //    var optionId = Guid.Empty;

        //    var reviewerGroupId = proposal.ReplyingGroup.Id;
        //    var requestedInfo = proposal.RequestedInfo;

        //    var workflowstepOptions = proposal.WorkflowStepOptions
        //        .Where(x => x.ReviewerGroupId == reviewerGroupId && x.OptionType == optionType)
        //        .ToList();

        //    var email = proposal.Reviewer.Email.ToLower();
        //    var workflowStepOption = workflowstepOptions
        //        .Where(x => x.IsTerminate == false && x.OptionName == email) 
        //        .OrderByDescending(x => x.Created)
        //        .FirstOrDefault();

        //    if (workflowStepOption != null)
        //    {
        //        optionId = workflowStepOption.OptionID;
        //    }

        //    return workflowstepOptions.Select(x => _mapper.Map<WorkflowStepOption>(x)).ToList();
        //}


        public async Task<List<WorkflowStepOption>> GetReOpenedOptionsAsync(string optionType, vm.Proposal proposal)
        {
            // unlock verify rows for requesting group
            var reviewerGroupId = proposal.RequestingGroupId;
            
            Debug.WriteLine($"proposal.RequestedInfo.RequestingReviewerId : {proposal.RequestedInfo.RequestingReviewerId}");

            var requestingReviewer = await _capitalRequestServices.GetReviewer(proposal.RequestedInfo.RequestingReviewerId);

            // Identify the requesting reviewer’s OptionID
            var requestingOption = proposal.WorkflowStepOptions
                .Where(x =>
                    x.ReviewerGroupId == reviewerGroupId &&
                    x.OptionType == optionType &&
                    x.IsTerminate == false &&
                    x.OptionName?.ToLower() == requestingReviewer.Email.ToLower()
                )
                .OrderByDescending(x => x.Created)
                .FirstOrDefault();

            var requestingOptionId = requestingOption.OptionID;

            // Find all other unlocked options in the same group and step
            var unlockedOptions = proposal.WorkflowStepOptions
                .Where(x =>
                    x.OptionType == optionType &&
                    x.ReviewerGroupId == reviewerGroupId &&
                    x.IsTerminate == false &&
                    x.OptionID != requestingOptionId
                )
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return unlockedOptions;
        }

        public async Task<List<WorkflowStepOption>> GetExpectedReOpenedOptionsAsync(string optionType, vm.Proposal proposal)
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
                    x.IsTerminate == true &&
                    x.OptionID != currentOptionId
                )
                .Select(x => _mapper.Map<WorkflowStepOption>(x))
                .ToList();

            return unlockedOptions;
        }
        

    }
}
