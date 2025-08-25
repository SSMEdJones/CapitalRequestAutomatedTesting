using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualProvidedInfoService
    {
    }
    public class ActualProvidedInfoService : IActualProvidedInfoService
    {
        private ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public ActualProvidedInfoService(ICapitalRequestServices capitalRequestServices, IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<ProvidedInfo> GetProvidedInfoAsync(vm.Proposal proposal)
        {
            var requestedInfo = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
                RequestingReviewerGroupId = proposal.ReviewerGroupId,
                IsOpen = true
            }))
            .FirstOrDefault();

            var providedInfo = (await _capitalRequestServices.GetAllProvidedInfos(new ProvidedInfoSearchFilter
            {
                RequestedInfoId = requestedInfo.Id
            }))
            .FirstOrDefault();

            return _mapper.Map<ProvidedInfo>(providedInfo);
        }
    }
}
