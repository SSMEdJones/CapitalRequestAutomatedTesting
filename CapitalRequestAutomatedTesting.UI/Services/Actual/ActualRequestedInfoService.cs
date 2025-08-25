using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualRequestedInfoService
    {
    }
    public class ActualRequestedInfoService : IActualRequestedInfoService
    {
        private ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public ActualRequestedInfoService(ICapitalRequestServices capitalRequestServices, IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<RequestedInfo> GetRequestedInfoAsync(vm.Proposal proposal)
        {
            var requestedInfo = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
                RequestingReviewerGroupId = proposal.ReviewerGroupId,
                IsOpen = true
            }))
            .FirstOrDefault();

            return _mapper.Map<RequestedInfo>(requestedInfo);
        }

        public async Task<RequestedInfo> GetRequestedInfoByIdAsync(vm.Proposal proposal)
        {
            var requestedInfo = await _capitalRequestServices.GetRequestedInfo(proposal.RequestedInfoId);

            return _mapper.Map<RequestedInfo>(requestedInfo);
        }
    }
}
