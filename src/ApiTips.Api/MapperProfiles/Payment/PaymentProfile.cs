using ApiTips.Api.Requisites.V1;
using ApiTips.Api.Services;
using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Payment;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMapFromDbCryptoWalletToApiResponseWallet();
        
        CreateMapFromDbBankWalletToApiResponseWallet();
    }
    
    /// <summary>
    ///     Маппинг сущностей из реквизитов крипто-кошелька в базе данных в реквизиты крипто-кошелька для ответа
    /// </summary>
    private void CreateMapFromDbCryptoWalletToApiResponseWallet()
    {
        CreateMap<Dal.schemas.data.Requisite.PaymentDetails.CryptoWallet, CryptoWallet>()
            .ForMember(dst => dst.Address, opt =>
                opt.MapFrom(src => src.Address))
            .ForMember(dst => dst.IsBanned, opt =>
                opt.Ignore())
            .ForMember(dst => dst.Token, opt =>
                opt.MapFrom(src => src.Token))
            .ForMember(dst => dst.CryptoCurrencyType, opt =>
                opt.MapFrom(src => src.Type))
            ;
    }
    
    /// <summary>
    ///     Маппинг сущностей из банковских реквизитов в базе данных в банковские реквизиты для ответа
    /// </summary>
    private void CreateMapFromDbBankWalletToApiResponseWallet()
    {
        CreateMap<Dal.schemas.data.Requisite.PaymentDetails.BankAccount, BankAccount>()
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
                opt.MapFrom(src => src.Type))
            
            .ForMember(dst => dst.AdditionalInfo, opt =>
                opt.MapFrom(src => src.AdditionalInfo))
            
            .ForMember(dst => dst.IsBanned, opt =>
                opt.Ignore())
            ;
    }
}
