using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using SSMWorkflow.API.DataAccess.Models;
using System.Diagnostics;


public class PredictiveEmailNotificationServiceTests : IntegrationTestBase
{
    private readonly IPredictiveEmailNotificationService _service;
    private readonly IPredictiveRequestedInfoService _requestedInfoService;
    private readonly IActualEmailNotificataionService _actualEmailNotificationService;
    private readonly ICapitalRequestServices _capitalRequestservices;
    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly IMapper _mapper;

    public PredictiveEmailNotificationServiceTests()
    {
        _service = _provider.GetRequiredService<IPredictiveEmailNotificationService>();
        _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
        _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
        _requestedInfoService = _provider.GetRequiredService<IPredictiveRequestedInfoService>();
        _actualEmailNotificationService = _provider.GetRequiredService<IActualEmailNotificataionService>();
        _mapper = _provider.GetRequiredService<IMapper>();
    }
    
    [Fact]
    public async Task CreateEmailNotificationsAsync_ShouldReturnExpectedResults()
    {
        int proposalId = 2884;
        var proposal = await _capitalRequestservices.GetProposal(proposalId);

        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 
        proposal.RequestedInfo = _mapper.Map<CapitalRequest.API.Models.RequestedInfo>(await _requestedInfoService.CreateRequestedInfoAsync(proposal, 0));

        var predicted = await _service.CreateEmailNotificationsAsync(proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION);

        var workflowSteps = await _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId);
        var workflowStep = _mapper.Map<WorkflowStep>(workflowSteps.FirstOrDefault(x => !x.IsComplete));

        var allNotifications = await _ssmWorkflowServices.GetAllEmailNotifications(new EmailNotificationSearchFilter { WorkflowStepId = workflowStep.WorkflowStepID });
        var emailTemplate = await _capitalRequestservices.GetAllEmailTemplates(new EmailTemplateSearchFilter { Name = Constants.EMAIL_REQUEST_MORE_INFORMATION });
        var emailTemplateId = emailTemplate.FirstOrDefault()?.Id ?? 0;

        var actual = _actualEmailNotificationService.FilterEmailNotifications(allNotifications, emailTemplateId, proposal.RequestedInfo.ReviewerGroupId, proposal.RequestedInfo.Id);

        Assert.NotNull(predicted);
        Assert.NotEmpty(actual);

        Assert.Equal(predicted.Count, actual.Count);

        predicted.ForEach(pred =>
        {
            var match = actual.FirstOrDefault(act => act.Recipients == pred.Recipients);
            Assert.True(match != null, $"No match found in actual for Recipients {pred.Recipients}");

            Assert.True(pred.WorkflowName == match.WorkflowName,
                $"WorkflowName mismatch for Recipients {pred.Recipients}. Predicted: {pred.WorkflowName}, Actual: {match.WorkflowName}");

            Assert.True(pred.WorkflowDescription == match.WorkflowDescription,
                $"WorkflowDescription mismatch for Recipients {pred.Recipients}. Predicted: {pred.WorkflowDescription}, Actual: {match.WorkflowDescription}");

            Assert.True(pred.WorkflowState == match.WorkflowState,
                $"WorkflowState mismatch for Recipients {pred.Recipients}. Predicted: {pred.WorkflowState}, Actual: {match.WorkflowState}");
            
            Assert.True(pred.StepName == match.StepName,
                $"StepName mismatch for Recipients {pred.Recipients}. Predicted: {pred.StepName}, Actual: {match.StepName}");

            Assert.True(pred.StepDescription == match.StepDescription,
                $"StepDescription mismatch for Recipients {pred.Recipients}. Predicted: {pred.StepDescription}, Actual: {match.StepDescription}");

            Assert.True(pred.Action == match.Action,
                $"Action mismatch for Recipients {pred.Recipients}. Predicted: {pred.Action}, Actual: {match.Action}");

            // Normalize HTML before comparing
            var normalizedPredHtml = _actualEmailNotificationService.NormalizeHtml(pred.EmailMessage);
            var normalizedActualHtml = _actualEmailNotificationService.NormalizeHtml(match.EmailMessage);

            Assert.True(normalizedPredHtml == normalizedActualHtml,
                $"EmailMessage mismatch for Recipients {pred.Recipients}.\nPredicted: {normalizedPredHtml}\nActual: {normalizedActualHtml}");

            Assert.True(pred.Subject == match.Subject,
                $"Subject mismatch for Recipients {pred.Recipients}. Predicted: {pred.Subject}, Actual: {match.Subject}");

            Assert.True(pred.Priority == match.Priority,
                $"Priority mismatch for Recipients {pred.Recipients}. Predicted: {pred.Priority}, Actual: {match.Priority}");
        });


        Debug.WriteLine($"EmailNotification: {System.Text.Json.JsonSerializer.Serialize(predicted)}");
    }
}

