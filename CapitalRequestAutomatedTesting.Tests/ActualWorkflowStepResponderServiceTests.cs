using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using Microsoft.Extensions.DependencyInjection;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
using vm = CapitalRequest.API.Models;

//

namespace CapitalRequestAutomatedTesting.Tests
{
    public class ActualWorkflowStepResponderServiceTests : IntegrationTestBase
    {
        private readonly IActualWorkflowStepResponderService _service;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestservices;
        private readonly IMapper _mapper;

        public ActualWorkflowStepResponderServiceTests()
        {
            _service = _provider.GetRequiredService<IActualWorkflowStepResponderService>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
            _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
            _mapper = _provider.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task GetWorkflowStepResponderAsync_ReturnsCorrectResponder()
        {
            // Arrange
            var proposalId = 2936; // Example proposal ID
            var responderType = "AddInfo"; // Example responder type
            var expectedResponderEmail = "edward.jones@ssmhealth.com";

            var proposal = await _capitalRequestservices.GetProposal(proposalId);
            proposal.Reviewer = new vm.Reviewer { Email = expectedResponderEmail };

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                     .Where(x => !x.IsComplete)
                     .FirstOrDefault();

            var workflowStepId = proposal.WorkflowStep.WorkflowStepID;

            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID))
                        .ToList();
            proposal.RequestingGroupId = 2;
            //proposal.ReplyingGroupId = 3;
            proposal.ReviewerGroupId = 3;
            // Act
            var result = await _service.GetWorkflowStepResponderAsync(proposal, responderType, Constants.OPTION_TYPE_ADD_INFO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workflowStepId, result.WorkflowStepID);
            Assert.Equal(expectedResponderEmail, result.Responder);
            Assert.Equal(responderType, result.ResponderType);
        }

        [Fact]
        public async Task GetWorkflowStepResponderAsyncRequest_ReturnsCorrectResponder()
        {
            // Arrange
            var proposalId = 2936; // Example proposal ID
            var responderType = Constants.ACTION_TYPE_REQUEST;

            var proposal = await _capitalRequestservices.GetProposal(proposalId);
            proposal.ExecutionDurationMinutes = 30;
            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                     .Where(x => !x.IsComplete)
                     .FirstOrDefault();

            proposal.Reviewer = await _capitalRequestservices.GetReviewer(37841); // Travis
            var workflowStepId = proposal.WorkflowStep.WorkflowStepID;

            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID))
                        .ToList();
            proposal.RequestingGroupId = 2;
            proposal.ReviewerGroupId = 3;
            // Act
            var result = await _service.GetWorkflowStepResponderAsync(proposal, responderType, Constants.OPTION_TYPE_VERIFY);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workflowStepId, result.WorkflowStepID);
            Assert.Equal(responderType, result.ResponderType);
        }


    }
}
