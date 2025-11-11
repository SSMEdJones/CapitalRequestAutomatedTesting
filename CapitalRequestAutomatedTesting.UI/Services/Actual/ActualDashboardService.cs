using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using SSMWorkflow.API.DataAccess.Models;
using System.Diagnostics;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualDashboardService
    {
        Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardDataByUserId(DashboardSearchFilter filter, string userId);
        //Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardDataByUserId(DashboardSearchFilter filter, vm.Reviewer selectedReviewer);
    }
    public class ActualDashboardService : IActualDashboardService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;

        public ActualDashboardService(
            ISSMWorkflowServices ssmWorkflowServices,
            ICapitalRequestServices capitalRequestServices)
        {
            _ssmWorkflowServices = ssmWorkflowServices;
            _capitalRequestServices = capitalRequestServices;
        }

        public async Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardDataByUserId(DashboardSearchFilter filter, string userId)
        {
            var dashboardData = new List<SSMWorkflow.API.Models.Dashboard>();
            var applicationUser = await GetApplicationUserAccess(userId);
            var dashboards = await _ssmWorkflowServices.GetAllDashboards(filter);

            if (dashboards == null || !dashboards.Any())
                return dashboardData;

            var reviewerFilter = (await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { UserId = userId }))
                    .Select(x => new
                    {
                        x.RegionId,
                        x.SegmentId
                    })
                    .Distinct()
                    .ToList();

            if (reviewerFilter.Any())
            {

                dashboardData = (from data in dashboards
                                 join reviewer in reviewerFilter on new
                                 {
                                     data.RegionId,
                                     data.SegmentId
                                 } equals new
                                 {
                                     reviewer.RegionId,
                                     reviewer.SegmentId
                                 }
                                 select data)
                .ToList();

            }
            else
            {
                dashboardData = (from data in dashboards
                                 where applicationUser != null && data.UserId == applicationUser.UserId
                                 select data)
                .ToList();
            }

                return dashboardData;
        }

        private async Task<vm.ApplicationUser> GetApplicationUserAccess(string userId)
        {
            Debug.WriteLine($"[SetApplicationUserAccess] Attempting to load user: {userId}");

            var applicationUserAccess = await _capitalRequestServices.GetApplicationUser(userId);

            return applicationUserAccess;
        }

        //public async Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardDataByUserId(DashboardSearchFilter filter, vm.Reviewer selectedReviewer)
        //{
        //    var dashboardData = new List<SSMWorkflow.API.Models.Dashboard>();
        //    var userId = selectedReviewer.UserId;
        //    var dashboards = await _ssmWorkflowServices.GetAllDashboards(filter);
        //    if (dashboards == null || !dashboards.Any())
        //        return dashboardData;

        //    var reviewerFilter = (await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { UserId = userId }))
        //            .Select(x => new
        //            {
        //                x.RegionId,
        //                x.SegmentId
        //            })
        //            .Distinct()
        //            .ToList();

        //    if (reviewerFilter.Any())
        //    {

        //        dashboardData = (from data in dashboards
        //                         join reviewer in reviewerFilter on new
        //                         {
        //                             data.RegionId,
        //                             data.SegmentId
        //                         } equals new
        //                         {
        //                             reviewer.RegionId,
        //                             reviewer.SegmentId
        //                         }
        //                         select data)
        //        .ToList();

        //    }

        //    return dashboardData;
        //}

    }



}
