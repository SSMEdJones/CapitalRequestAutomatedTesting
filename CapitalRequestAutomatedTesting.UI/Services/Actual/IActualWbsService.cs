using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualWbsService
    {
        Task<List<vm.Wbs>> GetWBSNumbersAsync(vm.Proposal proposal);
    }
    public class ActualWbsService : IActualWbsService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;
        private IMapper _mapper;

        public ActualWbsService(
            ISSMWorkflowServices ssmWbsServices,
            ICapitalRequestServices capitalRequestServices,
            IMapper mapper)
        {
            _capitalRequestServices = capitalRequestServices;
            _mapper = mapper;
        }

        public async Task<List<vm.Wbs>> GetWBSNumbersAsync(vm.Proposal proposal)
        {
            var wbs = await _capitalRequestServices.GetAllWbss(
            new WbsSearchFilter
            {
                ProposalId = proposal.Id
            });

            return wbs.Select(x => _mapper.Map<vm.Wbs>(x)).ToList(); 
        }       
    }
}
