using AutoMapper;
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
            CreateMap<WorkFlowViewModel, Workflow>();
            CreateMap<CreateUpdateWorkFlow, Workflow>();

            //WorkFlowStakeHolder
            CreateMap<WorkFlowStakeholderViewModel, CreateUpdateWorkFlowStakeholder>();
            CreateMap<WorkFlowStakeholderViewModel, WorkflowStakeholder>();
            CreateMap<CreateUpdateWorkFlowStakeholder, WorkflowStakeholder>();

            //WorkFlowStep
            CreateMap<WorkFlowStepViewModel, CreateUpdateWorkFlowStep>();
            CreateMap<CreateUpdateWorkFlowStep, WorkflowStep>();
            CreateMap<WorkFlowStepViewModel, WorkflowStep>();

            //WorkFlowInstance
            CreateMap<WorkFlowInstanceViewModel, CreateUpdateWorkFlowInstance>();
            CreateMap<WorkFlowInstanceViewModel, WorkflowInstance>();
            CreateMap<CreateUpdateWorkFlowInstance, WorkflowInstance>();
            CreateMap<WorkflowStep, WorkflowInstance>();

            //WorkFlowStepOption
            CreateMap<WorkFlowStepOptionViewModel, CreateUpdateWorkFlowStepOption>();
            CreateMap<CreateUpdateWorkFlowStepOption, WorkflowStepOption>();
            CreateMap<WorkflowStepOption, CreateUpdateWorkFlowStepOption>();
            CreateMap<WorkflowStepOption, WorkFlowStepOptionViewModel>();
            CreateMap<WorkFlowStepOptionViewModel, WorkflowStepOption>();


            //WorkFlowStepResponder
            CreateMap<WorkFlowStepResponderViewModel, CreateUpdateWorkFlowStepResponder>();
            CreateMap<WorkFlowStepResponderViewModel, WorkflowStepResponder>();
            CreateMap<CreateUpdateWorkFlowStepResponder, WorkflowStepResponder>();
           
            CreateMap<WorkFlowStepResponderViewModel, WorkflowStepResponder>();
            CreateMap<WorkflowStepResponder, WorkFlowStepResponderViewModel>();

            CreateMap<WorkflowStepOption, WorkflowStepResponder>()
                .ForMember(dest => dest.WorkflowStepID, o => o.MapFrom(src => src.WorkflowStepID));

           
            CreateMap<WorkFlowStepOptionViewModel, WorkflowStepResponder>();

            //WorkFlowInstanceActionHistory
            CreateMap<WorkflowInstance, CreateUpdateWorkFlowInstanceActionHistory>()
                .ForMember(dest => dest.WorkflowInstanceID, o => o.MapFrom(src => src.WorkflowInstanceID))
                .ForMember(dest => dest.WorkflowStepID, o => o.MapFrom(src => src.CurrentWorkflowStepID))
                .ForMember(dest => dest.Completed, o => o.MapFrom(src => DateTime.Now));

            CreateMap<WorkflowInstance, WorkflowInstanceActionHistory>()
                .ForMember(dest => dest.WorkflowInstanceID, o => o.MapFrom(src => src.WorkflowInstanceID))
                .ForMember(dest => dest.WorkflowStepID, o => o.MapFrom(src => src.CurrentWorkflowStepID))
                .ForMember(dest => dest.Completed, o => o.MapFrom(src => DateTime.Now));


            CreateMap<WorkFlowInstanceActionHistoryViewModel, CreateUpdateWorkFlowInstanceActionHistory>();
            CreateMap<CreateUpdateWorkFlowInstanceActionHistory, WorkflowInstanceActionHistory>();
            CreateMap<API.Models.Dashboard, Models.Dashboard>();
            CreateMap<Models.Dashboard, API.Models.Dashboard>();
        }

        

        
    }
}
