using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface ICapitalPoolIdentifiers
    {
        Task<List<CapitalPoolIdentifier>> GetAll();
    }

    public class CapitalPoolIdentifiers : ICapitalPoolIdentifiers
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public CapitalPoolIdentifiers(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        

        public async Task<List<CapitalPoolIdentifier>> GetAll()
        {
            try
            {
                var capitalPoolIdentifiers = new List<CapitalPoolIdentifier>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("CapitalPoolIdentifier")                    
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<CapitalPoolIdentifier>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        capitalPoolIdentifiers.Add(result);
                    }
                }

                return capitalPoolIdentifiers;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        
        
    }
}
