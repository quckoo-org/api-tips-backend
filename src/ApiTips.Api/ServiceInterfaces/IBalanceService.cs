using ApiTips.Api.Balance.V1;
using ApiTips.Dal.schemas.data;
using ApiTips.Dal.Enums;
using ApiTips.Dal;

namespace ApiTips.Api.ServiceInterfaces;

public interface IBalanceService
{
    /// <summary>
    ///     Получение истории операций агрегированной по годам
    /// </summary>
    Task<List<Year>?> GetHistories(DateTime startDate, DateTime endDate,
        CancellationToken token, List<long>? aggregateUserIds = null, List<long>? detailedUserIds = null);

    /// <summary>
    ///     Создание баланса для пользователя
    /// </summary>
    Task<bool> AddBalance(ApplicationContext applicationContext, long userId, CancellationToken token);

    /// <summary>
    ///     Пополнение подсказок на балансе
    /// </summary>
    Task<BalanceHistory?> CreditTipsToBalance(ApplicationContext applicationContext, long balanceId, string reason,
        CancellationToken token, long? creditedFreeTipsCount = null, long? creditedPaidTipsCount = null);

    /// <summary>
    ///     Списание подсказок с баланса
    /// </summary>
    Task<BalanceHistory?> DebitTipsFromBalance(ApplicationContext applicationContext, long balanceId, string reason,
        CancellationToken token, long debitedTipsCount);
}

