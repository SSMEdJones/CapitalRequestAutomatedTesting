using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowInstanceHistoryService
    {
        Task<WorkflowInstanceActionHistory> CreateWorkflowInstanceHistoryAsync(vm.Proposal proposal, string responseType);
    }
    public class PredictiveWorkflowInstanceHistoryService : IPredictiveWorkflowInstanceHistoryService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IMapper _mapper;

        public PredictiveWorkflowInstanceHistoryService(
            ICapitalRequestServices capitalRequestServices,
            ISSMWorkflowServices ssmWorkflowServices,
            IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _ssmWorkflowServices = ssmWorkflowServices;
            _mapper = mapper;
        }

        public async Task<WorkflowInstanceActionHistory> CreateWorkflowInstanceHistoryAsync(vm.Proposal proposal, string responseType)
        {
            var workflowIntances = await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId);

            var workflowInstance = workflowIntances
                .Select( x =>  _mapper.Map<WorkflowInstance>(x))
                .FirstOrDefault();

            var workflowInstanceActionHistory = _mapper.Map<WorkflowInstanceActionHistory>(workflowInstance);
            workflowInstanceActionHistory.CompletedBy = proposal.Reviewer.UserId;
            workflowInstanceActionHistory.Action = await GetHistoryActionString(responseType, proposal);

            return workflowInstanceActionHistory;
        }

        public async Task<WorkflowInstanceActionHistory> CreateNextStepWorkflowInstanceHistoryAsync(vm.Proposal proposal)
        {
            var workflowInstances = await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId);

            var workflowInstance = workflowInstances
                .Select(x => _mapper.Map<WorkflowInstance>(x))
                .FirstOrDefault();

            var workflowInstanceActionHistory = _mapper.Map<WorkflowInstanceActionHistory>(workflowInstance);
            workflowInstanceActionHistory.CompletedBy = proposal.Reviewer.UserId;
            workflowInstanceActionHistory.Action = $"{Constants.STEP_ACTION_ADDED} {proposal.NextStepName}";

            return workflowInstanceActionHistory;
        }

        private async Task<string> GetHistoryActionString(string responseType, vm.Proposal proposal)
        {
            var firstName = proposal.Reviewer.FirstName;
            var lastName = proposal.Reviewer.LastName;

            var user = $"{firstName} {lastName}";
            var reviewerGroup = await _capitalRequestServices.GetReviewerGroup(proposal.ReviewerGroupId);
            var reviewerGroupName = reviewerGroup.Name;

            return $"{user} {responseType} for Reviewer Group {reviewerGroupName} for Req Id {proposal.Id} on {DateTime.Today.ToShortDateString()}.";

        }

    }

}
