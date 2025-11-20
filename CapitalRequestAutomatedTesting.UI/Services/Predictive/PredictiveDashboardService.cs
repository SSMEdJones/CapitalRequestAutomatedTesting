using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using SSMWorkflow.API.DataAccess.Models;
using System.Buffers.Text;
using System.Numerics;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive
{
    public interface IPredictiveDashboardService
    {
        Task<SeleniumStepResult> ValidateDashboardStatusAsync(vm.Proposal proposal, string requestingGroup, string targetGroup, string dashboardStatus);

    }
    public class PredictiveDashboardService : IPredictiveDashboardService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;

        public PredictiveDashboardService(ISSMWorkflowServices ssmWorkflowServices)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
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

                var reviewDate = string.Empty;
                var reviewName = string.Empty;
                var reviewStatus = string.Empty;

                if (requestingGroup != null)
                {


                    var groupPrefix = $"{requestingGroup.Replace(" ", string.Empty)}Review"; // e.g., "ITReview"
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

                    reviewDate = dateProp.GetValue(dashboardData)?.ToString();
                    reviewName = nameProp.GetValue(dashboardData)?.ToString();
                    reviewStatus = statusProp.GetValue(dashboardData)?.ToString();
                }
                var todayStr = DateTime.Now.ToShortDateString();
                var errors = new List<string>();


                //TODO Need to test for any available proposals by reviewer or Author
                //If none need to check for message
                //TODO condtional based on predictive Response Message
                //<div class="noResults">
                //<div class="noResults">
                //    <h2><i class="far fa-comment-dots" aria-hidden="true"></i>&nbsp;Hmmm...</h2>
                //    <h3>We couldn't find any Requests for you.</h3>
                //    No current projects are available for you to view.If you feel, based on your access, you should see projects listed here, please contact Christine Domagalski or Patrick Herndon.
                //</div>

                if (proposal.ResponseMessage == Constants.RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT)
                {
                    reviewDate = DateTime.Now.ToShortDateString();
                    reviewName = targetGroup;
                    reviewStatus = Constants.DASHBOARD_STATUS_INFORMATION_REQUESTED;

                    if (!string.Equals(reviewDate, todayStr, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected review date '{todayStr}', but got '{reviewDate}'.");

                    if (!string.Equals(reviewName, targetGroup, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected reviewer name '{targetGroup}', but got '{reviewName}'.");

                    if (!string.Equals(reviewStatus, dashboardStatus, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected status '{dashboardStatus}', but got '{reviewStatus}'.");

                }
                else if (proposal.ResponseMessage == Constants.RESPONSE_ADDED_MORE_INFORMATION_SENT)
                {
                    todayStr = string.Empty;
                    reviewDate = string.Empty;
                    reviewName = targetGroup;
                    reviewStatus = Constants.DASHBOARD_STATUS_CLEAR;

                    if (!string.Equals(reviewDate, todayStr, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected review date '{todayStr}', but got '{reviewDate}'.");

                    if (!string.Equals(reviewName, targetGroup, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected reviewer name '{targetGroup}', but got '{reviewName}'.");

                    if (!string.Equals(reviewStatus, dashboardStatus, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected status '{dashboardStatus}', but got '{reviewStatus}'.");

                }
                else if (proposal.ResponseMessage == Constants.RESPONSE_ACTION_VERIFIED)
                {
                    todayStr = DateTime.Now.ToShortDateString();
                    reviewDate = todayStr;
                    reviewName = targetGroup;
                    reviewStatus = dashboardStatus;

                    if (!string.Equals(reviewDate, todayStr, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected review date '{todayStr}', but got '{reviewDate}'.");

                    if (!string.Equals(reviewName, targetGroup, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected reviewer name '{targetGroup}', but got '{reviewName}'.");

                    if (!string.Equals(reviewStatus, dashboardStatus, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected status '{dashboardStatus}', but got '{reviewStatus}'.");

                }
                else if (string.IsNullOrWhiteSpace(proposal.ResponseMessage))
                {
                    todayStr = DateTime.Now.ToShortDateString();
                    var submitName = proposal.SubmitUserId;

                    var submitPrefix = "Submit";

                    var submitted = DateTime.Now.ToShortDateString();
                    var submittedBy = submitName;

                    if (!string.Equals(submitted, todayStr, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected submit date '{todayStr}', but got '{submitted}'.");

                    if (!string.Equals(submitName, submittedBy, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Expected submit name '{submitName}', but got '{submittedBy}'.");
                    
                }

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
                    Message = $"{requestingGroup} dashboard matches expected status, name, and date."
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
