using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using DeletedReviewer = CapitalRequest.API.Models.DeletedReviewer;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IDeletedReviewers
    {
        Task<DeletedReviewer> Get(int id);
        Task<List<DeletedReviewer>> GetAll(DeletedReviewerSearchFilter filter);
    }

    public class DeletedReviewers : IDeletedReviewers
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public DeletedReviewers(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<DeletedReviewer> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Reviewer")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<DeletedReviewer>(responseObject);

                return _mapper.Map<DeletedReviewer>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<DeletedReviewer>> GetAll(DeletedReviewerSearchFilter filter)
        {
            try
            {
                var reviewers = new List<DeletedReviewer>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("DeletedReviewer")
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
                var results = JsonConvert.DeserializeObject<List<DeletedReviewer>>(responseObject);

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
