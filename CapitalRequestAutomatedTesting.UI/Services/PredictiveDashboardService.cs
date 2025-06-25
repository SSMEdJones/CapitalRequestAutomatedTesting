using SSMWorkflow.API.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using AutoMapper;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequest.API.DataAccess.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;
using SSMWorkflow.API.DataAccess.Models;
using SSMAuthenticationCore.Models;
using CapitalRequest.API.DataAccess.Services.Api;
using System.ComponentModel.DataAnnotations;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using System.Net.WebSockets;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPredictiveDashboardService
    {
        Task<SeleniumStepResult> ValidateDashboardStatusAsync(vm.Proposal proposal, string requestingGroup, string targetGroup, string dashboardStatus);

    }
    public class PredictiveDashboardService : IPredictiveDashboardService
{
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public PredictiveDashboardService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardDataAsync(vm.Proposal proposal)
        {
            var dashboardData = await _ssmWorkflowServices.GetAllDashboards(new DashboardSearchFilter
            {
                CapitalFundingYear = proposal.CapitalFundingYear,
                HistoricalDataOnly = false
            });

            return dashboardData;
        }


        public async Task<SSMWorkflow.API.Models.Dashboard> GetDashboardDataByIdAsync(vm.Proposal proposal)
        {
            var dashboardData = (await _ssmWorkflowServices.GetAllDashboards(new DashboardSearchFilter
            {
                CapitalFundingYear = proposal.CapitalFundingYear,
                HistoricalDataOnly = false
            }))
            .Where(x => x.ReqId == proposal.Id)
            .FirstOrDefault();

            return dashboardData;
        }

        public async Task<SeleniumStepResult> ValidateDashboardStatusAsync(vm.Proposal proposal, string requestingGroup, string targetGroup, string dashboardStatus)
        {
            try
            {
                var dashboardData = await GetDashboardDataByIdAsync(proposal);
                if (dashboardData == null)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = "Dashboard data not found."
                    };
                }

                var groupPrefix = $"{requestingGroup}Review"; // e.g., "ITReview"
                var expectedDate = DateTime.Now.Date.ToShortDateString(); // Adjust to your desired format if needed

                var dateProp = dashboardData.GetType().GetProperty($"{groupPrefix}Date");
                var nameProp = dashboardData.GetType().GetProperty($"{groupPrefix}Name");
                var statusProp = dashboardData.GetType().GetProperty($"{groupPrefix}Status");

                if (dateProp == null || nameProp == null || statusProp == null)
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = $"One or more expected properties ({groupPrefix}Date/Name/Status) not found on dashboard model."
                    };
                }

                var reviewDate = dateProp.GetValue(dashboardData)?.ToString();
                var reviewName = nameProp.GetValue(dashboardData)?.ToString();
                var reviewStatus = statusProp.GetValue(dashboardData)?.ToString();

                var todayStr = DateTime.Now.ToShortDateString();
                var errors = new List<string>();

                if (!string.Equals(reviewDate, todayStr, StringComparison.OrdinalIgnoreCase))
                    errors.Add($"Expected review date '{todayStr}', but got '{reviewDate}'.");

                if (!string.Equals(reviewName, targetGroup, StringComparison.OrdinalIgnoreCase))
                    errors.Add($"Expected reviewer name '{targetGroup}', but got '{reviewName}'.");

                if (!string.Equals(reviewStatus, dashboardStatus, StringComparison.OrdinalIgnoreCase))
                    errors.Add($"Expected status '{dashboardStatus}', but got '{reviewStatus}'.");

                if (errors.Any())
                {
                    return new SeleniumStepResult
                    {
                        Success = false,
                        Message = "Dashboard status mismatch:\n" + string.Join("\n", errors)
                    };
                }

                return new SeleniumStepResult
                {
                    Success = true,
                    Message = $"{requestingGroup} dashboard review matches expected status, name, and date."
                };
            }
            catch (Exception ex)
            {
                return new SeleniumStepResult
                {
                    Success = false,
                    Message = $"Error validating dashboard status: {ex.Message}"
                };
            }
        }

    }

}
