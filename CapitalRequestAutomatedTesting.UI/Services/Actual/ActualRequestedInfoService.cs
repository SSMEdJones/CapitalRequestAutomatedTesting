using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using System.Diagnostics;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualRequestedInfoService
    {
        Task<RequestedInfo> GetRequestedInfoAsync(vm.Proposal proposal, bool isOpen);
        Task<RequestedInfo> GetRequestedInfoByIdAsync(vm.Proposal proposal);
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

        public async Task<RequestedInfo> GetRequestedInfoAsync(vm.Proposal proposal, bool isOpen)
        {

            Debug.WriteLine($"In GetRequestedInfoAsync {Environment.NewLine} ProposalId: {proposal.Id} " +
                $"{ Environment.NewLine} VerifyingGroupId: {proposal.ReviewerGroupId} " +
                $"{Environment.NewLine} RequestingReviewerGroupId: {proposal.RequestingGroupId}" + 
                $"{Environment.NewLine} IsOpen: {isOpen}" );

            var requestedInfo = (await _capitalRequestServices.GetAllRequestedInfos(new RequestedInfoSearchFilter
            {
                ProposalId = proposal.Id,
                ReviewerGroupId = proposal.ReviewerGroupId,
                RequestingReviewerGroupId = proposal.RequestingGroupId,
                IsOpen = isOpen
            }))
            .FirstOrDefault();

            if (requestedInfo == null)
            {
                requestedInfo = new vm.RequestedInfo();
            }

            return _mapper.Map<RequestedInfo>(requestedInfo);
        }

        public async Task<RequestedInfo> GetRequestedInfoByIdAsync(vm.Proposal proposal)
        {
            var requestedInfo = await _capitalRequestServices.GetRequestedInfo(proposal.RequestedInfoId);

            return _mapper.Map<RequestedInfo>(requestedInfo);
        }
    }
}
