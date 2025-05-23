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
    public interface ISSMWorkFlowStepOption
    {
        Task<Guid> Add(CreateUpdateWorkFlowStepOption workFlowStepOption);

        Task Delete(Guid workflowId);

        Task<WorkFlowStepOptionViewModel> Get(Guid optionID);

        Task<List<WorkFlowStepOptionViewModel>> GetAll(Guid workflowStepID);

        Task<WorkFlowStepOptionViewModel> Update(CreateUpdateWorkFlowStepOption createWorkFlowStepOption, Guid optionID);
    }

    public class SSMWorkFlowStepOption : ISSMWorkFlowStepOption
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public SSMWorkFlowStepOption(
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper
            )
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Guid> Add(CreateUpdateWorkFlowStepOption createWorkFlowStepOption)
        {
            var workflowStepOption = _mapper.Map<WorkflowStepOption>(createWorkFlowStepOption);

            try
            {
                var optionId = Guid.Empty;

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStepOption")
                    .PostJsonAsync(workflowStepOption)
                    .ReceiveJson<Response<Guid>>();

                optionId = response.Result;

                return optionId;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send add request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<WorkFlowStepOptionViewModel> Get(Guid optionID)
        {
            try
            {
                var workFlowStepOptionViewModel = new WorkFlowStepOptionViewModel();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStepOption")
                    .AppendPathSegment($"{optionID}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStepOptionViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStepOptionViewModel = results;
                }

                return _mapper.Map<WorkFlowStepOptionViewModel>(workFlowStepOptionViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<List<WorkFlowStepOptionViewModel>> GetAll(Guid workflowStepID)
        {
            try
            {
                var workFlowStepOptionViewModel = new List<WorkFlowStepOptionViewModel>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStepOption")
                        .SetQueryParam($"WorkflowStepID={workflowStepID}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkFlowStepOptionViewModel>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        workFlowStepOptionViewModel.Add(result);
                    }
                }

                return workFlowStepOptionViewModel;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task Delete(Guid workflowStepID)
        {
            await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStepOption")
                        .AppendPathSegment($"{workflowStepID}")
                        .DeleteAsync();
        }

        public async Task<WorkFlowStepOptionViewModel> Update(CreateUpdateWorkFlowStepOption createWorkFlowStepOption, Guid optionID)
        {
            var workFlowStepOptionViewModel = new WorkFlowStepOptionViewModel();

            try
            {
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStepOption")
                        .AppendPathSegment($"{optionID}")
                        .PutJsonAsync(createWorkFlowStepOption)
                        .ReceiveJson<Response<WorkFlowViewModel>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStepOptionViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStepOptionViewModel = results;
                }

                return _mapper.Map<WorkFlowStepOptionViewModel>(workFlowStepOptionViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send update to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}