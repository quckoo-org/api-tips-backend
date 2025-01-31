using ApiTips.Api.Balance.V1;
using ApiTips.Dal.schemas.data;
using ApiTips.Dal.Enums;

namespace ApiTips.Api.ServiceInterfaces;

public interface IBalanceService
{
    /// <summary>
    ///     Получение истории операций сгруппированных по месяцам для пользователя/пользователей
    /// </summary>
    Task<List<History>?> GetHistoriesByMonth(DateTime startDate, DateTime endDate,
        CancellationToken token, long? userId = null);

    /// <summary>
    ///     Получение истории операций сгруппированных по пользователям
    /// </summary>
    Task<List<History>?> GetHistoriesByUsers(DateTime startDate, DateTime endDate, CancellationToken token);

    /// <summary>
    ///     Создание баланса для пользователя
    /// </summary>
    Task<bool> AddBalance(long userId, CancellationToken token);

    /// <summary>
    ///     Изменение баланса
    /// </summary>
    Task<BalanceHistory?> UpdateBalance(long balanceId, BalanceOperationType operationType, string reason,
        CancellationToken token, long? freeTipsCount = null, long? paidTipsCount = null);
}

