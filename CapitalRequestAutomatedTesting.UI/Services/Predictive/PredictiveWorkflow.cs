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
    public interface IPredictiveWorkflowService
    {
        Workflow CreateWorkflow(vm.Proposal proposal);
    }
    public class PredictiveWorkflowService : IPredictiveWorkflowService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;

        private readonly IMapper _mapper;

        public PredictiveWorkflowService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IUserContextService userContextService,
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _userContextService = userContextService;
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public Workflow CreateWorkflow(vm.Proposal proposal)
        {
            // Resolve WorkflowOptionId
            var workflow = _mapper.Map<Workflow>(proposal);

            workflow.StakeholderNotificationType = Constants.STAKE_HOLDER_NOTIFICATION_TYPE;
            workflow.CompleteMessage = Constants.COMPLETE_MESSAGE;
            workflow.CancelledMessage = Constants.CANCELLED_MESSAGE;
            workflow.ProjectReviewLink = _ssmWorkFlowSettings.ProjectReviewLink;

            return workflow;
        }

    }

}
