using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Asset = CapitalRequestAPI.Models.Asset;
using Flurl;
using Flurl.Http;

namespace CapitalRequest.API.DataAccess.Services
{
    public interface IAssets
    {
        Task<Asset> Get(int id);
        Task<IEnumerable<Asset>> GetAll(AssetSearchFilter filter);

    }
    public class Assets : IAssets
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public Assets(
           IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
           IMapper mapper
           )
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Asset> Get(int id)
        {
            try
            {
                var asset = new Asset();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Asset")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<Asset>(responseObject);

                if (results != null)
                {
                    asset = results;
                }

                return _mapper.Map<Asset>(asset);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }


        public async Task<List<Asset>> GetAll(AssetSearchFilter filter)
        {
            try
            {
                var assets = new List<Asset>();

                var response = await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("Asset")
                        .SetQueryParam($"ProposalId={filter.ProposalId}")
                        .SetQueryParam($"AssetNumber={filter.AssetNumber}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<Asset>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        assets.Add(result);
                    }
                }

                return assets;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }
        
    }

}
