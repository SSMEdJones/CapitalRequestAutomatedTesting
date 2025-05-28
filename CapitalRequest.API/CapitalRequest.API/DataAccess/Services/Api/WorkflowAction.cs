using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WorkflowAction = CapitalRequest.API.Models.WorkflowAction;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IWorkflowActions
    {
        Task<List<WorkflowAction>> GetAll(WorkflowActionSearchFilter filter);
    }

    public class WorkflowActions : IWorkflowActions
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public WorkflowActions(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        

        public async Task<List<WorkflowAction>> GetAll(WorkflowActionSearchFilter filter)
        {
            try
            {
                var workflowActions = new List<WorkflowAction>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("WorkflowAction")
                    .SetQueryParams(new
                    {
                        filter.Id,
                        filter.UserId,
                        filter.Email
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkflowAction>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        workflowActions.Add(result);
                    }
                }

                return workflowActions;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        
    }
}
