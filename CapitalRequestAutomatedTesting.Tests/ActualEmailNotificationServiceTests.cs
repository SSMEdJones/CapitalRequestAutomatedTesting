using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using vm = CapitalRequest.API.Models;
using Xunit;
using CapitalRequestAutomatedTesting.UI.Models;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class ActualEmailNotificationService : IntegrationTestBase
    {
        private readonly IActualEmailNotificationService _service;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IMapper _mapper;

        public ActualEmailNotificationService()
        {
            _service = _provider.GetRequiredService<IActualEmailNotificationService>();
            _capitalRequestServices = _provider.GetRequiredService<ICapitalRequestServices>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();

            _mapper = _provider.GetRequiredService<IMapper>();
        }

        
        [Fact]
        public async Task GetEmailNotificationsAsync_WithValidData_ReturnsCorrectEmailNotifications()
        {

            // Arrange
            int proposalId = 2936;
            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            proposal.RequestedInfoId = 720;
            proposal.RequestedInfo = await _capitalRequestServices.GetRequestedInfo(proposal.RequestedInfoId);

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();

            var requestingUser = (await _capitalRequestServices.GetReviewer(proposal.RequestedInfo.RequestingReviewerId)).FullName;

            proposal.Reviewer = await _capitalRequestServices.GetReviewer(15251);
            proposal.ReviewerGroupId = 3;
            proposal.RequestingGroupId = 2;
            proposal.ExecutionDurationMinutes = 2000;
            proposal.RequestedInfoId = 720;
            // Act
            var actual = await _service.GetEmailNotificationsAsync(proposal, Constants.EMAIL_PROVIDE_MORE_INFORMATION, requestingUser);

            // Assert
            Assert.NotNull(actual);
        }

    }
}