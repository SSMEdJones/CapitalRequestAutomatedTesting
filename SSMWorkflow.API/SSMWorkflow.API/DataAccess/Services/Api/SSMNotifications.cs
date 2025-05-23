using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.Data.Models;

namespace SSMWorkflow.API.DataAccess.Services.Api
{
    public interface ISSMNotification
    {
        //Task SendCapitalRequestGroupNotifications(Guid workflowStepId);
        Task SendCapitalRequestGroupNotificationsAsync(NotificationSearchFilter notificationSearchFilter);

        Task SendCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);

        Task<List<Notification>> GetCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);
    }

    public class SSMNotification : ISSMNotification
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;

        public SSMNotification(IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings)
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
        }

        //public async Task SendCapitalRequestGroupNotifications(Guid workflowStepId)
        //{
        //    await _ssmWorkFlowSettings.BaseApiUrl
        //                .AppendPathSegment("Notification")
        //                .AppendPathSegment($"{workflowStepId}")
        //                .PostJsonAsync(workflowStepId);

        //}

        public async Task SendCapitalRequestGroupNotificationsAsync(NotificationSearchFilter notificationSearchFilter)
        {
            await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("Notification")
                        .PostJsonAsync(notificationSearchFilter);
        }

        public Task SendCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter)
        {
            _ssmWorkFlowSettings.BaseApiUrl
                       .AppendPathSegment("Notification")
                       .PostJsonAsync(notificationSearchFilter);

            return Task.CompletedTask;
        }

        public async Task<List<Notification>> GetCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter)
        {
            try
            {
                var notifcation = new List<Notification>();

                var workflowStepId = notificationSearchFilter.WorkflowStepId;

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("Notification")
                        .SetQueryParam($"WorkflowStepId={notificationSearchFilter.WorkflowStepId}")
                        .SetQueryParam($"EmailTemplateId={notificationSearchFilter.EmailTemplateId}")
                        .SetQueryParam($"ReviewerGroupId={notificationSearchFilter.ReviewerGroupId}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<Notification>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        notifcation.Add(result);
                    }
                }

                return notifcation;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}