using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;

public class PredictiveScenarioServiceTests : IntegrationTestBase
{
    private readonly IPredictiveScenarioService _service;
    private readonly ICapitalRequestServices _capitalRequestservices;
    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly IUserContextService _userContextService;

    public PredictiveScenarioServiceTests()
    {
        _service = _provider.GetRequiredService<IPredictiveScenarioService>();
        _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
        _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
        _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
        _userContextService = _provider.GetRequiredService<IUserContextService>();
    }


    [Fact]
    public async Task Should_Invoke_RequestMoreInfo_Methods_Dynamically_And_Return_Valid_Results()
    {
        var proposal = await _capitalRequestservices.GetProposal(2884);
        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 
        proposal.RequestedInfo.RequestingReviewerGroupId = 2;
        proposal.ReviewerId = 37798;

        proposal.Reviewer = await _capitalRequestservices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);
        var increment = 0;
        var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
            .Where(x => !x.IsComplete)
            .FirstOrDefault();

        var workflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                       .Where(x => x.IsComplete == false && x.IsTerminate == false)
                       .ToList();

        var email = _userContextService.Email;

        var methods = new List<PredictiveMethod>
        {
            new PredictiveMethod { ServiceName = "IPredictiveRequestedInfoService", MethodName = "CreateRequestedInfoAsync", Parameters = new List<object> { proposal, increment }, Operation = CrudOperationType.Insert},
            new PredictiveMethod { ServiceName = "IPredictiveWorkflowStepResponderService", MethodName = "CreateWorkflowStepResponderAsync", Parameters = new List<object> { proposal, Constants.RESPONDER_REQUEST}, Operation = CrudOperationType.Insert },
            new PredictiveMethod { ServiceName = "IPredictiveWorkflowStepOptionService", MethodName = "CloseOptionsAsync", Parameters = new List<object> { proposal, Guid.Empty, Constants.OPTION_TYPE_VERIFY, null, Constants.OPTION_TYPE_REQUEST}, Operation = CrudOperationType.Update },
            new PredictiveMethod { ServiceName = "IPredictiveWorkflowStepOptionService", MethodName = "CreateSubmitWorkflowStepOptionsAsync", Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION, proposal.RequestedInfo.Id } , Operation = CrudOperationType.Insert },
            new PredictiveMethod { ServiceName = "IPredictiveEmailNotificationService", MethodName = "CreateSubmitEmailNotificationsAsync", Parameters = new List<object> { proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION }, Operation = CrudOperationType.Insert }
        };

        var scenarioDetailViewModel = new ScenarioDetailsViewModel
        {
            ScenarioId = "SCN001",
            ProposalId = 2884
        };

        // Act
        var result = await _service.ExecuteScenarioMethodsAsync(methods, scenarioDetailViewModel);

        // Assert - Properly compare expected results
        Assert.NotNull(result);
        Assert.Equal(result.Tables.First().Key, "RequestedInfo");
        Assert.Equal(result.Tables.Skip(1).First().Key, "WorkflowStepResponder");
        Assert.Equal(result.Tables.Skip(2).First().Key, "WorkflowStepOption");
        Assert.Equal(result.Tables.Skip(2).First().Value.Records.Count, 2);
        Assert.Equal(result.Tables.Skip(3).First().Key, "EmailNotification");

        foreach (var tableEntry in result.Tables)
        {
            Debug.WriteLine($"Table: {tableEntry.Key}");

            foreach (var record in tableEntry.Value.Records)
            {
                Debug.WriteLine($"Action: {record.Operation}, Data: {JsonConvert.SerializeObject(record.Data)}");
            }
        }
    }


    [Fact]
    public async Task GenerateScenarioDataAsync_ReturnsPredictiveData()
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
        
        // Act
        var result = await _service.GenerateScenarioDataAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(scenario.ScenarioId, result.ScenarioId);
    }

}

