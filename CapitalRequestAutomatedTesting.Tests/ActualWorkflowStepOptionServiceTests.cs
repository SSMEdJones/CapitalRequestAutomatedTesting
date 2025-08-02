using AutoMapper;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
using vm = CapitalRequest.API.Models;



namespace CapitalRequestAutomatedTesting.Tests
{
    public class ActualWorkflowStepOptionServiceTests : IntegrationTestBase
    {


        private readonly IActualWorkflowStepOptionService _service;

        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestservices;
        private readonly IMapper _mapper;

        public ActualWorkflowStepOptionServiceTests()
        {
            _service = _provider.GetRequiredService<IActualWorkflowStepOptionService>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
            _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
            _mapper = _provider.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task GetReOpenedOptionsAsync_ReturnsUnlockedOptions_ExcludesCurrentReviewer()
        {
            // Arrange
            var proposalId = 2935; // Example proposal ID
            var requestingRevierId = 36993; // Example requesting reviewer ID
            var reviewerGroupId = 4;
            var optionType = "Verify";
            var reviewerEmail = "edward.jones@ssmhealth.com";
            var workflowStepId = Guid.Parse("53E451AC-8057-F011-A31B-0050569736FD");
            var options = new List<WorkFlowStepOptionViewModel>
        {
            new WorkFlowStepOptionViewModel
            {
                WorkflowStepID = workflowStepId,
                OptionID = Guid.Parse("87E451AC-8057-F011-A31B-0050569736FD"),
                OptionName = "pamela.shumway@ssmhealth.com",
                IsTerminate = false,
                ReviewerGroupId = reviewerGroupId,
                OptionType = optionType,
                Created = DateTime.Parse("2025-07-02 15:11:07.807"),
                Updated = null,
                UpdatedBy = null
            },
            new WorkFlowStepOptionViewModel
            {
                WorkflowStepID = workflowStepId,
                OptionID = Guid.Parse("88E451AC-8057-F011-A31B-0050569736FD"),
                OptionName = "takashi.fujimoto@ssmhealth.com",
                IsTerminate = false,
                ReviewerGroupId = reviewerGroupId,
                OptionType = optionType,
                Created = DateTime.Parse("2025-07-02 15:11:07.883"),
                Updated = DateTime.Parse("2025-08-01 14:19:37.007"),
                UpdatedBy = "ejones08"

            },
            new WorkFlowStepOptionViewModel
            {
                WorkflowStepID = workflowStepId,
                OptionID = Guid.Parse("89E451AC-8057-F011-A31B-0050569736FD"),
                OptionName = "takashi.fujimoto@ssmhealth.com",
                IsTerminate = false,
                ReviewerGroupId = reviewerGroupId,
                OptionType = optionType,
                Created = DateTime.Parse("2025-07-02 15:11:07.833"),
                Updated = DateTime.Parse("2025-08-01 14:19:37.050"),
                UpdatedBy = "ejones08"
            }
        };

            var proposal = await _capitalRequestservices.GetProposal(proposalId);

            proposal.RequestedInfo = new vm.RequestedInfo { RequestingReviewerGroupId = reviewerGroupId };
            proposal.Reviewer = new vm.Reviewer { Email = reviewerEmail };
            proposal.WorkflowStepOptions = options;
            proposal.RequestedInfo.RequestingReviewerId = requestingRevierId;


            // Act
            var result = await _service.GetReOpenedOptionsAsync(optionType, proposal);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, x => x.OptionName == reviewerEmail);
            Assert.Contains(result, x => x.OptionName == "edward.jones@ssmhealth.com");
            Assert.Contains(result, x => x.OptionName == "takashi.fujimoto@ssmhealth.com");
        }

    }
}
