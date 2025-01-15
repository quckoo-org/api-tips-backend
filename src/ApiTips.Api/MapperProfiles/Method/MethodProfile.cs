using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Method;

public class MethodProfile: Profile
{
    public MethodProfile()
    {
        CreateMap<Dal.schemas.system.Method,Access.V1.Method>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Name, opt =>
                opt.MapFrom(src => src.Name))
            ;
    }
}