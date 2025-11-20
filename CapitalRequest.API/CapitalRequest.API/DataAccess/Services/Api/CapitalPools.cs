using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface ICapitalPools
    {
        Task<List<CapitalPool>> GetAll();
    }

    public class CapitalPools : ICapitalPools
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public CapitalPools(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<List<CapitalPool>> GetAll()
        {
            try
            {
                var capitalPools = new List<CapitalPool>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("CapitalPool")                    
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<CapitalPool>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        capitalPools.Add(result);
                    }
                }

                return capitalPools;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }
    }
}
