using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Rollback
{
    public interface IRollbackRequestedInfoService
    {
    }
    public class RollbackRequestedInfoService : IRollbackRequestedInfoService
    {
        private ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public RollbackRequestedInfoService(ICapitalRequestServices capitalRequestServices, IMapper mapper)
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

        public async Task<RequestedInfo> OpenRequestedInfoById(vm.Proposal proposal)
        {
            var requestedInfo = await _capitalRequestServices.GetRequestedInfo(proposal.RequestedInfoId);
            if (requestedInfo == null)
            {
                throw new Exception("Requested Info not found.");
            }
            requestedInfo.IsOpen = true;

            requestedInfo = await _capitalRequestServices.UpdateRequestedInfos(requestedInfo);

            return _mapper.Map<RequestedInfo>(requestedInfo);
        }
    }
}
