using ApiTips.Api.Enums;
using ApiTips.Dal.Enums;

namespace ApiTips.Api.ServiceInterfaces;

public interface IRequisites
{
    /// <summary>
    ///     Тип оплаты
    /// </summary>
    PaymentTypeEnum PaymentType { get; set; }
    
    /// <summary>
    ///     Признак запрета счёта для оплаты
    /// </summary>
    public bool IsBanned { get; set; }
}