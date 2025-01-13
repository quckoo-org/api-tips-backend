using AutoMapper;

namespace ApiTips.Api.MapeerProfiles.Role;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Dal.schemas.system.Role,Access.V1.Role>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Name, opt =>
                opt.MapFrom(src => src.Name))
            .ForPath(dst => dst.Permissions, opt =>
                opt.MapFrom(src => src.Permissions))
            ;
    }
}