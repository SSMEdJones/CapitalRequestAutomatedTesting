using AutoMapper;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequest.API.DataAccess.AutoMapper.MappingProfile
{
    public class CapitalRequestProfile : Profile
    {
        public CapitalRequestProfile()
        {
            CreateMap<dto.ApplicationUser, vm.ApplicationUser>();
            CreateMap<dto.Proposal, vm.Proposal>();
            CreateMap<dto.RequestedInfo, vm.RequestedInfo>();
            CreateMap<dto.ProvidedInfo, vm.ProvidedInfo>();
            CreateMap<dto.ReviewerGroup, vm.ReviewerGroup>();
            CreateMap<dto.Reviewer, vm.Reviewer>();
            CreateMap<dto.Wbs, vm.Wbs>();
            CreateMap<dto.Attachment, vm.Attachment>();
            CreateMap<dto.Quote, vm.Quote>();
            CreateMap<dto.WorkflowTemplate, vm.WorkflowTemplate>();
            CreateMap<dto.WorkflowAction, vm.WorkflowAction>();
            CreateMap<dto.ApplicationUser, vm.ApplicationUser>();
        }

    }
}
