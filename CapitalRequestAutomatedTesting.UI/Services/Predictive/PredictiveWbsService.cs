using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWbsService
    {
        Task<List<vm.Wbs>> CreateWBSNumbersAsync(vm.Proposal proposal);
        Task<List<vm.Wbs>> UpdateWBSNumbersAsync(vm.Proposal proposal);

    }

    public class PredictiveWbsService : IPredictiveWbsService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;


        public PredictiveWbsService(ICapitalRequestServices capitalRequestServices)
        {
            _capitalRequestServices = capitalRequestServices;
        }

        public async Task<List<vm.Wbs>> CreateWBSNumbersAsync(vm.Proposal proposal)
        {
            proposal.WBSList = await _capitalRequestServices.GetAllWbss(
            new WbsSearchFilter
            {
                ProposalId = proposal.Id
            });

            var wbsWithNumbers = await GenerateWBSNumbersAsync(proposal);

            wbsWithNumbers.ForEach(x =>
            {
                x.Updated = DateTime.Now;
                x.UpdatedBy = proposal.Reviewer.UserId;
            });

            return wbsWithNumbers;

        }
        public async Task<List<vm.Wbs>> UpdateWBSNumbersAsync(vm.Proposal proposal)
        {
            proposal.WBSList = await _capitalRequestServices.GetAllWbss(
            new WbsSearchFilter
            {
                ProposalId = proposal.Id
            });

            proposal.WBSList.ForEach(x =>
            {
                x.Updated = DateTime.Now;
                x.UpdatedBy = proposal.Reviewer.UserId;
            });

            return proposal.WBSList;

        }
        public async Task<List<vm.Wbs>> GenerateWBSNumbersAsync(vm.Proposal proposal)
        {
            var proposalId = proposal.Id;
            var proposals = await _capitalRequestServices.GetAllProposals(new ProposalSearchFilter { CapitalFundingYear = proposal.CapitalFundingYear });
            var proposalIds = proposals.Select(p => p.Id).ToList();
            var allWBS = (await _capitalRequestServices.GetAllWbss(new WbsSearchFilter()))
                .Where(x => proposalIds.Contains(x.ProposalId))
                .ToList();


            var projectTypes = await _capitalRequestServices.GetAllProjectTypes();
            var capitalPoolIdentifiers = await _capitalRequestServices.GetAllCapitalPoolIdentifiers();
            var capitalPools = await _capitalRequestServices.GetAllCapitalPools();

            var query =
                from w in allWBS
                join p in proposals on w.ProposalId equals p.Id
                join t in projectTypes on w.TypeOfProject equals t.Id
                join cpi in capitalPoolIdentifiers on p.CapitalPoolIdentifiers equals cpi.Id
                join cp in capitalPools on p.CapitalPool equals cp.Id
                where w.ProposalId == proposalId
                let componentCount = (
                    from x in allWBS
                    where x.ProposalId == proposalId && x.TypeOfProject == w.TypeOfProject
                    orderby x.Id
                    select x).ToList().IndexOf(w) + 1 // ROW_NUMBER equivalent
                let uniqueId = GetWBSUniqueId(p.CapitalFundingYear, proposalId, allWBS, proposals)
                select new
                {
                    w.Id,
                    w.ProposalId,
                    SortOrder = proposal.WBSList.OrderBy(y => y.Id).ToList().IndexOf(w) + 1,
                    w.TypeOfProject,
                    p.CapitalPool,
                    p.CapitalPoolIdentifiers,
                    WBSNumber = string.Join("-",
                        t.ShortName,
                        p.CompanyCode,
                        cpi.ShortName,
                        cp.ShortName,
                        p.CapitalFundingYear.ToString().Substring(2, 2) + uniqueId,
                        componentCount.ToString().PadLeft(2, '0'))
                };

            var newWBSList = query.ToList();

            var wbsList = new List<vm.Wbs>();

            foreach (var wbs in proposal.WBSList)
            {
                if (!string.IsNullOrEmpty(wbs.WbsNumber))
                {
                    // Existing WBSNumber, skip
                    wbsList.Add(wbs);
                    continue;
                }
                var newWbsEntry = newWBSList.FirstOrDefault(x => x.Id == wbs.Id);

                if (newWbsEntry != null)
                {
                    wbs.WbsNumber = newWbsEntry.WBSNumber;
                }

                wbsList.Add(wbs);
            }

            return wbsList;
        }

        public static string GetWBSUniqueId(
            int capitalFundingYear,
            int proposalId,
            IEnumerable<vm.Wbs> wbsList,
            IEnumerable<vm.Proposal> proposals)
        {

            // Find max UniqueID from existing WBSNumbers for same funding year, excluding current proposal
            var maxUniqueId = (from w in wbsList
                               join p in proposals on w.ProposalId equals p.Id
                               where p.CapitalFundingYear == capitalFundingYear
                                     && p.Id != proposalId
                                     && !string.IsNullOrEmpty(w.WbsNumber)
                               let uniqueId = int.TryParse(
                                   w.WbsNumber?.Substring(14, 4), out var val) ? val : 0
                               select uniqueId).DefaultIfEmpty(0).Max();

            int nextId = maxUniqueId + 1;

            // Pad to 4 digits
            return nextId.ToString().PadLeft(4, '0');
        }
    }

}
