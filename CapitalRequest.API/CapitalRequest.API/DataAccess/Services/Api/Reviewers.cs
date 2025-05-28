using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Reviewer = CapitalRequest.API.Models.Reviewer;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IReviewers
    {
        Task<Reviewer> Get(int id);
        Task<List<Reviewer>> GetAll(ReviewerSearchFilter filter);
    }

    public class Reviewers : IReviewers
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public Reviewers(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Reviewer> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Reviewer")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<Reviewer>(responseObject);

                return _mapper.Map<Reviewer>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<Reviewer>> GetAll(ReviewerSearchFilter filter)
        {
            try
            {
                var reviewers = new List<Reviewer>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Reviewer")
                    .SetQueryParams(new
                    {
                        filter.Email,
                        filter.RegionId,
                        filter.SegmentId,
                        filter.ReviewerGroupId,
                        filter.StepNumber,
                        filter.IsVpOfOps
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<Reviewer>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        reviewers.Add(result);
                    }
                }

                return reviewers;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }
    }
}
