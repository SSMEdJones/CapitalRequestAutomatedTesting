using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Attachment = CapitalRequest.API.Models.Attachment;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IAttachments
    {
        Task<Attachment> Get(int id);
        Task<List<Attachment>> GetAll(AttachmentSearchFilter filter);
        Task DeleteAll(AttachmentSearchFilter filter);
    }

    public class Attachments : IAttachments
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public Attachments(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Attachment> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Attachment")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<Attachment>(responseObject);

                return _mapper.Map<Attachment>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<Attachment>> GetAll(AttachmentSearchFilter filter)
        {
            try
            {
                var attachments = new List<Attachment>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Attachment")
                    .SetQueryParams(new
                    {
                        filter.ProposalId,
                        filter.FileName,
                        filter.ProvidedInfoId
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<Attachment>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        attachments.Add(result);
                    }
                }

                return attachments;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task DeleteAll(AttachmentSearchFilter filter)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("Attachment")
                         .SetQueryParams(new
                         {  
                             filter.ProposalId,
                             filter.FileName,
                             filter.ProvidedInfoId
                         }).DeleteAsync();
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send delete all request to CapitalRequest. {exceptionResponse}");
            }
        }
    }
}
