using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using EmailTemplate = CapitalRequest.API.Models.EmailTemplate;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IEmailTemplates
    {
        Task<EmailTemplate> Get(int id);
        Task<List<EmailTemplate>> GetAll(EmailTemplateSearchFilter filter);
    }

    public class EmailTemplates : IEmailTemplates
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public EmailTemplates(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<EmailTemplate> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("EmailTemplate")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<EmailTemplate>(responseObject);

                return _mapper.Map<EmailTemplate>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<EmailTemplate>> GetAll(EmailTemplateSearchFilter filter)
        {
            try
            {
                var EmailTemplate = new List<EmailTemplate>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("EmailTemplate")
                    .SetQueryParams(new
                    {
                        filter.Name,
                        filter.OptionType
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<EmailTemplate>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        EmailTemplate.Add(result);
                    }
                }

                return EmailTemplate;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        
    }
}
