using AutoMapper;
using DalNS = ApiTips.Dal.schemas.data;
using GrpcInvoice = ApiTips.Api.Invoices.V1;
using ApiTips.Api.Extensions.Grpc;
using ApiTips.Dal.Enums;
using Google.Protobuf.WellKnownTypes;

namespace ApiTips.Api.MapperProfiles.Invoice;

public class InvoiceProfile : Profile
{
    public InvoiceProfile()
    {
        CreateMap<Dal.Enums.PaymentType, CustomEnums.V1.PaymentType>().ConvertUsing((value, dest) =>
        {
            switch (value)
            {
                case PaymentType.Bank:
                    return CustomEnums.V1.PaymentType.Bank;
                case PaymentType.Crypto:
                    return CustomEnums.V1.PaymentType.Crypto;
                default:
                    return CustomEnums.V1.PaymentType.Unspecified;
            }
        });
        CreateMap<DalNS.Invoice, GrpcInvoice.Invoice>()
            .ForMember(dst => dst.Guid, opt =>
                opt.MapFrom(src => src.Id))
            .ForPath(dst => dst.InvoiceOwner, opt =>
                opt.MapFrom(src => src.Order.User))
            .ForMember(dst => dst.Alias, opt =>
                opt.MapFrom(src => src.Alias))
            .ForMember(dst => dst.RefNumber, opt =>
                opt.MapFrom(src => src.RefNumber))
            .ForMember(dst => dst.TotalAmount, opt =>
                opt.MapFrom(src => src.CurrentCurrency.TotalAmount.ToDecimal()))
            .ForMember(dst => dst.PaymentType, opt =>
                opt.MapFrom(src => src.CurrentCurrency.Type))
            .ForMember(dst => dst.AmountOfRequests, opt =>
                opt.MapFrom(src => src.AmountOfRequests))
            .ForMember(dst => dst.CreatedDate, opt =>
                opt.MapFrom(src => src.CreatedAt.ToTimestamp()))
            .ForMember(dst => dst.PaymentDate, opt =>
            {
                opt.PreCondition(src => src.PayedAt != null);
                opt.MapFrom(src => src.CreatedAt.ToTimestamp());
            })
            ;

        CreateMap<Dal.schemas.system.User, GrpcInvoice.User>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Email, opt =>
                opt.MapFrom(src => src.Email))
            .ForMember(dst => dst.FirstName, opt =>
                opt.MapFrom(src => src.FirstName))
            .ForMember(dst => dst.LastName, opt =>
                opt.MapFrom(src => src.LastName))
            ;
    }
}