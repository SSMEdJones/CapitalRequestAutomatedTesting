using AutoMapper;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using System.Diagnostics;
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

        public EmailNotifications(
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper
            )
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;

            Debug.WriteLine($"✅ Loaded BaseApiUrl in EmailNotifications from config: {_ssmWorkFlowSettings.BaseApiUrl}");
            Debug.WriteLine($"✅ Loaded ProjectReviewLink in EmailNotifications from config: {_ssmWorkFlowSettings.ProjectReviewLink}");

        }

        public async Task<EmailNotification> Get(int id)
        {
            try
            {
                var emailNotification = new EmailNotification();
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment($"EmailNotification/{id}")
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
                var rawItems = JsonConvert.DeserializeObject<List<JToken>>(responseObject);

                if (rawItems != null)
                {
                    int index = 0;
                    foreach (var item in rawItems)
                    {
                        try
                        {
                            var notification = item.ToObject<EmailNotification>();
                            if (notification != null)
                            {
                                emailNotifications.Add(notification);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log or handle the specific item that failed
                            Console.WriteLine($"Deserialization failed at index {index}: {ex.Message}");
                            // Optionally log the raw item for inspection
                            Console.WriteLine($"Raw item: {item}");
                        }
                        index++;
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