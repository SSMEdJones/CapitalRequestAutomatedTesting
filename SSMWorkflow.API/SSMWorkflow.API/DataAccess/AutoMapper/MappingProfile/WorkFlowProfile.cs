﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using SSMAuthenticationCore;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;

namespace SSMWorkflow.API.DataAccess.AutoMapper.MappingProfile
{
    public class WorkflowProfile : Profile
    {
        public WorkflowProfile()
        {
            //WorkFlow
            CreateMap<WorkFlowViewModel, CreateUpdateWorkFlow>();
            CreateMap<CreateUpdateWorkFlow, Workflow>();

            //WorkFlowStakeHolder
            CreateMap<WorkFlowStakeholderViewModel, CreateUpdateWorkFlowStakeholder>();
            CreateMap<CreateUpdateWorkFlowStakeholder, WorkflowStakeholder>();

            //WorkFlowStep
            CreateMap<WorkFlowStepViewModel, CreateUpdateWorkFlowStep>();
            CreateMap<CreateUpdateWorkFlowStep, WorkflowStep>();

            //WorkFlowInstance
            CreateMap<WorkFlowInstanceViewModel, CreateUpdateWorkFlowInstance>();
            CreateMap<CreateUpdateWorkFlowInstance, WorkflowInstance>();

            //WorkFlowStepOption
            CreateMap<WorkFlowStepOptionViewModel, CreateUpdateWorkFlowStepOption>();
            CreateMap<CreateUpdateWorkFlowStepOption, WorkflowStepOption>();

            //WorkFlowStepResponder
            CreateMap<WorkFlowStepResponderViewModel, CreateUpdateWorkFlowStepResponder>();
            CreateMap<CreateUpdateWorkFlowStepResponder, WorkflowStepResponder>();

            CreateMap<WorkflowStepOption, WorkflowStepResponder>()
                .ForMember(dest => dest.WorkflowStepID, o => o.MapFrom(src => src.WorkflowStepID))
                .ForMember(dest => dest.WorkflowStepOptionID, o => o.MapFrom(src => src.OptionID))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserName()));

            //WorkFlowInstanceActionHistory
            CreateMap<WorkFlowInstanceActionHistoryViewModel, CreateUpdateWorkFlowInstanceActionHistory>();
            CreateMap<CreateUpdateWorkFlowInstanceActionHistory, WorkflowInstanceActionHistory>();
            CreateMap<API.Models.Dashboard, Models.Dashboard>();
            CreateMap<Models.Dashboard, API.Models.Dashboard>();
        }

        private string GetUserId()
        {
            var userId = string.Empty;
            var httpContext = new HttpContextAccessor().HttpContext;


            var currentUser = httpContext.User;


            if (currentUser != null &&
                currentUser.Identity != null &&
                currentUser.Identity.Name != null)
            {
                string? user = currentUser.Identity.Name.ToString();
                if (user != null)
                {
                    var userInfo = user.Split('\\');
                    if (userInfo.Count() > 0)
                    {
                        userId = userInfo[1];
                    }
                    else
                    {
                        userId = userInfo[0];
                    }
                }
            }
            return userId;
        }

        private string GetUserName()
        {
            var userName = ActiveDirectory.GetDisplayName(GetUserId());
            if (userName.Contains(','))
            {
                string[] UserDisplayName = userName.Split(',');
                return $"{UserDisplayName[1].Trim()} {UserDisplayName[0].Trim()} ";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}