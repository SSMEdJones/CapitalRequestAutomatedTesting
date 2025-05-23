using AutoMapper;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.Data.Models;

namespace SSMWorkflow.API.DataAccess.Services.Api
{
    public interface ISSMWorkFlowStepResponder
    {
        Task<Guid> Add(CreateUpdateWorkFlowStepResponder workFlowStepResponder);

        Task Delete(Guid workflowId);

        Task<WorkFlowStepResponderViewModel> Get(Guid responderID);

        Task<List<WorkFlowStepResponderViewModel>> GetAll(Guid workflowStepID);

        Task<WorkFlowStepResponderViewModel> Update(CreateUpdateWorkFlowStepResponder createWorkFlowStepResponder, Guid responderID);
    }

    public class SSMWorkFlowStepResponder : ISSMWorkFlowStepResponder
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public SSMWorkFlowStepResponder(
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper
            )
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Guid> Add(CreateUpdateWorkFlowStepResponder createWorkFlowStepResponder)
        {
            var workflowStepResponder = _mapper.Map<WorkflowStepResponder>(createWorkFlowStepResponder);

            try
            {
                var workflowId = Guid.Empty;

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStepResponder")
                    .PostJsonAsync(workflowStepResponder)
                    .ReceiveJson<Response<Guid>>();

                workflowId = response.Result;

                return workflowId;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send add request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<WorkFlowStepResponderViewModel> Get(Guid responderID)
        {
            try
            {
                var workFlowStepResponderViewModel = new WorkFlowStepResponderViewModel();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStepResponder")
                    .AppendPathSegment($"{responderID}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStepResponderViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStepResponderViewModel = results;
                }

                return _mapper.Map<WorkFlowStepResponderViewModel>(workFlowStepResponderViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<List<WorkFlowStepResponderViewModel>> GetAll(Guid workflowStepID)
        {
            try
            {
                var workFlowStepResponderViewModel = new List<WorkFlowStepResponderViewModel>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStepResponder")
                        .SetQueryParam($"WorkflowStepID={workflowStepID}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkFlowStepResponderViewModel>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        workFlowStepResponderViewModel.Add(result);
                    }
                }

                return workFlowStepResponderViewModel;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task Delete(Guid responderID)
        {
            await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStepResponder")
                        .AppendPathSegment($"{responderID}")
                        .DeleteAsync();
        }

        public async Task<WorkFlowStepResponderViewModel> Update(CreateUpdateWorkFlowStepResponder createWorkFlowStepResponder, Guid responderID)
        {
            var workFlowStepResponderViewModel = new WorkFlowStepResponderViewModel();

            try
            {
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStepResponder")
                        .AppendPathSegment($"{responderID}")
                        .PutJsonAsync(createWorkFlowStepResponder)
                        .ReceiveJson<Response<WorkFlowViewModel>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStepResponderViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStepResponderViewModel = results;
                }

                return _mapper.Map<WorkFlowStepResponderViewModel>(workFlowStepResponderViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send update to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}