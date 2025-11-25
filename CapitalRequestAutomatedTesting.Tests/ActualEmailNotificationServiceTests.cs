#nullable disable
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using EmailNotification = SSMWorkflow.API.Models.EmailNotification;

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

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => !x.IsComplete)
                .FirstOrDefault();

            var nullField = "NULL";
            var emailQuery = $"EXECUTE dbo.GetCapitalRequestGroupNotifications {nullField}, {proposal.WorkflowStep.WorkflowStepID}, {nullField},{nullField},{nullField},{nullField},{nullField}";
            //EXECUTE dbo.GetCapitalRequestGroupNotifications NULL,'a664ac48-d4bf-f011-a31d-0050569736fd',NULL,NULL,NULL,NULL,NULL

            var scenario = new ScenarioDetailsViewModel
            {
                ScenarioId = "SCN004",
                ProposalId = proposalId,
                SubmitUserId = "tfujim"
            };
            scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);

            //var predictiveData = scenario.PredictiveData;
            //var tables = predictiveData.Tables;

            //// Make sure the table exists
            //if (tables.TryGetValue("EmailNotification", out var emailTable))
            //{
            //    // Get the first record's data and cast it
            //    var recordEntry = emailTable.Records.FirstOrDefault();
            //    Debug.WriteLine($"Data type: {recordEntry?.Data?.GetType().FullName}");

            //    var predictiveNotifications = recordEntry?.Data as List<SSMWorkflow.API.DataAccess.Models.EmailNotification>;
            //    if (predictiveNotifications != null)
            //    {
            //        foreach (var email in predictiveNotifications)
            //        {
            //            var emailQueryViewModel = new EmailQueryViewModel
            //            {
            //                WorkflowStepId = email.WorkflowStepId.ToString(),
            //                EmailTemplateId = "0", // or actual value
            //                ReviewerGroupId = email.ReviewerGroupId,
            //                Action = email.Action,
            //                OptionId = null,
            //                RequestedInfoId = null
            //            };

            //            // This sets EmailQuery and automatically updates EmailQueryDetails
            //            email.EmailQuery = emailQuery;
            //        }
            //    }
            //}

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                    .Where(x => !x.IsComplete)
                    .FirstOrDefault();

            var reviewerGroups = await _actualReviewerGroupService.GetFilteredReviewerGroupsAsync(Constants.STEP_ONE);
            proposal.ReviewerGroups = _actualReviewerGroupService.FilterReviewerGroups(reviewerGroups, proposal, Constants.STEP_ONE);
                
            // Act
            var actual = await _service.GetSubmitEmailNotificationsAsync(proposal);

            // Assert
            Assert.NotNull(actual);
        }

    }
}
