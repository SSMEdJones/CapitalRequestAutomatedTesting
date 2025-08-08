using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json;
using OpenQA.Selenium.BiDi.Modules.Script;
using SSMWorkflow.API.DataAccess.Models;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class PredictiveSeleniumServiceTests : IntegrationTestBase
    {
        private readonly IPredictiveSeleniumService _predictiveSeleniumService;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IUserContextService _userContextService;

        public PredictiveSeleniumServiceTests()
        {
            _predictiveSeleniumService = _provider.GetRequiredService<IPredictiveSeleniumService>();
            _capitalRequestServices = _provider.GetRequiredService<ICapitalRequestServices>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
            _userContextService = _provider.GetRequiredService<IUserContextService>();
        }

        //[Fact]
        //public async Task GenerateSeleniumOutcomeAsync_RequestMoreInfo_ReturnsValidOutcome()
        //{
        //    // Arrange
        //    var proposal = await _capitalRequestServices.GetProposal(2884);
        //    proposal.ReviewerGroupId = 2;  // Requesting group (e.g., IT Review)
        //    proposal.RequestedInfo.ReviewerGroupId = 3; // Target group (e.g., Facilities)
        //    proposal.RequestedInfo.RequestingReviewerGroupId = 2;
        //    proposal.ReviewerId = 37798;
        //    proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId);

        //    var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId))
        //        .Where(x => !x.IsComplete)
        //        .FirstOrDefault();

        //    var scenarioDetailViewModel = new ScenarioDetailsViewModel
        //    {
        //        ScenarioId = "SCN001", // Request More Info scenario
        //        ProposalId = proposal.Id,
        //        RequestingGroupId = proposal.ReviewerGroupId,
        //        TargetGroupId = proposal.RequestedInfo.ReviewerGroupId,
        //        ReviewerId = proposal.ReviewerId,
        //        ReviewerEmail = proposal.Reviewer.Email,
        //        RequestedInformation = "This is a test request for more information from automated testing.",
        //    };

        //    // Act
        //    var result = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenarioDetailViewModel);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.NotNull(result.Expected);
        //    Assert.True(result.Success, $"Selenium outcome generation failed: {result.ErrorMessage}");
        //    Assert.NotEmpty(result.Expected.Steps);

        //    // Verify steps include expected actions for Request More Info scenario
        //    var steps = result.Expected.Steps;
        //    Assert.Contains(steps, s => s.Action.Contains("Navigate") || s.Action.Contains("Open"));
        //    Assert.Contains(steps, s => s.Action.Contains("Click") && s.Action.Contains("Verify"));

        //    // Log steps for debugging and rollback planning
        //    Debug.WriteLine("Generated Selenium Steps:");
        //    foreach (var step in steps)
        //    {
        //        Debug.WriteLine($"Step: {step.Action}, Success: {step.Result?.Success}, Message: {step.Result?.Message}");
        //        if (step.DataChanges != null && step.DataChanges.Any())
        //        {
        //            foreach (var change in step.DataChanges)
        //            {
        //                Debug.WriteLine($"  → Change: {change.EntityType}.{change.PropertyName} = {change.NewValue}");
        //            }
        //        }
        //    }

        //    // Additional assertions about expected outcome
        //    Assert.Equal("Request More Information", result.Expected.ScenarioName);
        //    Assert.True(result.Expected.CompletedSuccessfully);
        //}

        [Fact]
        public async Task GenerateSeleniumOutcomeAsync_ReplyToRequest_ReturnsValidOutcome()
        {
            // Arrange
            var scenario = new ScenarioDetailsViewModel
            {
                ProposalId = 2936,
                ScenarioId = "SCN002",
                PartialViewName = "_ReplyToRequest",
                DisplayText = "Reply to Request",
                RequestingGroupId = 4,
                ReplyingGroupId = 5,
                ReviewerId = 37807,
                RequestedInformation = "Supply Chain requesting more information from EPMO as Pam Shumway via Workflow Automated Testing - Request More Information Scenario.",
                ReturnedInformation = "EPMO replying to request for more information from Supply Chain as Gavin Harrell via Workflow Automated Testing - Reply to Request Scenario.",
                RequestedInfoId = 691,
                PredictiveCompletionStep = 0,
                CanExecuteActualSteps = true
            };
            
//            var json = @"
//{
//  ""ProposalId"": 2936,
//  ""ScenarioId"": ""SCN002"",
//  ""PartialViewName"": ""_ReplyToRequest"",
//  ""DisplayText"": ""Reply to Request"",
//  ""SelectedProperties"": { },
//  ""RequestingGroupId"": 4,
//  ""ReplyingGroupId"": 5,
//  ""TargetGroupId"": null,
//  ""ReviewerId"": 37807,
//  ""ReviewerEmail"": null,
//  ""ReviewerUserId"": null,
//  ""SequenceNumber"": 0,
//  ""RequestedInformation"": ""Supply Chain requesting more information from EPMO as Pam Shumway via Workflow Automated Testing - Request More Information Scenario."",
//  ""ReturnedInformation"": ""EPMO replying to request for more information from Supply Chain as Gavin Harrell via Workflow Automated Testing - Reply to Request Scenario."",
//  ""Message"": """",
//  ""RequestingGroups"": [],
//  ""SecondaryRequestingGroups"": [],
//  ""ReplyingGroups"": [],
//  ""TargetGroups"": [],
//  ""Reviewers"": [],
//  ""RequestCount"": 0,
//  ""PredictedSeleniumOutcome"": {
//    ""ScenarioId"": null,
//    ""Expected"": {
//      ""ScenarioName"": null,
//      ""Steps"": [],
//      ""Passed"": true,
//      ""Messages"": [],
//      ""Success"": false,
//      ""PredictiveCompletionStep"": 0,
//      ""PredictiveStopReason"": null
//    },
//    ""Actual"": null,
//    ""Comparison"": null,
//    ""PredictiveCompletionStep"": 0,
//    ""PredictiveStopReason"": null,
//    ""Success"": false
//  },
//  ""ActualSeleniumOutcome"": {
//    ""ScenarioId"": null,
//    ""Expected"": {
//      ""ScenarioName"": null,
//      ""Steps"": [],
//      ""Passed"": true,
//      ""Messages"": [],
//      ""Success"": false,
//      ""PredictiveCompletionStep"": 0,
//      ""PredictiveStopReason"": null
//    },
//    ""Actual"": null,
//    ""Comparison"": null,
//    ""PredictiveCompletionStep"": 0,
//    ""PredictiveStopReason"": null,
//    ""Success"": false
//  },
//  ""PredictiveData"": {
//    ""ScenarioId"": null,
//    ""ActualExecutionDurationMinutes"": null,
//    ""ActualExecutionDuration"": null,
//    ""Tables"": { }
//  },
//  ""OriginalData"": {
//    ""ScenarioId"": null,
//    ""ActualExecutionDurationMinutes"": null,
//    ""ActualExecutionDuration"": null,
//    ""Tables"": { }
//  },
//  ""ActualData"": {
//    ""ScenarioId"": null,
//    ""ActualExecutionDurationMinutes"": null,
//    ""ActualExecutionDuration"": null,
//    ""Tables"": { }
//  },
//  ""PredictiveCompletionStep"": 0,
//  ""PredictiveStopReason"": null,
//  ""CanExecuteActualSteps"": true,
//  ""RequestedInfoId"": 691
//}";

            //var scenario = JsonConvert.DeserializeObject<ScenarioDetailsViewModel>(json);

            // Act
            var result = await _predictiveSeleniumService.GenerateSeleniumOutcomeAsync(scenario); ;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Expected);
            //Assert.True(result.Success, $"Selenium outcome generation failed: {result.ErrorMessage}");
            Assert.NotEmpty(result.Expected.Steps);

            // Log steps for debugging
            Debug.WriteLine("Generated Selenium Steps for Reply to Request:");
            foreach (var step in result.Expected.Steps)
            {
                Debug.WriteLine($"Step: {step.Action}");
            }

            // Additional assertions about expected outcome
            Assert.Equal("Reply to Request", result.Expected.ScenarioName);
            //Assert.True(result.Expected.CompletedSuccessfully);
        }
    }
}