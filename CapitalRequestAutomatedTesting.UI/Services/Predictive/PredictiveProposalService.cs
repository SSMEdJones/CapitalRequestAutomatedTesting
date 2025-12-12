using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveProposalService
    {

        Task<vm.Proposal> UpdateProposalAsync(vm.Proposal proposal);
    }
    public class PredictiveProposalService : IPredictiveProposalService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IMapper _mapper;

        public PredictiveProposalService(ICapitalRequestServices capitalRequestServices, IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }


        public async Task<vm.Proposal> UpdateProposalAsync(vm.Proposal proposal)
        {
            proposal.ProjectCENumber = CreateProjectNumber(proposal);
            proposal.WBSApproved = true;
            proposal.Updated = DateTime.Now;
            proposal.UpdatedBy = proposal.Reviewer.UserId;

            return proposal;

        }

        private string CreateProjectNumber(vm.Proposal proposal)
        {
            var wbs = proposal.WBSList.FirstOrDefault();
            var wbsNumber = _mapper.Map<vm.WbsNumber>(wbs);

            return $"{Constants.PROJECT_NUMBER_PREFIX}-{wbsNumber.CompanyCode}-{wbsNumber.CapitalPoolIdentifierShortName}-" +
                     $"{wbsNumber.CapitalPoolShortName}-{wbsNumber.CapitalFundingYearShortName}{wbsNumber.UniqueIdentifier}";
        }

    }
}
