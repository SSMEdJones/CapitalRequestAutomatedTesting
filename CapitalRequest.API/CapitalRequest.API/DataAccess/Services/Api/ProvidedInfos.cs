using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProvidedInfo = CapitalRequest.API.Models.ProvidedInfo;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IProvidedInfos
    {
        Task<ProvidedInfo> Get(int id);
        Task<List<ProvidedInfo>> GetAll(ProvidedInfoSearchFilter filter);
        Task DeleteAll(ProvidedInfoSearchFilter filter);
    }

    public class ProvidedInfos : IProvidedInfos
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public ProvidedInfos(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<ProvidedInfo> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ProvidedInfo")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<ProvidedInfo>(responseObject);

                return _mapper.Map<ProvidedInfo>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<ProvidedInfo>> GetAll(ProvidedInfoSearchFilter filter)
        {
            try
            {
                var providedInfos = new List<ProvidedInfo>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ProvidedInfo")
                    .SetQueryParams(new
                    {
                        filter.RequestedInfoId,
                        filter.ReviewerGroupId,
                        filter.ReviewerId
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<ProvidedInfo>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        providedInfos.Add(result);
                    }
                }

                return providedInfos;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task DeleteAll(ProvidedInfoSearchFilter filter)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("ProvidedInfo")
                        .SetQueryParams(new
                        {
                            filter.RequestedInfoId,
                            filter.ReviewerGroupId,
                            filter.ReviewerId
                        })
                        .DeleteAsync();
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send delete all request to CapitalRequest. {exceptionResponse}");
            }
        }
    }
}
