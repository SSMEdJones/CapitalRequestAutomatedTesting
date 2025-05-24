using AutoMapper;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;

namespace SSMWorkflow.API.DataAccess.Services.Api
{
    public interface ISSMWorkFlowInstance
    {
        Task<Guid> Add(CreateUpdateWorkFlowInstance workFlowInstance);

        Task Delete(Guid workflowId);

        Task<WorkFlowInstanceViewModel> Get(Guid workflowInstanceID);

        Task<List<WorkFlowInstanceViewModel>> GetAll(Guid workflowID);

        Task<WorkFlowInstanceViewModel> Update(CreateUpdateWorkFlowInstance createWorkFlowInstance, Guid stakeholderId);
    }

    public class SSMWorkFlowInstance : ISSMWorkFlowInstance
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public SSMWorkFlowInstance(
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper
            )
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Guid> Add(CreateUpdateWorkFlowInstance createWorkFlowInstance)
        {
            var workflowInstance = _mapper.Map<WorkflowInstance>(createWorkFlowInstance);

            try
            {
                var workflowInstanceId = Guid.Empty;

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowInstance")
                    .PostJsonAsync(workflowInstance)
                    .ReceiveJson<Response<Guid>>();

                workflowInstanceId = response.Result;

                return workflowInstanceId;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send add request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<WorkFlowInstanceViewModel> Get(Guid WorkflowInstanceID)
        {
            try
            {
                var workFlowInstanceViewModel = new WorkFlowInstanceViewModel();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowInstance")
                    .AppendPathSegment($"{WorkflowInstanceID}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowInstanceViewModel>(responseObject);

                if (results != null)
                {
                    workFlowInstanceViewModel = results;
                }

                return _mapper.Map<WorkFlowInstanceViewModel>(workFlowInstanceViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<List<WorkFlowInstanceViewModel>> GetAll(Guid workflowID)
        {
            try
            {
                var workFlowInstanceViewModel = new List<WorkFlowInstanceViewModel>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowInstance")
                        .SetQueryParam($"WorkflowID={workflowID}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkFlowInstanceViewModel>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        workFlowInstanceViewModel.Add(result);
                    }
                }

                return workFlowInstanceViewModel;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task Delete(Guid WorkflowInstanceID)
        {
            await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowInstance")
                        .AppendPathSegment($"{WorkflowInstanceID}")
                        .DeleteAsync();
        }

        public async Task<WorkFlowInstanceViewModel> Update(CreateUpdateWorkFlowInstance createWorkFlowInstance, Guid WorkflowInstanceID)
        {
            var workFlowInstanceViewModel = new WorkFlowInstanceViewModel();

            try
            {
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowInstance")
                        .AppendPathSegment($"{WorkflowInstanceID}")
                        .PutJsonAsync(createWorkFlowInstance)
                        .ReceiveJson<Response<WorkFlowViewModel>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowInstanceViewModel>(responseObject);

                if (results != null)
                {
                    workFlowInstanceViewModel = results;
                }

                return _mapper.Map<WorkFlowInstanceViewModel>(workFlowInstanceViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send update to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}