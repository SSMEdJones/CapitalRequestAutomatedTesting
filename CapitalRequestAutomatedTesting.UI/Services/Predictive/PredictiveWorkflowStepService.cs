using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowStepService
    {
        Task<WorkflowStep> CreateWorkflowStepAsync(vm.Proposal proposal);
        WorkFlowStepViewModel? GetWorkflowStep(vm.Proposal proposal);
        Task<bool> AllGroupsVerifiedAsync(vm.Proposal proposal);
        Task<bool> AllStepsCompleteAsync(vm.Proposal proposal);
        Task<WorkflowStep> CreateNextStepAsync(vm.Proposal proposal);
    }
        
    public class PredictiveWorkflowStepService : IPredictiveWorkflowStepService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IMapper _mapper;

        public PredictiveWorkflowStepService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<WorkflowStep> CreateWorkflowStepAsync(vm.Proposal proposal)
        {
            var workflowTemplate = await _capitalRequestServices.GetWorkflowTemplate(Constants.STEP_ONE);

            var workflowStep = _mapper.Map<WorkflowStep>(workflowTemplate);
            workflowStep.CreatedBy = proposal.SubmitUserId;

            return workflowStep;
        }

        public WorkFlowStepViewModel? GetWorkflowStep(vm.Proposal proposal)
        {
            var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId).Result;
            var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);

            return workflowStep;
        }

        public async Task<bool> AllGroupsVerifiedAsync(vm.Proposal proposal)
        {
            var workflowStepOptions = proposal.WorkflowStepOptions
                .Where(x => x.OptionType == Constants.OPTION_TYPE_VERIFY);

            var verifyingOption = workflowStepOptions
                .Where(x => x.ReviewerGroupId == proposal.ReviewerGroupId && x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower())
                .FirstOrDefault();

            var workflowStep = await _ssmWorkflowServices.GetWorkflowStep(proposal.WorkflowStepId);
            
            var reviewerGroups = workflowStepOptions
                .Select(x => x.ReviewerGroupId)
                .Distinct()
                .ToList();

            var verifiedGroups = workflowStepOptions
                .Where(x => x.IsComplete || x.OptionID == verifyingOption.OptionID)
                .Select(y => y.ReviewerGroupId)
                .Distinct()
                .ToList();


            return reviewerGroups.Count == verifiedGroups.Count;
        }

        public async Task<bool> AllStepsCompleteAsync(vm.Proposal proposal)
        {
            var workflowStep = proposal.WorkflowStep;
            var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter());
            var lastStepNumber = workflowTemplates.Max(x => x.StepNumber);

            var currentStepNumber = workflowTemplates
                .Where(x => x.StepName == workflowStep.StepName)
                .First()
                .StepNumber;

            return currentStepNumber == lastStepNumber;
        }

        public async Task<WorkflowStep> CreateNextStepAsync(vm.Proposal proposal)
        {
            var workflowStep = proposal.WorkflowStep;
            var workflowTemplates = await _capitalRequestServices.GetAllWorkflowTemplates(new WorkflowTemplateSearchFilter());

            var currentStepNumber = workflowTemplates
                .Where(x => x.StepName == workflowStep.StepName)
                .First()
                .StepNumber;

            var nextStepNumber = currentStepNumber + 1;

            var workflowTemplate = workflowTemplates
                .Where(x => x.StepNumber == nextStepNumber)
                .First();

            var nextWorkflowStep = _mapper.Map<WorkflowStep>(workflowTemplate);

            nextWorkflowStep.WorkflowStepID = proposal.NextWorkflowStepId;
            nextWorkflowStep.CreatedBy = proposal.Reviewer.UserId;

            return nextWorkflowStep;

        }
    }

}
