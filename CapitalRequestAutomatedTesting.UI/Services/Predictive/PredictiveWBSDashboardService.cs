using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveWBSDashboardService
    {
        Task<SeleniumStepResult> ValidateDashboardStatusAsync(vm.Proposal proposal);

    }
    public class PredictiveWBSDashboardService : IPredictiveWBSDashboardService
    {
        private readonly ICapitalRequestServices _capitalRequestServices;

        public PredictiveWBSDashboardService(ICapitalRequestServices capitalRequestServices)
        {
            _capitalRequestServices = capitalRequestServices;
        }


        public async Task<List<vm.Wbs>> GetWBSDashboardDataByIdAsync(vm.Proposal proposal)
        {
            var dashboardData = await _capitalRequestServices.GetAllWbss(new CapitalRequest.API.DataAccess.Models.WbsSearchFilter
            { ProposalId = proposal.Id });

            return dashboardData;
        }

        public async Task<SeleniumStepResult> ValidateDashboardStatusAsync(vm.Proposal proposal)
        {
            try
            {
                var dashboardData = await GetWBSDashboardDataByIdAsync(proposal);
                if (dashboardData == null)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"{proposal.Id} remains on WBS Dashboard"
                    };
                }
                else
                {
                    return new SeleniumStepResult
                    {
                        Success = true,
                        Message = $"{proposal.Id} no longer on WBS Dashboard"
                    };
                }
            }
            catch (Exception ex)
            {
                return new SeleniumStepResult
                {
                    Success = false,
                    Message = $"Error validating WBS Dashboard status for Proposal ID {proposal.Id}: {ex.Message}"
                };


            }
        }

    }

}
