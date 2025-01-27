using ApiTips.Api.Payment.V1;
using ApiTips.Api.Services;
using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Payment;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<CryptoRequisites, CryptoWallet>()
            .ForMember(dst => dst.Address, opt =>
                opt.MapFrom(src => src.Address))
            .ForMember(dst => dst.IsBanned, opt =>
                opt.MapFrom(src => src.IsBanned))
            .ForMember(dst => dst.Token, opt =>
                opt.MapFrom(src => src.Token))
            .ForMember(dst => dst.CryptoCurrencyType, opt =>
                opt.MapFrom(src => src.Crypto))
            ;
        CreateMap<BankRequisites, BankAccount>()
            .ForMember(dst => dst.BankName, opt =>
                opt.MapFrom(src => src.BankName))
            .ForMember(dst => dst.BankAddress, opt =>
                opt.MapFrom(src => src.BankAddress))
            .ForMember(dst => dst.Swift, opt =>
                opt.MapFrom(src => src.Swift))
            .ForMember(dst => dst.Iban, opt =>
                opt.MapFrom(src => src.Iban))
            .ForMember(dst => dst.AccountNumber, opt =>
                opt.MapFrom(src => src.AccountNumber))
            .ForMember(dst => dst.CurrencyType, opt =>
                opt.MapFrom(src => src.AdditionalInfo))
            .ForMember(dst => dst.AdditionalInfo, opt =>
                opt.MapFrom(src => src.Swift))
            .ForMember(dst => dst.IsBanned, opt =>
                opt.MapFrom(src => src.IsBanned))
            ;
    }
}