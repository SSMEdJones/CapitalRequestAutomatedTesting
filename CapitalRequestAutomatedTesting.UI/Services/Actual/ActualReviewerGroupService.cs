using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Enums;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using System.Diagnostics;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualReviewerGroupService
    {
        Task<List<vm.ReviewerGroup>> GetFilteredReviewerGroupsAsync(int stepNumber);
        List<vm.ReviewerGroup> FilterReviewerGroups(List<vm.ReviewerGroup> reviewerGroups, vm.Proposal proposal, int stepNumber);
    }
    public class ActualReviewerGroupService : IActualReviewerGroupService
    {
        private ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public ActualReviewerGroupService(ICapitalRequestServices capitalRequestServices, IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<List<vm.ReviewerGroup>> GetFilteredReviewerGroupsAsync(int stepNumber)
        {
            var filter = new ReviewerGroupSearchFilter
            {
                StepNumber = stepNumber
            };

            return await _capitalRequestServices.GetAllReviewerGroups(filter);

        }

        public List<vm.ReviewerGroup> FilterReviewerGroups(List<vm.ReviewerGroup> reviewerGroups, vm.Proposal proposal, int stepNumber)
        {
            if (proposal.ReviewerGroupId == 0 || stepNumber == Constants.STEP_SIX)
            {

                return reviewerGroups
                        .Where(reviewerGroup =>
                            reviewerGroup.StepNumber == stepNumber &&
                            (stepNumber == Constants.STEP_SIX && reviewerGroup.Name != Constants.PURCHASING_GROUP) ||
                            !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                            (reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments) ||
                            (reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup))
                        )
                        .ToList();
            }
            else
            {

                return reviewerGroups
                    .Where(reviewerGroup =>
                        reviewerGroup.Id != proposal.ReviewerGroupId &&
                        !(reviewerGroup.Name == Constants.EPMO_GROUP && proposal.IsProjectManagerDesired == (int)ProjectManagerDesired.No ||
                         (reviewerGroup.Name == Constants.ADMIN_GROUP && !proposal.AffectsMultipleSegments) ||
                         (reviewerGroup.Name == Constants.PURCHASING_GROUP && !proposal.IncludePurchasingGroup))
                    )
                    .ToList();
            }
        }

    }
}
