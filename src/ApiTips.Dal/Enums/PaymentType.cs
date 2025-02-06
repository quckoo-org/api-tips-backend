namespace ApiTips.Dal.Enums;

public enum PaymentType
{
    /// <summary>
    ///     Неопределённый статус, который нельзя присвоить реквизитам
    /// </summary>
    Unspecified = 0,

    /// <summary>
    ///     Способ оплаты через крипто-кошелек
    /// </summary>
    Crypto = 1,

    /// <summary>
    ///     Тип оплаты через банковские реквизиты
    /// </summary>
    Bank = 2
}