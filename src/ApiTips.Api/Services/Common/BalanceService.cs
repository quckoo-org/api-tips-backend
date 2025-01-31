using ApiTips.Api.Balance.V1;
using ApiTips.Api.MapperProfiles.Balance;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal;
using ApiTips.Dal.Enums;
using ApiTips.Dal.schemas.data;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Api.Services.Common;

public class BalanceService : IBalanceService
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<BalanceService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    public BalanceService(IHostEnvironment env, ILogger<BalanceService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(BalanceProfile));
        });

        if (env.IsDevelopment())
        {
            config.CompileMappings();
            config.AssertConfigurationIsValid();
        }

        _mapper = new Mapper(config);
    }

    /// <summary>
    ///     Зарегистрированные сервисы
    /// </summary>
    private IServiceProvider Services { get; }

    public async Task<List<History>?> GetHistoriesByMonth(
        DateTime startDate, DateTime endDate, CancellationToken token, long? userId = null)
    {
        List<History> result = [];

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var query = applicationContext
            .BalanceHistories
            .AsNoTracking();

        if (userId is not null)
            query = query
                .Include(x => x.Balance)
                .ThenInclude(x => x.User)
                .Where(x => x.OperationDateTime >= startDate
                            && x.OperationDateTime <= endDate
                            && x.Balance.User.Id == userId.Value);
        else
            query = query
                .Where(x => x.OperationDateTime >= startDate
                            && x.OperationDateTime <= endDate);

        var histories = await query.ToListAsync(token);

        if (histories.Count == 0)
        {
            _logger.LogWarning("В базе нет истории баланса за указанный период {Start} - {End}",
                startDate.ToShortDateString(), endDate.ToShortDateString());
            return null;
        }

        var historiesByMonth = histories
            .OrderBy(x => x.OperationDateTime)
            .GroupBy(x => new { x.OperationDateTime.Year, x.OperationDateTime.Month });

        foreach (var historyGroup in historiesByMonth)
        {
            //Создание объекта агрегированной истории баланса по месяцам
            var historyByMonth = new History
            {
                StartDate = historyGroup.FirstOrDefault()?.OperationDateTime.ToTimestamp(),
                EndDate = historyGroup.LastOrDefault()?.OperationDateTime.ToTimestamp()
            };

            var historyData = new HistoryData();

            foreach (var history in historyGroup)
                switch (history.OperationType)
                {
                    case BalanceOperationType.Crediting:
                        if (history.FreeTipsCountChangedTo is not null)
                            historyData.CreditedFreeTipsCount += history.FreeTipsCountChangedTo.Value;
                        if (history.PaidTipsCountChangedTo is not null)
                            historyData.CreditedPaidTipsCount += history.PaidTipsCountChangedTo.Value;
                        break;

                    case BalanceOperationType.Debiting:
                        if (history.TotalTipsCountChangedTo is not null)
                            historyData.DebitedTipsCount += history.TotalTipsCountChangedTo.Value;
                        break;
                    default:
                        _logger.LogWarning("Неизвестный тип операции {OperationType}", history.OperationType);
                        break;
                }

            historyByMonth.HistoryData.Add(historyData);

            //Добавляем агрегированную историю за месяц в ответ
            result.Add(historyByMonth);
        }

        return result;
    }

    public async Task<List<History>?> GetHistoriesByUsers(
        DateTime startDate, DateTime endDate, CancellationToken token)
    {
        List<History> result = [];

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var histories = await applicationContext
            .BalanceHistories
            .Include(x => x.Balance)
            .ThenInclude(x => x.User)
            .AsNoTracking()
            .Where(x => x.OperationDateTime >= startDate && x.OperationDateTime <= endDate)
            .ToListAsync(token);

        if (histories.Count == 0)
        {
            _logger.LogWarning("В базе нет истории баланса за указанный период {Start} - {End}",
                startDate.ToShortDateString(), endDate.ToShortDateString());
            return null;
        }

        var historiesByUser = histories.GroupBy(x => new { UserId = x.Balance.User.Id });

        //Создание объекта агрегированной истории баланса по пользователям
        var historyByUsers = new History
        {
            StartDate = startDate.ToTimestamp(),
            EndDate = endDate.ToTimestamp()
        };

        foreach (var historyGroup in historiesByUser)
        {
            var historyData = new HistoryData
            {
                User = _mapper.Map<User>(historyGroup.FirstOrDefault()?.Balance.User)
            };

            foreach (var history in historyGroup)
                switch (history.OperationType)
                {
                    case BalanceOperationType.Crediting:
                        if (history.FreeTipsCountChangedTo is not null)
                            historyData.CreditedFreeTipsCount += history.FreeTipsCountChangedTo.Value;
                        if (history.PaidTipsCountChangedTo is not null)
                            historyData.CreditedPaidTipsCount += history.PaidTipsCountChangedTo.Value;
                        break;

                    case BalanceOperationType.Debiting:
                        if (history.TotalTipsCountChangedTo is not null)
                            historyData.DebitedTipsCount += history.TotalTipsCountChangedTo.Value;
                        break;
                }

            historyByUsers.HistoryData.Add(historyData);
        }

        //Добавляем агрегированную историю по всем пользователям за указанный период в ответ
        result.Add(historyByUsers);

        return result;
    }

    public async Task<bool> AddBalance(long userId, CancellationToken token)
    {
        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var user = await applicationContext
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, token);

        if (user is null)
        {
            _logger.LogWarning("В базе нет пользователя с идентификатором {Id}",
                userId);

            return false;
        }

        applicationContext.Balances.Add(new Dal.schemas.data.Balance()
        {
            User = user
        });

        if (await applicationContext.SaveChangesAsync(token) > 0)
            return true;

        return false;
    }

    public async Task<BalanceHistory?> UpdateBalance(
        long balanceId,
        BalanceOperationType operationType,
        string reason,
        CancellationToken token,
        long? freeTipsCount = null,
        long? paidTipsCount = null)
    {
        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var balance = await applicationContext.Balances
            .FirstOrDefaultAsync(x => x.Id == balanceId, token);

        if (balance is null)
        {
            _logger.LogWarning("Баланса с идентификатором {Id} не существует", balanceId);

            return null;
        }

        var balanceHistoryCandidate = new BalanceHistory
        {
            OperationType = operationType,
            ReasonDescription = reason,
            TotalTipsBalance = 0,
            Balance = balance
        };

        switch (operationType)
        {
            case BalanceOperationType.Crediting:
                //Пополнение бесплатных подсказок
                balance.FreeTipsCount += freeTipsCount ?? 0;
                balanceHistoryCandidate.FreeTipsCountChangedTo = freeTipsCount;

                //Пополнение платных подсказок
                balance.PaidTipsCount += paidTipsCount ?? 0;
                balanceHistoryCandidate.PaidTipsCountChangedTo = paidTipsCount;
                break;
            case BalanceOperationType.Debiting:
                //На данный момент договорённость, что не опускаем значение баланса ниже 0
                if (freeTipsCount is not null)
                {
                    //Списание бесплатных подсказок
                    if (freeTipsCount > balance.FreeTipsCount)
                    {
                        _logger.LogWarning("Попытка списать {Debit} бесплатных подсказок, a на балансе {Balance}",
                            freeTipsCount, balance.FreeTipsCount);

                        balanceHistoryCandidate.FreeTipsCountChangedTo = balance.FreeTipsCount;
                        balance.FreeTipsCount = 0;
                    }
                    else
                    {
                        balanceHistoryCandidate.FreeTipsCountChangedTo = freeTipsCount;
                        balance.FreeTipsCount -= freeTipsCount.Value;
                    }
                }

                if (paidTipsCount is not null)
                {
                    //Списание платных подсказок
                    if (paidTipsCount > balance.PaidTipsCount)
                    {
                        _logger.LogWarning("Попытка списать {Debit} платных подсказок, a на балансе {Balance}",
                            paidTipsCount, balance.PaidTipsCount);

                        balanceHistoryCandidate.PaidTipsCountChangedTo = balance.PaidTipsCount;
                        balance.PaidTipsCount = 0;
                    }
                    else
                    {
                        balanceHistoryCandidate.PaidTipsCountChangedTo = paidTipsCount;
                        balance.PaidTipsCount -= paidTipsCount.Value;
                    }
                }
                break;
            default:
                _logger.LogError("Указан неизвестный тип операции");
                return null;
        }

        balanceHistoryCandidate.TotalTipsBalance = balance.TotalTipsCount;

        var balanceHistoryResult = await applicationContext.BalanceHistories
            .AddAsync(balanceHistoryCandidate, token);

        if (await applicationContext.SaveChangesAsync(token) > 0)
        {
            return balanceHistoryResult.Entity;
        }

        return null;
    }
}
