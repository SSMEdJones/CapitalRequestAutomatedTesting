using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
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
            var workflowStepId = Guid.Parse("53E451AC-8057-F011-A31B-0050569736FD");
            var expectedResponderEmail = "edward.jones@ssmhealth.com";

            var proposal = await _capitalRequestservices.GetProposal(proposalId);
            proposal.WorkflowStep = await _ssmWorkflowServices.GetWorkflowStep(workflowStepId);
            proposal.Reviewer = new vm.Reviewer { Email = expectedResponderEmail };

            // Act
            var result = await _service.GetWorkflowStepResponderAsync(proposal, responderType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workflowStepId, result.WorkflowStepID);
            Assert.Equal(expectedResponderEmail, result.Responder);
            Assert.Equal(responderType, result.ResponderType);
        }


    }
}
