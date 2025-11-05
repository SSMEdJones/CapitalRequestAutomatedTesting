using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using Microsoft.Extensions.Options;
using SSMWorkflow.API.DataAccess.ConfigurationSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWorkflowInstanceService
    {
        Task<WorkflowInstance> CreateWorkflowInstanceAsync(vm.Proposal proposal);
    }
    public class PredictiveWorkflowInstanceService : IPredictiveWorkflowInstanceService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IPredictiveWorkflowStepService _predictiveWorkflowStepService;

        private readonly IMapper _mapper;

        public PredictiveWorkflowInstanceService(
            ICapitalRequestServices capitalRequestServices,
            IPredictiveWorkflowStepService predictiveWorkflowStepService,
            IMapper mapper)
        {
            _predictiveWorkflowStepService = predictiveWorkflowStepService;
            _mapper = mapper;
        }

        public async Task<WorkflowInstance> CreateWorkflowInstanceAsync(vm.Proposal proposal)
        {
            var workflowStep = await _predictiveWorkflowStepService.CreateWorkflowStepAsync(proposal);
            var workflowInstance = _mapper.Map<WorkflowInstance>(workflowStep);

            return workflowInstance;
        }

    }

}
