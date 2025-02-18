using ApiTips.Api.Extensions.Grpc;
using AutoMapper;
using DalOrderStatus = ApiTips.Dal.Enums.OrderStatus;
using ProtoOrderStatus = ApiTips.CustomEnums.V1.OrderStatus;

namespace ApiTips.Api.MapperProfiles.Order;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<DalOrderStatus, ProtoOrderStatus>().ConvertUsing((value, dest) =>
        {
            switch (value)
            {
                case DalOrderStatus.Created:
                    return ProtoOrderStatus.Created;
                case DalOrderStatus.Paid:
                    return ProtoOrderStatus.Paid;
                case DalOrderStatus.Cancelled:
                    return ProtoOrderStatus.Cancelled;
                default:
                    return ProtoOrderStatus.Unspecified;
            }
        });

        CreateMap<Dal.schemas.data.Order, Api.Order.V1.Order>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
             .ForMember(dst => dst.CreatedAt, opt =>
                 opt.MapFrom<OrderResolverProfile.CreateDateTimeDtResolver>())
             .ForMember(dst => dst.OrderStatus, opt =>
                opt.MapFrom(src => src.Status))
             .ForMember(dst => dst.PaidAt, opt =>
             {
                 opt.PreCondition(src => src.PaymentDateTime != null);
                 opt.MapFrom<OrderResolverProfile.PaymentDateTimeDtResolver>();
             })
             .ForMember(dst => dst.Tariff, opt =>
                opt.MapFrom(src => src.Tariff))
             .ForMember(dst => dst.User, opt =>
                opt.MapFrom(src => src.User))
            .ForMember(dst => dst.Invoice, opt  =>
            {
                opt.PreCondition(src => src.Invoice != null);
                opt.MapFrom(src => src.Invoice);
            })
            ;

        CreateMap<Dal.schemas.data.Tariff, Api.Order.V1.Tariff>()
             .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
             .ForMember(dst => dst.Name, opt =>
                opt.MapFrom(src => src.Name))
             .ForMember(dst => dst.TotalTipsCount, opt =>
             {
                 opt.PreCondition(src => src.TotalTipsCount != null);
                 opt.MapFrom(src => src.TotalTipsCount);
             })
             .ForMember(dst => dst.TotalPrice, opt =>
                opt.MapFrom(src => src.TotalPrice.ToDecimal()))
             .ForMember(dst => dst.Currency, opt =>
                opt.MapFrom(src => src.Currency))
             ;

        CreateMap<Dal.schemas.system.User, Api.Order.V1.User>()
             .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
             .ForMember(dst => dst.Email, opt =>
                opt.MapFrom(src => src.Email))
             .ForMember(dst => dst.FirstName, opt =>
                opt.MapFrom(src => src.FirstName))
             .ForMember(dst => dst.LastName, opt =>
                opt.MapFrom(src => src.LastName))
             ;
        CreateMap<Dal.schemas.data.Invoice, Api.Order.V1.Invoice>()
            .ForMember(dst => dst.Guid, opt =>
                opt.MapFrom(src => src.Id))
            ;
    }
}
