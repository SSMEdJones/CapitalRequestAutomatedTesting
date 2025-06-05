using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

public class PredictiveWorkflowStepOptionServiceTests : IntegrationTestBase
{
    private readonly IPredictiveWorkflowStepOptionService _service;
    private readonly IPredictiveRequestedInfoService _requestedInfoService;

    private readonly ICapitalRequestServices _capitalRequestservices;
    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly IMapper _mapper;

    public PredictiveWorkflowStepOptionServiceTests()
    {
        _service = _provider.GetRequiredService<IPredictiveWorkflowStepOptionService>();
        _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
        _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
        _requestedInfoService = _provider.GetRequiredService<IPredictiveRequestedInfoService>();
        _mapper = _provider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void CloseWorkflowStepOptions_ShouldReturnWorkflowStepOptionList()
    {

        int proposalId = 2884;
        var proposal = _capitalRequestservices.GetProposal(proposalId).Result;

        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 


        var predicted = _service.CloseOptions(proposal, Guid.Empty, Constants.OPTION_TYPE_VERIFY, null, Constants.OPTION_TYPE_REQUEST);

        var actual = _service.GetFilteredOptions(proposal, Constants.OPTION_TYPE_VERIFY, null);

        Assert.Equal(predicted.Count, actual.Count); // Ensure both lists have the same number of items

        predicted.ForEach(pred =>
        {
            var match = actual.FirstOrDefault(act => act.OptionID == pred.OptionID);
            Assert.True(match != null, $"No match found in actual for OptionId {pred.OptionID}");

            Assert.True(pred.OptionName == match.OptionName,
                $"OptionName mismatch for OptionId {pred.OptionID}. Predicted: '{pred.OptionName}', Actual: '{match.OptionName}'");

            Assert.True(pred.ReviewerGroupId == match.ReviewerGroupId,
                $"ReviewerGroupId mismatch for OptionId {pred.OptionID}. Predicted: '{pred.ReviewerGroupId}', Actual: '{match.ReviewerGroupId}'");

            Assert.True(pred.IsComplete == match.IsComplete,
                $"IsComplete mismatch for OptionId {pred.OptionID}. Predicted: {pred.IsComplete}, Actual: {match.IsComplete}");

            Assert.True(pred.IsTerminate == match.IsTerminate,
                $"IsTerminate mismatch for OptionId {pred.OptionID}. Predicted: {pred.IsTerminate}, Actual: {match.IsTerminate}");

            Assert.True(pred.UpdatedBy == match.UpdatedBy,
                $"IsTerminate mismatch for OptionId {pred.OptionID}. Predicted: {pred.UpdatedBy}, Actual: {match.UpdatedBy}");
        });

        Debug.WriteLine($"WorkflowStepOption: {System.Text.Json.JsonSerializer.Serialize(predicted)}");
    }

    [Fact]
    public void CreateWorkflowStepOptions_ReturnsListofWorkflowStepOptions()
    {
        int proposalId = 2884;
        var proposal = _capitalRequestservices.GetProposal(proposalId).Result;
        proposal.ReviewerGroupId = 2;  // will come from selection of what button selected
        proposal.RequestedInfo.ReviewerGroupId = 3; //will come from drop down selection from what Group info requested 
        proposal.RequestedInfo.Id = _requestedInfoService.CreateRequestedInfo(proposal, 0).Id;

        var predicted = _service.CreateWorkflowStepOptions(proposal, Constants.EMAIL_REQUEST_MORE_INFORMATION, proposal.RequestedInfo.Id);

        var actual = _ssmWorkflowServices.GetAllWorkFlowStepOptions(predicted.First().WorkflowStepID).Result
            .Where(x => x.OptionType == Constants.OPTION_TYPE_ADD_INFO && 
                   x.RequestedInfoId == proposal.RequestedInfo.Id)
            .ToList();
        
        Assert.NotNull(actual);
        Assert.NotEmpty(actual);

        Assert.Equal(predicted.Count, actual.Count);

        predicted.ForEach(pred =>
        {
            var match = actual.FirstOrDefault(act => act.OptionName == pred.OptionName);
            Assert.True(match != null, $"No match found in actual for OptionName {pred.OptionName}");

            Assert.True(pred.ReviewerGroupId == match.ReviewerGroupId,
                $"ReviewerGroupId mismatch for OptionId {pred.OptionName}. Predicted: '{pred.ReviewerGroupId}', Actual: '{match.ReviewerGroupId}'");

            Assert.True(pred.IsComplete == match.IsComplete,
                $"IsComplete mismatch for OptionId {pred.OptionName}. Predicted: {pred.IsComplete}, Actual: {match.IsComplete}");

            Assert.True(pred.IsTerminate == match.IsTerminate,
                $"IsTerminate mismatch for OptionId {pred.OptionName}. Predicted: {pred.IsTerminate}, Actual: {match.IsTerminate}");

            Assert.True(pred.CreatedBy == match.CreatedBy,
                $"CreatedBy mismatch for OptionId {pred.OptionName}. Predicted: {pred.UpdatedBy}, Actual: {match.CreatedBy}");
        });

        Debug.WriteLine($"WorkflowStepOption: {System.Text.Json.JsonSerializer.Serialize(predicted)}");
    }
}

