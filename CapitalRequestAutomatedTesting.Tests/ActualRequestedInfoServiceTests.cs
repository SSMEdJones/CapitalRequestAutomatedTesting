using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using Microsoft.Extensions.DependencyInjection;
using vm = CapitalRequest.API.Models;
using Xunit;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class ActualRequestedInfoServiceTests : IntegrationTestBase
    {
        private readonly IActualRequestedInfoService _service;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IMapper _mapper;

        public ActualRequestedInfoServiceTests()
        {
            _service = _provider.GetRequiredService<IActualRequestedInfoService>();
            _capitalRequestServices = _provider.GetRequiredService<ICapitalRequestServices>();
            _mapper = _provider.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task GetRequestedInfoAsync_ReturnsRequestedInfo_ForValidProposal()
        {
            // Arrange
            var proposalId = 2936; // Example proposal ID
            var reviewerGroupId = 3;
            var requestingReviewerGroupId = 2;

            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            proposal.ReviewerGroupId = reviewerGroupId;
            proposal.RequestingReviewerGroupId = requestingReviewerGroupId;

            // Act
            var result = await _service.GetRequestedInfoAsync(proposal);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.WorkflowStepOptionId);
        }

        [Fact]
        public async Task GetRequestedInfoByIdAsync_ReturnsRequestedInfo_ForValidRequestedInfoId()
        {
            // Arrange
            var proposalId = 2936; // Example proposal ID
            var requestedInfoId = 691; // Example requested info ID

            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            proposal.RequestedInfoId = requestedInfoId;

            // Act
            var result = await _service.GetRequestedInfoByIdAsync(proposal);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.WorkflowStepOptionId);
        }

        [Fact]
        public async Task GetRequestedInfoAsync_ReturnsNull_WhenNoMatchingRequestedInfo()
        {
            // Arrange
            var proposalId = 2935; // Example proposal ID
            var reviewerGroupId = 999; // Non-existent reviewer group
            var requestingReviewerGroupId = 999; // Non-existent requesting reviewer group

            var proposal = await _capitalRequestServices.GetProposal(proposalId);
            proposal.ReviewerGroupId = reviewerGroupId;
            proposal.RequestingReviewerGroupId = requestingReviewerGroupId;

            // Act
            var result = await _service.GetRequestedInfoAsync(proposal);

            // Assert
            Assert.Null(result);
        }
    }
}