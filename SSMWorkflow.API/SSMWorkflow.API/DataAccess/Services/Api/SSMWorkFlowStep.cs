﻿using AutoMapper;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;

namespace SSMWorkflow.API.DataAccess.Services.Api
{
    public interface ISSMWorkFlowStep
    {
        Task<Guid> Add(CreateUpdateWorkFlowStep workFlowStep);

        Task Delete(Guid workflowId);

        Task<WorkFlowStepViewModel> Get(Guid workflowStepID);

        Task<List<WorkFlowStepViewModel>> GetAll(Guid workflowID);

        Task<WorkFlowStepViewModel> Update(CreateUpdateWorkFlowStep createWorkFlowStep, Guid stakeholderId);
    }

    public class SSMWorkFlowStep : ISSMWorkFlowStep
    {
        private readonly SSMWorkFlowSettings _ssmWorkFlowSettings;
        private readonly IMapper _mapper;

        public SSMWorkFlowStep(
            IOptionsMonitor<SSMWorkFlowSettings> ssmWorkFlowSettings,
            IMapper mapper
            )
        {
            _ssmWorkFlowSettings = ssmWorkFlowSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<Guid> Add(CreateUpdateWorkFlowStep createWorkFlowStep)
        {
            var workflowStep = _mapper.Map<WorkflowStep>(createWorkFlowStep);

            try
            {
                var workflowStepId = Guid.Empty;

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStep")
                    .PostJsonAsync(workflowStep)
                    .ReceiveJson<Response<Guid>>();

                workflowStepId = response.Result;

                return workflowStepId;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send add request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<WorkFlowStepViewModel> Get(Guid workflowStepId)
        {
            try
            {
                var workFlowStepViewModel = new WorkFlowStepViewModel();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                    .AppendPathSegment("WorkFlowStep")
                    .AppendPathSegment($"{workflowStepId}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStepViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStepViewModel = results;
                }

                return _mapper.Map<WorkFlowStepViewModel>(workFlowStepViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to SSMWorkFlow. {exceptionResponse}");
            }
        }

        public async Task<List<WorkFlowStepViewModel>> GetAll(Guid workflowId)
        {
            try
            {
                var workFlowStepViewModel = new List<WorkFlowStepViewModel>();

                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStep")
                        .SetQueryParam($"WorkflowID={workflowId}")
                        .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<WorkFlowStepViewModel>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        workFlowStepViewModel.Add(result);
                    }
                }

                return workFlowStepViewModel;
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
                        .AppendPathSegment("WorkFlowStep")
                        .AppendPathSegment($"{stakeholderID}")
                        .DeleteAsync();
        }

        public async Task<WorkFlowStepViewModel> Update(CreateUpdateWorkFlowStep createWorkFlowStep, Guid workflowStepId)
        {
            var workFlowStepViewModel = new WorkFlowStepViewModel();

            try
            {
                var response = await _ssmWorkFlowSettings.BaseApiUrl
                        .AppendPathSegment("WorkFlowStep")
                        .AppendPathSegment($"{workflowStepId}")
                        .PutJsonAsync(createWorkFlowStep)
                        .ReceiveJson<Response<WorkFlowStepViewModel>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<WorkFlowStepViewModel>(responseObject);

                if (results != null)
                {
                    workFlowStepViewModel = results;
                }

                return _mapper.Map<WorkFlowStepViewModel>(workFlowStepViewModel);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send update to SSMWorkFlow. {exceptionResponse}");
            }
        }
    }
}