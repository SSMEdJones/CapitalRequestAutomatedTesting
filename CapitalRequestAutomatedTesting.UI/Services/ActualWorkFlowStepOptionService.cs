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
            WorkFlowStepViewModel? workflowStep = await GetActiveWorkflowStepAsync(proposal);

            if (workflowStep == null)
            {
                throw new Exception("No workflow steps found for the given proposal.");
            }

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                                            .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId &&
                                                   x.OptionType == Constants.OPTION_TYPE_VERIFY)
                                            .ToList();

            var mostRecent = allOptions.OrderByDescending(x => x.Updated)
                .FirstOrDefault();

            if (mostRecent == null)
            {
                throw new Exception("No workflow steps found for the given proposal.");

            }

            //LEFT OFF HERE
            var relevantOptions = allOptions
                .Where(x => x.Created.IsFuzzyMatch(mostRecent.Created, 3) &&
                x.Updated.HasValue && x.Updated.Value.Date == mostRecent.Updated.Value.Date &&
                x.IsTerminate && !x.IsComplete ||
                (!x.Updated.HasValue && !x.IsTerminate && !x.IsComplete &&
                x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
                .ToList();

            //x.Updated == null && !x.IsTerminate && x.IsComplete


            //!x.IsTerminate && !x.IsComplete && x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower() ||

            //.ToList();


            //            and(convert(date, updated) = '5/30/2025'
            // or Updated is null
            //and IsTerminate = 0 and IsComplete = 0 and OptionName = 'edward.jones@ssmhealth.com')
            //var workflowStepOptionsActive = allOptions.Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY &&
            //                  x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower())
            //                  .Last();
            ////TODO clean up terminated
            ////LEFT OFF HERE need to exclude previously terminated maybe group by updated date?
            //var activeOptionId = workflowStepOptionsActive.OptionID;
            ////TODO better value for fuzzyMatch than hardcoded 3 minutes
            //var workflowStepOptionsTerminated = allOptions
            //                .Where(x => x.IsTerminate &&
            //                       x.OptionType == Constants.OPTION_TYPE_VERIFY &&
            //                       x.UpdatedBy == proposal.Reviewer.UserId &&
            //                       x.Updated.HasValue &&
            //                       x.OptionID != activeOptionId 
            //                       //x.Updated.Value.IsFuzzyMatch(DateTime.Now, 3)
            //                       )
            //                .ToList();

            var actual = relevantOptions
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
