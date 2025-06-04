using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.Tests.Helpers;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
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
    public void CreateRequestedInfo_ShouldReturnRequestedInfo()
    {

        int proposalId = 2884;
        var proposal = _capitalRequestservices.GetProposal(proposalId).Result;

        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 

        var predicted = _service.CreateRequestedInfo(proposal, 0);

        var filter = new RequestedInfoSearchFilter
        {
            ProposalId = proposalId,
            ReviewerGroupId = predicted.ReviewerGroupId,
            RequestingReviewerId = predicted.RequestingReviewerId,
            WorkflowStepOptionId = predicted.WorkflowStepOptionId,
            IsOpen = predicted.IsOpen
        };

        var actual = _capitalRequestservices
            .GetAllRequestedInfos(filter)
            .Result
            .FirstOrDefault();

        Assert.NotNull(actual);
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

        Assert.NotNull(predicted);
        Debug.WriteLine($"RequestedInfo: {System.Text.Json.JsonSerializer.Serialize(predicted)}");
    }
}

