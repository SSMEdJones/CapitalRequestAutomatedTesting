using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

public class PredictiveRequestedInfoServiceTests : IntegrationTestBase
{
    private readonly IPredictiveRequestedInfoService _service;
    private readonly ICapitalRequestServices _capitalRequestservices;

    public PredictiveRequestedInfoServiceTests()
    {
        _service = _provider.GetRequiredService<IPredictiveRequestedInfoService>();
        _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
    }

    [Fact]
    public async Task CreateRequestedInfoAsync_ShouldReturnRequestedInfo()
    {

        int proposalId = 2884;
        var proposal = await _capitalRequestservices.GetProposal(proposalId);

        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 

        var predicted = await _service.CreateRequestedInfoAsync(proposal, 0);

        var filter = new RequestedInfoSearchFilter
        {
            ProposalId = proposalId,
            ReviewerGroupId = predicted.ReviewerGroupId,
            RequestingReviewerId = predicted.RequestingReviewerId,
            WorkflowStepOptionId = predicted.WorkflowStepOptionId,
            IsOpen = predicted.IsOpen
        };

        var actual = (await _capitalRequestservices
            .GetAllRequestedInfos(filter))
            .FirstOrDefault();

        Assert.NotNull(actual);
        Assert.NotNull(predicted);
        Assert.Equal(predicted.Id, actual.Id);
        Assert.Equal(predicted.ProposalId, actual.ProposalId);
        Assert.Equal(predicted.RequestingReviewerGroupId, actual.RequestingReviewerGroupId);
        Assert.Equal(predicted.RequestingReviewerId, actual.RequestingReviewerId);
        Assert.Equal(predicted.ReviewerGroupId, actual.ReviewerGroupId);
        Assert.Equal(predicted.WorkflowStepOptionId, actual.WorkflowStepOptionId);
        Assert.Equal(predicted.IsOpen, actual.IsOpen);
        Assert.Equal(predicted.CreatedBy, actual.CreatedBy);
        Assert.Equal(predicted.UpdatedBy, actual.UpdatedBy);

        //Assert.Equal(predicted.Action, actual.Action);

        //TestHelpers.AssertTimestampsClose(predicted.Created, actual.Created);
        //TestHelpers.AssertTimestampsClose(predicted.Updated, actual.Updated);

        Debug.WriteLine($"RequestedInfo: {System.Text.Json.JsonSerializer.Serialize(predicted)}");
    }

    [Fact]
    public async Task GetRequestedInfo_ShouldReturnRequestedInfo()
    {

        int proposalId = 2884;
        var proposal = _capitalRequestservices.GetProposal(proposalId).Result;

        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 

        var predicted = await _service.CreateRequestedInfoAsync(proposal, 0);

        var filter = new RequestedInfoSearchFilter
        {
            ProposalId = proposalId,
            ReviewerGroupId = predicted.ReviewerGroupId,
            RequestingReviewerId = predicted.RequestingReviewerId,
            WorkflowStepOptionId = predicted.WorkflowStepOptionId,
            IsOpen = predicted.IsOpen
        };

        var actual = (await _capitalRequestservices
            .GetAllRequestedInfos(filter))
            .FirstOrDefault();

        Assert.NotNull(actual);
        Assert.NotNull(predicted);
        Assert.Equal(predicted.Id, actual.Id);
        Assert.Equal(predicted.ProposalId, actual.ProposalId);
        Assert.Equal(predicted.RequestingReviewerGroupId, actual.RequestingReviewerGroupId);
        Assert.Equal(predicted.RequestingReviewerId, actual.RequestingReviewerId);
        Assert.Equal(predicted.ReviewerGroupId, actual.ReviewerGroupId);
        Assert.Equal(predicted.WorkflowStepOptionId, actual.WorkflowStepOptionId);
        Assert.Equal(predicted.IsOpen, actual.IsOpen);
        Assert.Equal(predicted.CreatedBy, actual.CreatedBy);
        Assert.Equal(predicted.UpdatedBy, actual.UpdatedBy);

        //Assert.Equal(predicted.Action, actual.Action);

        //TestHelpers.AssertTimestampsClose(predicted.Created, actual.Created);
        //TestHelpers.AssertTimestampsClose(predicted.Updated, actual.Updated);

        Debug.WriteLine($"RequestedInfo: {System.Text.Json.JsonSerializer.Serialize(predicted)}");
    }
}

