namespace ApiTips.Dal.Enums;

/// <summary>
///     Виды операций изменения баланса
/// </summary>
public enum BalanceOperationType
{
    /// <summary>
    ///     Пополнение подсказок на балансе (покупка)
    /// </summary>
    Crediting = 1,

    /// <summary>
    ///     Списание подсказок с баланса
    /// </summary>
    Debiting = 2,
}
