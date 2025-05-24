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
    public interface ISSMWorkFlowInstanceActionHistory
    {
        Task<Guid> Add(CreateUpdateWorkFlowInstanceActionHistory workFlowInstanceActionHistory);

        Task Delete(Guid workflowId);

        Task<WorkFlowInstanceActionHistoryViewModel> Get(Guid workflowInstanceActionHistoryID);

        Task<List<WorkFlowInstanceActionHistoryViewModel>> GetAll(WorkFlowInstanceActionHistorySearchFilter filter);

        Task<WorkFlowInstanceActionHistoryViewModel> Update(CreateUpdateWorkFlowInstanceActionHistory createWorkFlowInstanceActionHistory, Guid stakeholderId);
    }

    public class SSMWorkFlowInstanceActionHistory : ISSMWorkFlowInstanceActionHistory
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public SSMWorkFlowInstanceActionHistory(
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper
            )
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Guid> Add(CreateUpdateWorkFlowInstanceActionHistory createWorkFlowInstanceActionHistory)
        {
            var workflowInstanceActionHistory = _mapper.Map<WorkflowInstanceActionHistory>(createWorkFlowInstanceActionHistory);

            try
            {
                var workflowInstanceActionHistoryId = Guid.Empty;

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowInstanceActionHistory")
                    .PostJsonAsync(workflowInstanceActionHistory)
                    .ReceiveJson<Response<Guid>>();

                workflowInstanceActionHistoryId = response.Result;

                return await Task.FromResult(workflowInstanceActionHistoryId);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send add request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<WorkFlowInstanceActionHistoryViewModel> Get(Guid WorkflowInstanceActionHistoryID)
        {
            try
            {
                var workFlowInstanceActionHistoryViewModel = new WorkFlowInstanceActionHistoryViewModel();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowInstanceActionHistory")
                    .AppendPathSegment($"{WorkflowInstanceActionHistoryID}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowInstanceActionHistoryViewModel>(responseObject);

                if (results != null)
                {
                    workFlowInstanceActionHistoryViewModel = results;
                }

                return _mapper.Map<WorkFlowInstanceActionHistoryViewModel>(workFlowInstanceActionHistoryViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<List<WorkFlowInstanceActionHistoryViewModel>> GetAll(WorkFlowInstanceActionHistorySearchFilter filter)
        {
            try
            {
                var workFlowInstanceActionHistoryViewModel = new List<WorkFlowInstanceActionHistoryViewModel>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowInstanceActionHistory")
                        .SetQueryParam($"WorkflowInstanceID={filter.WorkflowInstanceID}")
                        .SetQueryParam($"WorkflowInstanceID={filter.OptionID}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkFlowInstanceActionHistoryViewModel>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        workFlowInstanceActionHistoryViewModel.Add(result);
                    }
                }

                return workFlowInstanceActionHistoryViewModel;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task Delete(Guid WorkflowInstanceActionHistoryID)
        {
            await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowInstanceActionHistory")
                        .AppendPathSegment($"{WorkflowInstanceActionHistoryID}")
                        .DeleteAsync();
        }

        public async Task<WorkFlowInstanceActionHistoryViewModel> Update(CreateUpdateWorkFlowInstanceActionHistory createWorkFlowInstanceActionHistory, Guid WorkflowInstanceActionHistoryID)
        {
            var workFlowInstanceActionHistoryViewModel = new WorkFlowInstanceActionHistoryViewModel();

            try
            {
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowInstanceActionHistory")
                        .AppendPathSegment($"{WorkflowInstanceActionHistoryID}")
                        .PutJsonAsync(createWorkFlowInstanceActionHistory)
                        .ReceiveJson<Response<WorkFlowViewModel>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);

                var results = JsonConvert.DeserializeObject<WorkFlowInstanceActionHistoryViewModel>(responseObject);

                if (results != null)
                {
                    workFlowInstanceActionHistoryViewModel = results;
                }

                return _mapper.Map<WorkFlowInstanceActionHistoryViewModel>(workFlowInstanceActionHistoryViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send update to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}