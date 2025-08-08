using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Interfaces;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Rollback
{
    public interface IRollbackProvidedInfoService
    {
        Task<ProvidedInfo> DeleteProvidedInfoAsync(vm.Proposal proposal);
    }
    public class RollbackProvidedInfoService : IRollbackProvidedInfoService, IRollbackHandler
    {
    {
        private ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public RollbackProvidedInfoService(ICapitalRequestServices capitalRequestServices, IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<ProvidedInfo> DeleteProvidedInfoAsync(vm.Proposal proposal)
        {
            var providedInfo = new vm.ProvidedInfo();
            var requestedInfo = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                ReviewerGroupId = proposal.RequestedInfo.ReviewerGroupId,
                RequestingReviewerGroupId = proposal.ReviewerGroupId,
                IsOpen = true
            }))
            .FirstOrDefault();

            if (requestedInfo != null)
            {

                providedInfo = (await _capitalRequestServices.GetAllProvidedInfos(new ProvidedInfoSearchFilter
                {
                    RequestedInfoId = requestedInfo.Id
                }))
                .FirstOrDefault();

                if (providedInfo != null)
                {
                    await _capitalRequestServices.DeleteAllProvidedInfos(new ProvidedInfoSearchFilter { Id = providedInfo.Id });
                }
            }

            return _mapper.Map<ProvidedInfo>(providedInfo);
        }

        public async Task ExecuteRollbackAsync(vm.Proposal proposal)
        {
            await DeleteProvidedInfoAsync(proposal);
        }

        public Task<object> RollbackAsync(List<object> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
