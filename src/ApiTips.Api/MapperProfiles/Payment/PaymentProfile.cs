using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Payment;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Dal.schemas.data.Payment, Api.Payment.V1.Payment>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.PaymentTypeCase, opt =>
                opt.Ignore())
            .ForMember(dst => dst.IsBanned, opt =>
                opt.MapFrom(src => src.IsBanned))
            .ForMember(dst => dst.BankAccount, opt =>
            {
                opt.PreCondition(x => x.Details?.BankAccountDetails is not null);
                opt.MapFrom(src => src.Details.BankAccountDetails);
            })
            .ForMember(dst => dst.CryptoWallet, opt =>
            {
                opt.PreCondition(x => x.Details?.CryptoWalletDetails is not null);
                opt.MapFrom(src => src.Details.CryptoWalletDetails);
            })
            ;

        CreateMap<Dal.schemas.data.Payment.PaymentDetails.BankAccount, Api.Payment.V1.BankAccount>()
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
            .ForMember(dst => dst.AdditionalInfo, opt => 
                opt.MapFrom(src => src.AdditionalInfo))
            .ForMember(dst => dst.CurrencyType, opt => 
                opt.MapFrom(src => src.Type))
            ;
        
        
        CreateMap<Dal.schemas.data.Payment.PaymentDetails.CryptoWallet, Api.Payment.V1.CryptoWallet>()
            .ForMember(dst => dst.Address, opt => 
                opt.MapFrom(src => src.Аddress))
            .ForMember(dst => dst.Wallet, opt => 
                opt.MapFrom(src => src.Wallet))
            .ForMember(dst => dst.Token, opt => 
                opt.MapFrom(src => src.Token))
            .ForMember(dst => dst.CryptoCurrencyType, opt => 
                opt.MapFrom(src => src.Type))
            ;
        
        CreateMap<Api.Payment.V1.Payment, Dal.schemas.data.Payment>()
            .ForMember(dst => dst.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.PaymentType, opt =>
                opt.MapFrom<PaymetResolverProfile.PaymentTypeResolver>())
            .ForMember(dst => dst.IsBanned, opt =>
                opt.MapFrom(src => src.IsBanned))
            .ForPath(dst => dst.Details.BankAccountDetails, opt =>
            {
                opt.MapFrom(src => src.BankAccount);
            })
            .ForPath(dst => dst.Details.CryptoWalletDetails, opt =>
            {
                opt.MapFrom(src => src.CryptoWallet);
            })
            .ForMember(dst => dst.User, opt =>
                opt.Ignore())
            ;
        
        CreateMap<Api.Payment.V1.BankAccount, Dal.schemas.data.Payment.PaymentDetails.BankAccount>()
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
            .ForMember(dst => dst.AdditionalInfo, opt => 
                opt.MapFrom(src => src.AdditionalInfo))
            .ForMember(dst => dst.Type, opt => 
                opt.MapFrom(src => src.CurrencyType))
            ;
        
        
        CreateMap<Api.Payment.V1.CryptoWallet, Dal.schemas.data.Payment.PaymentDetails.CryptoWallet>()
            .ForMember(dst => dst.Аddress, opt => 
                opt.MapFrom(src => src.Address))
            .ForMember(dst => dst.Wallet, opt => 
                opt.MapFrom(src => src.Wallet))
            .ForMember(dst => dst.Token, opt => 
                opt.MapFrom(src => src.Token))
            .ForMember(dst => dst.Type, opt => 
                opt.MapFrom(src => src.CryptoCurrencyType))
            ;

    }
}