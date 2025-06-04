using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

public class PredictiveWorkflowStepResponderServiceTests : IntegrationTestBase
{
    private readonly IPredictiveWorkflowStepResponderService _service;
    private readonly ICapitalRequestServices _capitalRequestservices;
    private readonly ISSMWorkflowServices _ssmWorkflowServices;

    public PredictiveWorkflowStepResponderServiceTests()
    {
        _service = _provider.GetRequiredService<IPredictiveWorkflowStepResponderService>();
        _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
        _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
    }

    [Fact]
    public void CreateWorkflowStepResponder_ShouldReturneWorkflowStepResponder()
    {

        int proposalId = 2884;
        var proposal = _capitalRequestservices.GetProposal(proposalId).Result;

        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 
        
        var predicted = _service.CreateWorkflowStepResponder(proposal, Constants.RESPONDER_REQUEST);

        var workflowSteps = _ssmWorkflowServices.GetAllWorkFlowSteps((Guid)proposal.WorkflowId)
            .Result;
           

        var workflowStep = workflowSteps.FirstOrDefault(x => !x.IsComplete);

        if (workflowStep == null)
        {
            throw new Exception("No workflow steps found for the given proposal.");
        }

        var workflowStepResponders = _ssmWorkflowServices.GetAllAddWorkFlowStepResponder(workflowStep.WorkflowStepID)
            .Result;

        var actual = workflowStepResponders.Where(x => x.ReviewerGroupId == predicted.ReviewerGroupId &&
                    x.WorkflowStepOptionID == predicted.WorkflowStepOptionID &&
                    x.ResponderType == predicted.ResponderType && 
                    x.Responder.ToLower() == predicted.Responder && 
                    x.CreatedBy == predicted.CreatedBy )
            .FirstOrDefault();

        Assert.NotNull(predicted);
        Assert.NotNull(actual);
        Assert.Equal(predicted.ReviewerGroupId, actual.ReviewerGroupId);
        Assert.Equal(predicted.WorkflowStepOptionID, actual.WorkflowStepOptionID);
        Assert.Equal(predicted.ResponderType, actual.ResponderType);
        Assert.Equal(predicted.CreatedBy, actual.CreatedBy);
        Assert.Equal(predicted.UpdatedBy, actual.UpdatedBy);

        //TestHelpers.AssertTimestampsClose(predicted.Created, actual.Created);
        //TestHelpers.AssertTimestampsClose(predicted.Updated, actual.Updated);

        Debug.WriteLine($"WorkflowStepResponser: {System.Text.Json.JsonSerializer.Serialize(predicted)}");
    }
}

