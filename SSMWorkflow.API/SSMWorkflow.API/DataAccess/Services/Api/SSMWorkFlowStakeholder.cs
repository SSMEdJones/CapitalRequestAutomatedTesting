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
    public interface ISSMWorkFlowStakeholder
    {
        Task<Guid> Add(CreateUpdateWorkFlowStakeholder workFlowStakeholder);

        Task Delete(Guid workflowId);

        Task<WorkFlowStakeholderViewModel> Get(Guid workflowStakeholderID);

        Task<List<WorkFlowStakeholderViewModel>> GetAll(Guid workflowID);

        Task<WorkFlowStakeholderViewModel> Update(CreateUpdateWorkFlowStakeholder createWorkFlowStakeholder, Guid stakeholderId);
    }

    public class SSMWorkFlowStakeholder : ISSMWorkFlowStakeholder
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public SSMWorkFlowStakeholder(
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper
            )
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Guid> Add(CreateUpdateWorkFlowStakeholder createWorkFlowStakeholder)
        {
            var workflowStakeholder = _mapper.Map<WorkflowStakeholder>(createWorkFlowStakeholder);

            try
            {
                var workflowId = Guid.Empty;

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStakeholder")
                    .PostJsonAsync(workflowStakeholder)
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

        public async Task<WorkFlowStakeholderViewModel> Get(Guid stakeholderID)
        {
            try
            {
                var workFlowStakeholderViewModel = new WorkFlowStakeholderViewModel();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStakeholder")
                    .AppendPathSegment($"{stakeholderID}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStakeholderViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStakeholderViewModel = results;
                }

                return _mapper.Map<WorkFlowStakeholderViewModel>(workFlowStakeholderViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<List<WorkFlowStakeholderViewModel>> GetAll(Guid workflowID)
        {
            try
            {
                var workFlowStakeholderViewModel = new List<WorkFlowStakeholderViewModel>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStakeholder")
                        .SetQueryParam($"WorkflowID={workflowID}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkFlowStakeholderViewModel>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        workFlowStakeholderViewModel.Add(result);
                    }
                }

                return workFlowStakeholderViewModel;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task Delete(Guid stakeholderID)
        {
            await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStakeholder")
                        .AppendPathSegment($"{stakeholderID}")
                        .DeleteAsync();
        }

        public async Task<WorkFlowStakeholderViewModel> Update(CreateUpdateWorkFlowStakeholder createWorkFlowStakeholder, Guid stakeholderId)
        {
            var workFlowStakeholderViewModel = new WorkFlowStakeholderViewModel();

            try
            {
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStakeholder")
                        .AppendPathSegment($"{stakeholderId}")
                        .PutJsonAsync(createWorkFlowStakeholder)
                        .ReceiveJson<Response<WorkFlowViewModel>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStakeholderViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStakeholderViewModel = results;
                }

                return _mapper.Map<WorkFlowStakeholderViewModel>(workFlowStakeholderViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send update to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}