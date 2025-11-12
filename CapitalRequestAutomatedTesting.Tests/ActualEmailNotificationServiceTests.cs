#nullable disable
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class ActualEmailNotificationService : IntegrationTestBase
    {
        private readonly IActualEmailNotificationService _service;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IActualReviewerGroupService _actualReviewerGroupService;
        private readonly IPredictiveScenarioService _predictiveScenarioService;


        public ActualEmailNotificationService()
        {
            _service = _provider.GetRequiredService<IActualEmailNotificationService>();
            _capitalRequestServices = _provider.GetRequiredService<ICapitalRequestServices>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
            _actualReviewerGroupService = _provider.GetRequiredService<IActualReviewerGroupService>();
            _predictiveScenarioService = _provider.GetRequiredService<IPredictiveScenarioService>();

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

        [Fact]
        public async Task GetSubmitEmailNotificationsAsync_WithValidData_ReturnsCorrectEmailNotifications()
        {

            // Arrange
            int proposalId = 2947;
            var proposal = await _capitalRequestServices.GetProposal(proposalId);

            var scenario = new ScenarioDetailsViewModel
            {
                ScenarioId = "SCN003",
                ProposalId = proposalId,
                SubmitUserId = "tfujim"
            };
            scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();

            var reviewerGroups = await _actualReviewerGroupService.GetFilteredReviewerGroupsAsync(Constants.STEP_ONE);
            proposal.ReviewerGroups = _actualReviewerGroupService.FilterReviewerGroups(reviewerGroups, proposal, Constants.STEP_ONE);
                
            // Act
            var actual = await _service.GetSubmitEmailNotificationsAsync(proposal, scenario);

            // Assert
            Assert.NotNull(actual);
        }

    }
}
