using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wbs = CapitalRequest.API.Models.Wbs;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IWBSs
    {
        Task<Wbs> Get(int id);
        Task<List<Wbs>> GetAll(WbsSearchFilter filter);
        Task DeleteAll(WbsSearchFilter filter);
    }

    public class WBSs : IWBSs
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public WBSs(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Wbs> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Wbs")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<Wbs>(responseObject);

                return _mapper.Map<Wbs>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<Wbs>> GetAll(WbsSearchFilter filter)
        {
            try
            {
                var wbsList = new List<Wbs>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Wbs")
                    .SetQueryParams(new
                    {
                        filter.Wbsnumber,
                        filter.ProposalId,
                        filter.TypeOfProject
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<Wbs>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        wbsList.Add(result);
                    }
                }

                return wbsList;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task DeleteAll(WbsSearchFilter filter)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Wbs")
                    .SetQueryParams(new
                    {
                        filter.Wbsnumber,
                        filter.ProposalId,
                        filter.TypeOfProject

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
