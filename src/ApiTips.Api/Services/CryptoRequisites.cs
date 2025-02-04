using ApiTips.Api.Enums;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal.Enums;

namespace ApiTips.Api.Services;

public class CryptoRequisites : IRequisites
{
    /// <summary>
    ///     Адрес крипто-кошелька
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    ///     Кошелёк для оплаты
    /// </summary>
    public string Wallet { get; set; }

    /// <summary>
    ///     Токен для проведения оплаты
    /// </summary>
    public string Token { get; set; }
    
    /// <summary>
    ///     
    /// </summary>
        
    public string Crypto { get; set; }


    public PaymentTypeEnum PaymentType { get; set; } = PaymentTypeEnum.Cryptocurrency;
    public bool IsBanned { get; set; } = false;
}