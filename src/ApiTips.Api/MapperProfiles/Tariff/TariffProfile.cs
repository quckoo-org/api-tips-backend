using ApiTips.Api.Extensions.Grpc;
using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Tariff;

public class TariffProfile : Profile
{
    public TariffProfile()
    {
        CreateMap<Dal.schemas.data.Tariff, Api.Tariff.V1.Tariff>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Name, opt =>
                opt.MapFrom(src => src.Name))
             .ForMember(dst => dst.TipPrice, opt =>
             {
                 opt.PreCondition(src => src.TipPrice != null);
                 opt.MapFrom(src => src.TipPrice!.Value.ToDecimal());
             })
             .ForMember(dst => dst.FreeTipsCount, opt =>
                opt.MapFrom(src => src.FreeTipsCount))
             .ForMember(dst => dst.PaidTipsCount, opt =>
                opt.MapFrom(src => src.PaidTipsCount))
             .ForMember(dst => dst.TotalTipsCount, opt =>
                opt.MapFrom(src => src.TotalTipsCount))
             .ForMember(dst => dst.TotalPrice, opt =>
                opt.MapFrom(src => src.TotalPrice.ToDecimal()))
             .ForMember(dst => dst.StartDate, opt =>
                 opt.MapFrom<TariffResolverProfile.StartDateTimeDtResolver>())
             .ForMember(dst => dst.EndDate, opt =>
             {
                 opt.PreCondition(src => src.EndDateTime != null);
                 opt.MapFrom<TariffResolverProfile.EndDateTimeDtResolver>();
             })
            .ForMember(dst => dst.HiddenAt, opt =>
            {
                opt.PreCondition(src => src.HideDateTime != null);
                opt.MapFrom<TariffResolverProfile.HideDateTimeDtResolver>();
            })
            ;
    }
}
