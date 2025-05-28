using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WorkflowTemplate = CapitalRequest.API.Models.WorkflowTemplate;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IWorkflowTemplates
    {
        Task<WorkflowTemplate> Get(int id);
        Task<List<WorkflowTemplate>> GetAll(WorkflowTemplateSearchFilter filter);
        Task DeleteAll(WorkflowTemplateSearchFilter filter);
    }

    public class WorkflowTemplates : IWorkflowTemplates
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public WorkflowTemplates(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<WorkflowTemplate> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("WorkflowTemplate")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var result = JsonConvert.DeserializeObject<WorkflowTemplate>(responseObject);

                return _mapper.Map<WorkflowTemplate>(result);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<WorkflowTemplate>> GetAll(WorkflowTemplateSearchFilter filter)
        {
            try
            {
                var wbsList = new List<WorkflowTemplate>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("WorkflowTemplate")
                    .SetQueryParams(new
                    {
                        filter.StepName,
                        filter.StepDescription,
                        filter.StepNumber
                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkflowTemplate>>(responseObject);

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

        public async Task DeleteAll(WorkflowTemplateSearchFilter filter)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("WorkflowTemplate")
                    .SetQueryParams(new
                    {
                        filter.StepName,
                        filter.StepDescription,
                        filter.StepNumber

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
