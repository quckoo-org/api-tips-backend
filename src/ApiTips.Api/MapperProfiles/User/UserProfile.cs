using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace ApiTips.Api.MapperProfiles.User;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Dal.schemas.system.User, Access.V1.User>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Email, opt =>
                opt.MapFrom(src => src.Email))
            .ForMember(dst => dst.FirstName, opt =>
                opt.MapFrom(src => src.FirstName))
            .ForMember(dst => dst.LastName, opt =>
                opt.MapFrom(src => src.LastName))
            .ForMember(dst => dst.Cca3, opt =>
                opt.MapFrom(src => src.Cca3))
            .ForMember(dst => dst.CreatedAt, opt =>
                opt.MapFrom(src => src.CreateDateTime.ToTimestamp()))
            .ForMember(dst => dst.BlockedAt, opt =>
            {
                opt.PreCondition(src => src.LockDateTime != null);
                opt.MapFrom<UserResolverProfile.LockDateTimeDtResolver>();
            })
            .ForMember(dst => dst.DeletedAt, opt =>
            {
                opt.PreCondition(src => src.DeleteDateTime != null);
                opt.MapFrom<UserResolverProfile.DeleteDateTimeDtResolver>();
            })
            .ForMember(dst => dst.VerifiedAt, opt =>
            {
                opt.PreCondition(src => src.VerifyDateTime != null);
                opt.MapFrom<UserResolverProfile.VerifyDateTimeDtResolver>();
            })
            .ForPath(dst => dst.Roles, opt =>
                opt.MapFrom(src => src.Roles))
            ;
        CreateMap<Dal.schemas.system.User, Access.V1.DetailedUser>()
            .ForMember(dst => dst.User, opt =>
                opt.MapFrom(src => src))
            .ForMember(dst => dst.Balance, opt =>
            {
                opt.PreCondition(src => src.Balance != null);
                opt.MapFrom(src => src.Balance.TotalTipsCount);
            })
            .ForMember(dst => dst.AccessToken, opt =>
                opt.MapFrom(src => src.AccessToken))
            ;
    }
}