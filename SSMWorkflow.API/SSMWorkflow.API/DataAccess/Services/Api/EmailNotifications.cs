using AutoMapper;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using EmailNotification = SSMWorkflow.API.Models.EmailNotification;

namespace SSMWorkflow.API.DataAccess.Services.Api
{
    public interface IEmailNotifications
    {
        Task<EmailNotification> Get(int id);
        Task<List<EmailNotification>> GetAll(EmailNotificationSearchFilter filter);

    }

    public class EmailNotifications : IEmailNotifications
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public EmailNotifications(IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings, IMapper mapper)
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;

        }

        public async Task<EmailNotification> Get(int id)
        {
            try
            {
                var emailNotification = new EmailNotification();
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("EmailNotification")
                    .SetQueryParam($"id={id}")
                    .GetJsonAsync<Response<dynamic>>();
                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<EmailNotification>(responseObject);

                if (results != null)
                {
                    emailNotification = results;
                }

                return _mapper.Map<EmailNotification>(emailNotification);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to SSMWorkFlow. {exceptionResponse}");
            }

        }

        public async Task<List<EmailNotification>> GetAll(EmailNotificationSearchFilter filter)
        {
            try
            {
                var emailNotifications = new List<EmailNotification>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("EmailNotification")
                        .SetQueryParam($"WorkflowStepId={filter.WorkflowStepId}")
                        .SetQueryParam($"EmailQuery={filter.EmailQuery}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<EmailNotifications>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        emailNotifications.Add(result);
                    }
                }

                return emailNotifications;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        
    }
}