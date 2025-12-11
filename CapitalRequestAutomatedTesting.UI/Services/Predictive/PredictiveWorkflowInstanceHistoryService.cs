using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using SSMWorkflow.API.DataAccess.Models;
using System.Runtime.InteropServices;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowInstanceHistoryService
    {
        Task<List<WorkflowInstanceActionHistory>> CreateWorkflowInstanceHistoryAsync(vm.Proposal proposal, string responseType);
        Task<List<WorkflowInstanceActionHistory>> CreateNextStepWorkflowInstanceHistoryAsync(vm.Proposal proposal);
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

        public async Task<List<WorkflowInstanceActionHistory>> CreateWorkflowInstanceHistoryAsync(vm.Proposal proposal, string responseType)
        {
            var workflowIntances = await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId);

            var workflowInstance = workflowIntances
                .Select( x =>  _mapper.Map<WorkflowInstance>(x))
                .FirstOrDefault();

            var workflowInstanceActionHistories = new List<WorkflowInstanceActionHistory>();    
            var workflowInstanceActionHistory = _mapper.Map<WorkflowInstanceActionHistory>(workflowInstance);
            workflowInstanceActionHistory.CompletedBy = proposal.Reviewer.UserId;
            workflowInstanceActionHistory.Action = await GetHistoryActionString(responseType, proposal);

            workflowInstanceActionHistories.Add(workflowInstanceActionHistory);

            return workflowInstanceActionHistories;
        }

        public async Task<List<WorkflowInstanceActionHistory>> CreateNextStepWorkflowInstanceHistoryAsync(vm.Proposal proposal)
        {
            var workflowInstanceActionHistories = new List<WorkflowInstanceActionHistory>();
            var verifyingGroup = await _capitalRequestServices.GetReviewerGroup(proposal.VerifyingGroupId);

            var currentStepNumber = verifyingGroup.StepNumber;

            var workflowInstances = await _ssmWorkflowServices.GetAllWorkflowInstances(proposal.WorkflowId);

            var workflowInstance = workflowInstances
                .Select(x => _mapper.Map<WorkflowInstance>(x))
                .FirstOrDefault();

            var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(new CapitalRequest.API.DataAccess.Models.WorkflowTemplateSearchFilter());
            var workflowTemplate = workflowTemplates.FirstOrDefault(x => x.StepNumber == currentStepNumber);
            var nextStep = workflowTemplates.FirstOrDefault(x => x.StepNumber == workflowTemplate.StepNumber + 1);

            var actionList = new List<string>
            {
                $"Added step {nextStep.StepName}",
                $"{nextStep.StepDescription}"
            };

            actionList.ForEach(x =>
            {
                var completedBy = x.Contains("Added step") ? proposal.Reviewer.UserId : "_SYSTEM";

                var workflowInstanceActionHistory = _mapper.Map<WorkflowInstanceActionHistory>(workflowInstance);
                workflowInstanceActionHistory.WorkflowInstanceID = Guid.Empty;
                workflowInstanceActionHistory.WorkflowStepID = Guid.Empty;
                workflowInstanceActionHistory.CompletedBy = completedBy;
                workflowInstanceActionHistory.Action = x;

                workflowInstanceActionHistories.Add(workflowInstanceActionHistory);

            });

            return workflowInstanceActionHistories;
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
