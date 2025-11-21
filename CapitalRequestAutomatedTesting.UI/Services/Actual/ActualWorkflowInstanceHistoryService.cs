using AutoMapper;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;


namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWorkflowInstanceHistoryService
    {
        Task<WorkflowInstanceActionHistory> GetWorkflowInstanceHistoryAsync(vm.Proposal proposal);
    }
    public class ActualWorkflowInstanceHistoryService : IActualWorkflowInstanceHistoryService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IActualWorkflowStepService _actualWorkflowStepService;
        private IMapper _mapper;

        public ActualWorkflowInstanceHistoryService(
            ISSMWorkflowServices ssmWorkFlowStepServices,
            ICapitalRequestServices capitalRequestServices,
            IActualWorkflowStepService actualWorkflowStepService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkFlowStepServices;
            _capitalRequestServices = capitalRequestServices;
            _actualWorkflowStepService = actualWorkflowStepService;
            _mapper = mapper;
        }

        public async Task<WorkflowInstanceActionHistory> GetWorkflowInstanceHistoryAsync(vm.Proposal proposal)
        {
            var action = GetHistoryAction(proposal);
            var workflowInstance = (await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId))
                .FirstOrDefault(x => x.CurrentWorkflowStepID == proposal.WorkflowStepId);

            var workflowInstanceActionHistorys = await _ssmWorkflowServices.GetAllWorkflowInstanceActionHistory(
                new WorkFlowInstanceActionHistorySearchFilter
                {
                    WorkflowInstanceID = workflowInstance.WorkflowInstanceID
                });

            var worklowInstanceActionHistory = workflowInstanceActionHistorys
                .FirstOrDefault(x => x.Action == action);

            return _mapper.Map<WorkflowInstanceActionHistory>(worklowInstanceActionHistory);
        }

        public async Task<WorkflowInstanceActionHistory> GetNextStepWorkflowInstanceHistoryAsync(vm.Proposal proposal)
        {
            var workflowStep = await _actualWorkflowStepService.GetWorkflowStepAsync(proposal);
            var action = $"Added step {workflowStep.StepName}";
            var workflowInstance = (await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId))
                .FirstOrDefault(x => x.CurrentWorkflowStepID == workflowStep.WorkflowStepID);

            var workflowInstanceActionHistorys = await _ssmWorkflowServices.GetAllWorkflowInstanceActionHistory(
                new WorkFlowInstanceActionHistorySearchFilter
                {
                    WorkflowInstanceID = workflowInstance.WorkflowInstanceID
                });

            var worklowInstanceActionHistory = workflowInstanceActionHistorys
                .FirstOrDefault(x => x.Action == action);

            return _mapper.Map<WorkflowInstanceActionHistory>(worklowInstanceActionHistory);
        }


        private string GetHistoryAction(Proposal proposal)
        {
            var reviewerName = proposal.Reviewer.FullName;
            var reviewerGroupName = proposal.ReviewerGroupName;
            var reqId = proposal.Id;
            var today = DateTime.Now.ToString("MM/dd/yyyy");
            var action = $"{reviewerName} Verified for Reviewer Group {reviewerGroupName} for Req Id {reqId} on {today}.";

            return action;
        }
    }
}
