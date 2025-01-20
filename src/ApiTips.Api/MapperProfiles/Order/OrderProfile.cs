using ApiTips.Api.Extensions.Grpc;
using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Order;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        //CreateMap<Dal.schemas.data.Order, Api.Order.V1.Order>()
        //    .ForMember(dst => dst.Id, opt =>
        //        opt.MapFrom(src => src.Id))
        //    ;
    }
}
