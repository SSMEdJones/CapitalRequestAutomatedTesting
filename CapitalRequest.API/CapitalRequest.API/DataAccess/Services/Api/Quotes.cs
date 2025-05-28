using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quote = CapitalRequest.API.Models.Quote;
using Attachment = CapitalRequest.API.Models.Attachment;


namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IQuotes
    {
        Task<Quote> Get(int id);
        Task<List<Quote>> GetAll(QuoteSearchFilter filter);
        Task DeleteAll(QuoteSearchFilter filter);
    }

    public class Quotes : IQuotes
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public Quotes(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Quote> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Quote")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<Quote>(responseObject);

                return _mapper.Map<Quote>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<Quote>> GetAll(QuoteSearchFilter filter)
        {
            try
            {
                var quotes = new List<Quote>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Quote")
                    .SetQueryParams(new
                    {
                        filter.ProposalId,
                        filter.FileName
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<Quote>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        quotes.Add(result);
                    }
                }

                return quotes;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task DeleteAll(QuoteSearchFilter filter)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("Quote")
                        .SetQueryParams(new
                        {
                            filter.ProposalId,
                            filter.FileName
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
