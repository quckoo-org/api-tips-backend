using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Permission;

public class PermissionProfile: Profile
{
    public PermissionProfile()
    {
        CreateMap<Dal.schemas.system.Permission,Access.V1.Permission>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Name, opt =>
                opt.MapFrom(src => src.Name))
            .ForPath(dst => dst.Methods, opt =>
                opt.MapFrom(src => src.Methods))
            ;
    }
}