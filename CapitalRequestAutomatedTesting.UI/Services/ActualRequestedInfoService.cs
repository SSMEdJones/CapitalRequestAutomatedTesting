using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
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

        public async Task<dto.RequestedInfo> GetRequestedInfoAsync(vm.Proposal proposal)
        {
            var requestedInfo = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
                RequestingReviewerGroupId = proposal.ReviewerGroupId,
                IsOpen = true
            }))
            .FirstOrDefault();

            return _mapper.Map<dto.RequestedInfo>(requestedInfo);
        }
    }
}
