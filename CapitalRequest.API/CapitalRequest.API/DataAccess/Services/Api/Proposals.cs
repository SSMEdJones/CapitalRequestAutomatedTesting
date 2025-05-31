using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Flurl;
using Flurl.Http;
using CapitalRequest.API.DataAccess.Models;
using Proposal = CapitalRequest.API.Models.Proposal;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;
using Newtonsoft.Json.Linq;
using static Dapper.SqlMapper;

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
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Proposal")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var json = JsonConvert.SerializeObject(response.Result);

                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        // Optional: log or debug
                        args.ErrorContext.Handled = true;
                    }
                };

                var proposal = JsonConvert.DeserializeObject<vm.Proposal>(json, settings);
                return proposal;

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
