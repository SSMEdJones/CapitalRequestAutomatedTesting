using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using SSMWorkflow.API.DataAccess.Models;
using System.Buffers.Text;
using System.Diagnostics;
using System.Numerics;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services
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

            var applicationUserAccess = await GetApplicationUserAccess(userId);

            var applicationRoleName = applicationUserAccess.ApplicationRoleName;

            if (applicationRoleName == Constants.APPLICATION_ROLE_NAME_REVIEWER)
            {
                var reviewerFilter = (await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter { UserId = userId}))
                    .Select(x => new
                    {
                        x.RegionId,
                        x.SegmentId
                    })
                    .Distinct()
                    .ToList();

                if (reviewerFilter.Any())
                {
                    var authorlist = dashboardData.Where(x => x.UserId == userId).ToList();

                    dashboardData = (from data in dashboardData
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

                    if (authorlist.Any())
                    {
                        dashboardData.RemoveAll(x => x.UserId == userId);

                        authorlist.ForEach(x =>
                        {
                            dashboardData.Add(x);
                        });
                    }
                }
            }
            //else if (applicationRoleName == Constants.APPLICATION_ROLE_NAME_REGIONAL)
            //{
            //    var regionFilter = _userRegionRepo.GetAllUserRegions(userId)
            //                .Distinct()
            //                .ToList();

            //    dashboardData = (from model in dashboardData
            //                     join region in regionFilter on model.RegionId equals region.RegionId
            //                     select model)
            //              .Distinct()
            //              .ToList();
            //}
            else if (applicationRoleName == Constants.APPLICATION_ROLE_NAME_AUTHOR)
            {
                dashboardData = dashboardData.Where(x => x.UserId == userId).ToList();
            }

            if (dashboardData != null && dashboardData.Any())
            {
                dashboardData = dashboardData
                .OrderByDescending(x => x.Submitted == DateTime.MinValue ?
                        x.Pending :
                        x.Submitted)
                .ToList();

            }

            return dashboardData;
        }


        private async Task<CapitalRequest.API.Models.ApplicationUser> GetApplicationUserAccess(string userId)
        {
            Debug.WriteLine($"[SetApplicationUserAccess] Attempting to load user: {userId}");

            var applicationUserAccess = await _capitalRequestServices.GetApplicationUser(userId);

            return applicationUserAccess;
        }
    }

    

    }
