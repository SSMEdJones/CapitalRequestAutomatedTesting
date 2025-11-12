using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Infrastructure.ApiDiagnostics;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReviewerGroup = CapitalRequest.API.Models.ReviewerGroup;


namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IReviewerGroups
    {
        Task<ReviewerGroup> Get(int id);
        Task<List<ReviewerGroup>> GetAll(ReviewerGroupSearchFilter filter);
    }

    public class ReviewerGroups : IReviewerGroups
    {
        private readonly IApiDiagnosticsSender _apiDiagnosticsSender;
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;
        private const string EndpointName = "ReviewerGroup";

        public ReviewerGroups(
            IApiDiagnosticsSender apiDiagnosticsSender,
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _apiDiagnosticsSender = apiDiagnosticsSender;
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }


        public async Task<ReviewerGroup> Get(int id)
        {
            try
            {
                var fullurl = _capitalRequestSettings.BaseApiUrl
                   .AppendPathSegment("ReviewerGroup")
                   .AppendPathSegment($"{id}");

                var response = await _apiDiagnosticsSender
                    .GetWithDiagnosticsAsync<Response<dynamic>>(fullurl);

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<ReviewerGroup>(responseObject);

                return _mapper.Map<ReviewerGroup>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }
        
        public async Task<List<ReviewerGroup>> GetAll(ReviewerGroupSearchFilter filter)
        {

            var reviewerGroups = new List<ReviewerGroup>();

            // Build query parameters
            string queryString = BuildQueryString(filter);
            var fullurl = _capitalRequestSettings.BaseApiUrl
               .AppendPathSegment("ReviewerGroup")
               .SetQueryParams(new
               {
                   filter.Name,
                   filter.EmailTemplateId,
                   filter.ReviewerType,
                   filter.AdminReviewer,
                   filter.StepNumber
               });

            var response = await _apiDiagnosticsSender
                    .GetWithDiagnosticsAsync<Response<dynamic>>(fullurl);

            var responseObject = JsonConvert.SerializeObject(response.Result);
            var results = JsonConvert.DeserializeObject<List<ReviewerGroup>>(responseObject);

            if (results != null)
            {
                foreach (var result in results)
                {
                    reviewerGroups.Add(result);
                }
            }

            return reviewerGroups;
        }
        //public async Task<List<ReviewerGroup>> GetAll(ReviewerGroupSearchFilter filter)
        //{
        //    try
        //    {
        //        // Build query parameters
        //        string queryString = BuildQueryString(filter);
        //        var endpoint = $"{EndpointName}{queryString}";

        //        var response = await _apiDiagnosticsSender
        //            .GetWithDiagnosticsAsync<Response<dynamic>>(_capitalRequestSettings.BaseApiUrl, endpoint);

        //        var responseObject = JsonConvert.SerializeObject(response.Result);
        //        var results = JsonConvert.DeserializeObject<List<ReviewerGroup>>(responseObject);

        //        return results ?? new List<ReviewerGroup>();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Failed attempting to send get all request to CapitalRequest. {ex.Message}", ex);
        //    }
        //}

        private string BuildQueryString(ReviewerGroupSearchFilter filter)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(filter.Name))
                queryParams.Add($"Name={Uri.EscapeDataString(filter.Name)}");

            if (filter.EmailTemplateId.HasValue)
                queryParams.Add($"EmailTemplateId={filter.EmailTemplateId}");

            if (!string.IsNullOrEmpty(filter.ReviewerType))
                queryParams.Add($"ReviewerType={Uri.EscapeDataString(filter.ReviewerType)}");

            if (filter.AdminReviewer.HasValue)
                queryParams.Add($"AdminReviewer={filter.AdminReviewer}");

            return queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : "";
        }
    }
}
