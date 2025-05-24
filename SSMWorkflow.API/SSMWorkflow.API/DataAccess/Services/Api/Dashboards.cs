using AutoMapper;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.Models;
using SSMWorkflow.API.DataAccess.Models;

namespace SSMWorkflow.API.DataAccess.Services.Api
{
    public interface IDashboards
    {
        Task<List<API.Models.Dashboard>> GetDashboardData(DashboardSearchFilter dashboardSearchFilter);
    }

    public class Dashboards : IDashboards
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public Dashboards(IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings, IMapper mapper
)
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<List<API.Models.Dashboard>> GetDashboardData(DashboardSearchFilter dashboardSearchFilter)
        {
            try
            {
                var dashboardViewModel = new List<API.Models.Dashboard>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("Dashboard")
                        .SetQueryParam($"CapitalFundingYear={dashboardSearchFilter.CapitalFundingYear}")
                        .SetQueryParam($"HistoricalDataOnly={dashboardSearchFilter.HistoricalDataOnly}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<Models.Dashboard>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        dashboardViewModel.Add(_mapper.Map<API.Models.Dashboard>(result));
                    }
                }

                return dashboardViewModel;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}