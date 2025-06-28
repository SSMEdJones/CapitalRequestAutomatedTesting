using AutoMapper;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;

namespace CapitalRequestAutomatedTesting.UI.AutoMapper.MappingProfile
{
    public class AutomatedTestingProfile : Profile
    {
        public AutomatedTestingProfile() 
        {
            CreateMap<ScenarioDetailsViewModel, ScenarioDetails>()
                .ForMember(dest => dest.ReviewerId, opt => opt.MapFrom(src => src.ReviewerId ?? 0))
                .ForMember(dest => dest.RequestingGroupId, opt => opt.MapFrom(src => src.RequestingGroupId ?? 0))
                .ForMember(dest => dest.TargetGroupId, opt => opt.MapFrom(src => src.TargetGroupId ?? 0))
                .ForMember(dest => dest.ActualData, opt => opt.MapFrom(src => src.ActualData))
                .ForMember(dest => dest.PredictiveData, opt => opt.MapFrom(src => src.PredictiveData));

            CreateMap<ScenarioDetailsViewModel, ScenarioDetails>()
             // your `.ForMember(...)` mappings
             .ReverseMap();

        }
    }
}
