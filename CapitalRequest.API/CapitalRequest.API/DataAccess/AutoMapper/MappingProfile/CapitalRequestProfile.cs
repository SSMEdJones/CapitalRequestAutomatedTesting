using AutoMapper;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;
using CapitalRequest.API.DataAccess.Utilities;

namespace CapitalRequest.API.DataAccess.AutoMapper.MappingProfile
{
    public class CapitalRequestProfile : Profile
    {
        public CapitalRequestProfile()
        {
            CreateMap<dto.ApplicationUser, vm.ApplicationUser>();
            //CreateMap<dto.Proposal, vm.Proposal>();
            CreateMap<vm.Proposal, dto.Proposal>();

            CreateMap<dto.Proposal, vm.Proposal>()
                           .ForMember(dest => dest.CostCenter, o => o.MapFrom(src => src.CostCenter.ToString()))
                           .ForMember(dest => dest.TotalProjectCost, o => o.MapFrom(src => Money.Convert(src.TotalProjectCost)))
                           .ForMember(dest => dest.SalesTax, o => o.MapFrom(src => Money.Convert(src.SalesTax)))
                           .ForMember(dest => dest.FreightAmount, o => o.MapFrom(src => Money.Convert(src.FreightAmount)))
                           .ForMember(dest => dest.TotalCapital, o => o.MapFrom(src => Money.Convert(src.TotalCapital)))
                           .ForMember(dest => dest.Year1, o => o.MapFrom(src => Money.Convert(src.Year1)))
                           .ForMember(dest => dest.Year2, o => o.MapFrom(src => Money.Convert(src.Year2)))
                           .ForMember(dest => dest.Year3, o => o.MapFrom(src => Money.Convert(src.Year3)))
                           .ForMember(dest => dest.NetPresentValue, o => o.MapFrom(src => Money.Convert(src.NetPresentValue)))
                           .ForMember(dest => dest.InternalRateOfReturn, o => o.MapFrom(src => Percentage.Convert(src.InternalRateOfReturn)))
                           .ForMember(dest => dest.IsMovingForward, o => o.MapFrom(src => src.IsMovingForward == null ? true : src.IsMovingForward))
                           .ForMember(dest => dest.SubmitButtonPressed, o => o.MapFrom(src => false))
                           .ForMember(dest => dest.ReturnOnInvestment, o => o.MapFrom(src => Percentage.Convert(src.ReturnOnInvestment)))
                           .ForMember(dest => dest.OriginalCapitalFundingYear, o => o.MapFrom(src => src.CapitalFundingYear))
                           .ForMember(dest => dest.IncludePurchasingGroup, o => o.MapFrom(src => src.IncludePurchasingGroup == null ? true : src.IncludePurchasingGroup));

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

            //Predictive models
            CreateMap<vm.RequestedInfo, dto.RequestedInfo>();
            //using dto = CapitalRequest.API.DataAccess.Models;
            //using vm = CapitalRequest.API.Models;

            //CapitalRequest.API.Models.RequestedInfo->CapitalRequest.API.DataAccess.Models.RequestedInfo
        }

    }
}
