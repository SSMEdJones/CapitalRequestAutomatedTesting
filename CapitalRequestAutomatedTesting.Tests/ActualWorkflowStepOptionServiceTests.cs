using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.Tests;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
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
            var proposalId = 2936; // Example proposal ID
            var proposal = await _capitalRequestservices.GetProposal(proposalId);

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
            .Where(x => !x.IsComplete)
            .FirstOrDefault();

            proposal.RequestedInfo = (await _capitalRequestservices.GetAllRequestedInfos(new RequestedInfoSearchFilter { ProposalId = proposal.Id, IsOpen = true }))
                .FirstOrDefault();


            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID)).ToList();

            // Act
            var actual = await _service.GetReOpenedOptionsAsync(Constants.OPTION_TYPE_VERIFY, proposal);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(actual.First().ReviewerGroupId, proposal.RequestedInfo.RequestingReviewerGroupId);
        }

        [Fact]
        public async Task GetClosedOptionsAsync_ReturnsCorrectOptions()
        {
            // Arrange
            var proposalId = 2936; // Example proposal ID
            var optionType = "AddInfo";
            var reviewerEmail = "edward.jones@ssmhealth.com";
            var requestedInfoId = 691;
            //var workflowStepId = Guid.Parse("53E451AC-8057-F011-A31B-0050569736FD");

            var proposal = await _capitalRequestservices.GetProposal(proposalId);
            proposal.RequestedInfo = await _capitalRequestservices.GetRequestedInfo(requestedInfoId);
            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                     .Where(x => !x.IsComplete)
                     .FirstOrDefault();

            var workflowStepId = proposal.WorkflowStep.WorkflowStepID;

            proposal.WorkflowStepOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStep.WorkflowStepID))
                        .ToList();
            proposal.RequestingGroupId = proposal.RequestedInfo.RequestingReviewerGroupId;
            proposal.ReplyingGroupId = proposal.RequestedInfo.ReviewerGroupId;
            proposal.ReviewerGroupId = proposal.ReplyingGroupId;
            proposal.WorkflowStep = await _ssmWorkflowServices.GetWorkflowStep(workflowStepId);

            //};
            // Act
            var result = await _service.GetClosedWorkflowStepOptionsAsync(proposal, optionType, requestedInfoId);
            // Assert
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, x => x.OptionName == reviewerEmail);
            Assert.Contains(result, x => x.OptionName == "pamela.shumway@ssmhealth.com");
            Assert.Contains(result, x => x.OptionName == "takashi.fujimoto@ssmhealth.com");

        }

        [Fact]
        public async Task GetRequestTypeWorkflowStepOptionsAsync_ReturnsCorrectOptions()
        {
            /*
             *  var workflowStep = proposal.WorkflowStep;

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(workflowStep.WorkflowStepID))
                                .Where(x => x.ReviewerGroupId == proposal.RequestedInfo.ReviewerGroupId &&
                                       x.OptionType == Constants.OPTION_TYPE_ADD_INFO &&
                                       x.CreatedBy == proposal.Reviewer.UserId &&
                                       x.RequestedInfoId == proposal.RequestedInfo.Id
             */
            // Arrange
            var proposalId = 2936; // Example proposal ID
            //var optionType = Constants.OPTION_TYPE_ADD_INFO;
            var requestedInfoId = 723;
            //var workflowStepId = Guid.Parse("53E451AC-8057-F011-A31B-0050569736FD");
            var proposal = await _capitalRequestservices.GetProposal(proposalId);
            proposal.ReviewerId = 37841;
            proposal.Reviewer = await _capitalRequestservices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);
            proposal.RequestedInfo = await _capitalRequestservices.GetRequestedInfo(requestedInfoId);
            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                     .Where(x => !x.IsComplete)
                     .FirstOrDefault();


            //};
            // Act
            var result = await _service.GetRequestTypeWorkflowStepOptionsAsync(proposal);
            // Assert
            Assert.NotNull(result);

        }

    }
}
