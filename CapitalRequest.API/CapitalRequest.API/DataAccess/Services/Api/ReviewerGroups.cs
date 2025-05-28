using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
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
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public ReviewerGroups(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<ReviewerGroup> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ReviewerGroup")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

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
            try
            {
                var reviewerGroups = new List<ReviewerGroup>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ReviewerGroup")
                    .SetQueryParams(new
                    {
                        filter.Name,
                        filter.EmailTemplateId,
                        filter.ReviewerType,
                        filter.AdminReviewer
                    })
                    .GetJsonAsync<Response<dynamic>>();

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
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }
    }
}
