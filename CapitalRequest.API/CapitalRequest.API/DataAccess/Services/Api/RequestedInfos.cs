using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using RequestedInfo = CapitalRequest.API.Models.RequestedInfo;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IRequestedInfos
    {
        Task<RequestedInfo> Get(int id);
        Task<RequestedInfo> Update(RequestedInfo requestedInfo);
        Task<List<RequestedInfo>> GetAll(RequestedInfoSearchFilter filter);
        Task DeleteAll(RequestedInfoSearchFilter filter);
    }

    public class RequestedInfos : IRequestedInfos
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public RequestedInfos(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<RequestedInfo> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("RequestedInfo")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<API.Models.Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<RequestedInfo>(responseObject);

                return _mapper.Map<RequestedInfo>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<RequestedInfo>> GetAll(RequestedInfoSearchFilter filter)
        {
            try
            {
                var requestedInfos = new List<RequestedInfo>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("RequestedInfo")
                    .SetQueryParams(new
                    {
                        filter.ProposalId,
                        filter.RequestingReviewerGroupId,
                        filter.RequestingReviewerId,
                        filter.ReviewerGroupId,
                        filter.WorkflowStepOptionId,
                        filter.IsOpen
                    })
                    .GetJsonAsync<API.Models.Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<RequestedInfo>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        requestedInfos.Add(result);
                    }
                }

                return requestedInfos;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task DeleteAll(RequestedInfoSearchFilter filter)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("RequestedInfo")
                    .SetQueryParams(new
                    {
                        filter.ProposalId,
                        filter.RequestingReviewerGroupId,
                        filter.RequestingReviewerId,
                        filter.ReviewerGroupId,
                        filter.WorkflowStepOptionId,
                        filter.IsOpen
                    })
                        .DeleteAsync();
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send delete all request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<RequestedInfo> Update(RequestedInfo requestedInfo)
        {

            var createRequestedInfo = _mapper.Map<CreateUpdateRequestedInfo>(requestedInfo);
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("RequestedInfo")
                        .AppendPathSegment($"{requestedInfo.Id}")
                        .PutJsonAsync(createRequestedInfo)
                        .ReceiveJson<API.Models.Response<RequestedInfo>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<RequestedInfo>(responseObject);

                return results;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send update to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}
