using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Flurl;
using Flurl.Http;
using CapitalRequest.API.DataAccess.Models;
using Proposal = CapitalRequest.API.Models.Proposal;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IProposals
    {
        Task<Proposal> Get(int id);
        Task Delete(int id);
    }

    public class Proposals : IProposals
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public Proposals(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<API.Models.Proposal> Get(int id)
        {
            try
            {
                var proposal = new Proposal();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Proposal")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<Proposal>(responseObject);

                if (result != null)
                {
                    proposal = result;
                }

                return _mapper.Map<Proposal>(proposal);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("Proposal")
                        .AppendPathSegment($"{id}")
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
