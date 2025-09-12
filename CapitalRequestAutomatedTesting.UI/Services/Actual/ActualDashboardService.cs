using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using SSMWorkflow.API.DataAccess.Models;
using System.Buffers.Text;
using System.Diagnostics;
using System.Numerics;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual
{
    public interface IActualDashboardService
    {
        Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardDataByUserId(DashboardSearchFilter filter, vm.Reviewer selectedReviewer);
    }
    public class ActualDashboardService : IActualDashboardService
    {
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public ActualDashboardService(
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

        public async Task<List<SSMWorkflow.API.Models.Dashboard>> GetDashboardDataByUserId(DashboardSearchFilter filter, vm.Reviewer selectedReviewer)
        {
            var dashboardData = new List<SSMWorkflow.API.Models.Dashboard>();
            var userId = selectedReviewer.UserId;
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

            return dashboardData;
        }


        private async Task<vm.ApplicationUser> GetApplicationUserAccess(string userId)
        {
            Debug.WriteLine($"[SetApplicationUserAccess] Attempting to load user: {userId}");

            var applicationUserAccess = await _capitalRequestServices.GetApplicationUser(userId);

            return applicationUserAccess;
        }
    }

    

    }
