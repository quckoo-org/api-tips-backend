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

    /// <summary>
    /// Получение агрегированной истории изменения баланса
    /// </summary>
    /// <param name="startDate">Дата начала периода сбора истории</param>
    /// <param name="endDate">Дата окончания периода сбора истории</param>
    /// <param name="token">Токен отмены</param>
    /// <param name="aggregateUserIds">Список идентификаторов пользователей по которым нужно собрать историю</param>
    /// <param name="detailedUserIds">Список идентификаторов пользователей по которым нужно собрать детальную историю</param>
    public async Task<List<Year>?> GetHistories(DateTime startDate, DateTime endDate, CancellationToken token,
        List<long>? aggregateUserIds = null, List<long>? detailedUserIds = null)
    {
        // TODO убрать gRPC-модели из сервиса логики

        List<Year> result = [];

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var query = applicationContext
            .BalanceHistories
            .Include(x => x.Balance)
            .ThenInclude(x => x.User)
            .AsNoTracking()
            .Where(x => x.OperationDateTime >= startDate && x.OperationDateTime <= endDate);

        if (aggregateUserIds is not null)
            query = query.Where(x => aggregateUserIds.Contains(x.Balance.User.Id));

        var histories = await query.ToListAsync(token);

        if (histories.Count == 0)
        {
            _logger.LogWarning("В базе нет истории баланса за указанный период {Start} - {End}",
                startDate.ToShortDateString(), endDate.ToShortDateString());
            return null;
        }

        //Сортируем по дате и группируем историю по годам
        var historiesByYear = histories
            .OrderBy(x => x.OperationDateTime)
            .GroupBy(x => x.OperationDateTime.Year);

        foreach (var groupYear in historiesByYear)
        {
            var yearData = groupYear.FirstOrDefault();
            if (yearData is null) continue;

            //Создание объекта агрегированной истории баланса за год
            var year = new Year
            {
                Date = yearData.OperationDateTime.ToTimestamp()
            };

            //Группируем историю за год по месяцам
            var historiesByMonth = groupYear
                .GroupBy(x => x.OperationDateTime.Month);

            foreach (var groupMonth in historiesByMonth)
            {
                var monthData = groupMonth.FirstOrDefault();
                if (monthData is null) continue;

                //Создание объекта агрегированной истории баланса за месяц
                var month = new Month
                {
                    Date = monthData.OperationDateTime.ToTimestamp()
                };

                //Группируем историю за месяц по пользователям
                var historiesByUser = groupMonth
                    .GroupBy(x => x.Balance.User.Id);

                foreach (var groupUser in historiesByUser)
                {
                    var userData = groupUser.FirstOrDefault();
                    if (userData is null) continue;

                    //Создание объекта агрегированной истории баланса за месяц для пользователя
                    var user = new User
                    {
                        Id = userData.Balance.User.Id,
                        FirstName = userData.Balance.User.FirstName,
                        LastName = userData.Balance.User.LastName,
                        Email = userData.Balance.User.Email
                    };

                    if (detailedUserIds is not null && detailedUserIds.Contains(user.Id))
                    {
                        //Группируем историю для пользователя по дням
                        var historiesByDay = groupUser
                            .GroupBy(x => x.OperationDateTime.Day);

                        foreach (var groupDay in historiesByDay)
                        {
                            var dayData = groupDay.FirstOrDefault();
                            if (dayData is null) continue;

                            //Создание объекта агрегированной истории баланса за день для пользователя
                            var day = new Day
                            {
                                Date = dayData.OperationDateTime.ToTimestamp()
                            };

                            foreach (var operation in groupDay)
                            {
                                switch (operation.OperationType)
                                {
                                    case BalanceOperationType.Crediting:
                                        if (operation.FreeTipsCountChangedTo is not null)
                                            day.CreditedFreeTipsCount += operation.FreeTipsCountChangedTo.Value;
                                        if (operation.PaidTipsCountChangedTo is not null)
                                            day.CreditedPaidTipsCount += operation.PaidTipsCountChangedTo.Value;
                                        break;

                                    case BalanceOperationType.Debiting:
                                        if (operation.TotalTipsCountChangedTo is not null)
                                            day.DebitedTipsCount += operation.TotalTipsCountChangedTo.Value;
                                        break;
                                    default:
                                        _logger.LogWarning("Неизвестный тип операции {OperationType}",
                                            operation.OperationType);
                                        break;
                                }

                                day.Operations.Add(_mapper.Map<Operation>(operation));
                            }

                            if (day.HasCreditedFreeTipsCount) user.CreditedFreeTipsCount += day.CreditedFreeTipsCount;
                            if (day.HasCreditedPaidTipsCount) user.CreditedPaidTipsCount += day.CreditedPaidTipsCount;
                            if (day.HasDebitedTipsCount) user.DebitedTipsCount += day.DebitedTipsCount;

                            user.Days.Add(day);
                        }
                    }
                    else
                    {
                        foreach (var operation in groupUser)
                        {
                            switch (operation.OperationType)
                            {
                                case BalanceOperationType.Crediting:
                                    if (operation.FreeTipsCountChangedTo is not null)
                                    {
                                        user.CreditedFreeTipsCount += operation.FreeTipsCountChangedTo.Value;
                                        user.TotalTipsBalance += operation.FreeTipsCountChangedTo.Value;
                                    }

                                    if (operation.PaidTipsCountChangedTo is not null)
                                    {
                                        user.CreditedPaidTipsCount += operation.PaidTipsCountChangedTo.Value;
                                        user.TotalTipsBalance += operation.PaidTipsCountChangedTo.Value;
                                    }

                                    break;

                                case BalanceOperationType.Debiting:
                                    if (operation.TotalTipsCountChangedTo is not null)
                                    {
                                        user.DebitedTipsCount += operation.TotalTipsCountChangedTo.Value;
                                        user.TotalTipsBalance -= operation.TotalTipsCountChangedTo.Value;
                                    }

                                    break;
                                default:
                                    _logger.LogWarning("Неизвестный тип операции {OperationType}",
                                        operation.OperationType);
                                    break;
                            }
                        }
                    }

                    if (user.HasCreditedFreeTipsCount) month.CreditedFreeTipsCount += user.CreditedFreeTipsCount;
                    if (user.HasCreditedPaidTipsCount) month.CreditedPaidTipsCount += user.CreditedPaidTipsCount;
                    if (user.HasDebitedTipsCount) month.DebitedTipsCount += user.DebitedTipsCount;

                    month.Users.Add(user);
                }

                if (month.HasCreditedFreeTipsCount) year.CreditedFreeTipsCount += month.CreditedFreeTipsCount;
                if (month.HasCreditedPaidTipsCount) year.CreditedPaidTipsCount += month.CreditedPaidTipsCount;
                if (month.HasDebitedTipsCount) year.DebitedTipsCount += month.DebitedTipsCount;

                year.Months.Add(month);
            }

            //Добавляем агрегированную историю за год в ответ
            result.Add(year);
        }

        return result;
    }

    public async Task<bool> AddBalance(ApplicationContext applicationContext, long userId, CancellationToken token)
    {
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

    public async Task<BalanceHistory?> CreditTipsToBalance(ApplicationContext applicationContext, long balanceId,
        string reason,
        CancellationToken token, long? creditedFreeTipsCount = null, long? creditedPaidTipsCount = null)
    {
        var balance = await applicationContext.Balances
            .FirstOrDefaultAsync(x => x.Id == balanceId, token);

        if (balance is null)
        {
            _logger.LogWarning("Баланса с идентификатором {Id} не существует", balanceId);
            return null;
        }

        var balanceHistoryCandidate = new BalanceHistory
        {
            OperationType = BalanceOperationType.Crediting,
            ReasonDescription = reason,
            TotalTipsBalance = 0,
            Balance = balance
        };

        //Пополнение бесплатных подсказок
        balance.FreeTipsCount += creditedFreeTipsCount ?? 0;
        balanceHistoryCandidate.FreeTipsCountChangedTo = creditedFreeTipsCount;

        //Пополнение платных подсказок
        balance.PaidTipsCount += creditedPaidTipsCount ?? 0;
        balanceHistoryCandidate.PaidTipsCountChangedTo = creditedPaidTipsCount;

        balanceHistoryCandidate.TotalTipsBalance = balance.TotalTipsCount;

        var balanceHistoryResult = await applicationContext.BalanceHistories
            .AddAsync(balanceHistoryCandidate, token);

        if (await applicationContext.SaveChangesAsync(token) > 0)
            return balanceHistoryResult.Entity;

        return null;
    }

    /// <summary>
    ///     Списание подсказок с баланса
    /// </summary>
    /// <param name="applicationContext">Контекст с БД</param>
    /// <param name="balanceId">Идентификатор баланса, с которого будет списание</param>
    /// <param name="reason">Причина списания - комментарий в дополнение к типу списания</param>
    /// <param name="token">Токен отмены</param>
    /// <param name="debitedTipsCount">Общее количество списанных подсказок</param>
    /// <returns></returns>
    public async Task<BalanceHistory?> DebitTipsFromBalance(ApplicationContext applicationContext, long balanceId,
        string reason,
        CancellationToken token, long debitedTipsCount)
    {
        // Получение искомого баланса по идентификатору
        var balance = await applicationContext.Balances
            .FirstOrDefaultAsync(x => x.Id == balanceId, token);

        // Если баланса в базе данных не существует, то выход из метода
        if (balance is null)
        {
            _logger.LogWarning("Баланса с идентификатором {Id} не существует", balanceId);
            return null;
        }

        // Создание объекта истории операции списания
        var balanceHistoryCandidate = new BalanceHistory
        {
            OperationType = BalanceOperationType.Debiting,
            ReasonDescription = reason,
            TotalTipsBalance = 0,
            Balance = balance
        };

        // Если количество списанных подсказок больше, чем общее количество подсказок на балансе
        if (debitedTipsCount > balance.TotalTipsCount)
        {
            _logger.LogWarning("Попытка списать больше подсказок [{Debit}], чем на балансе [{Balance}]",
                debitedTipsCount, balance.TotalTipsCount);

            balanceHistoryCandidate.FreeTipsCountChangedTo = balance.FreeTipsCount;
            balanceHistoryCandidate.PaidTipsCountChangedTo = balance.PaidTipsCount;
            balance.FreeTipsCount = 0;
            balance.PaidTipsCount = 0;
        }
        else
        {
            // Если количество списанных подсказок больше, чем количество платных подсказок на балансе
            if (debitedTipsCount > balance.PaidTipsCount)
            {
                //Списываем сначала платные подсказки
                debitedTipsCount -= balance.PaidTipsCount;
                balanceHistoryCandidate.PaidTipsCountChangedTo = balance.PaidTipsCount;
                balance.PaidTipsCount = 0;

                //Остаток списываем из бесплатных подсказок
                balanceHistoryCandidate.FreeTipsCountChangedTo = debitedTipsCount;
                balance.FreeTipsCount -= debitedTipsCount;
            }
            else
            {
                //Платных подсказок достаточно на балансе для списания, списываем сначала их
                balanceHistoryCandidate.PaidTipsCountChangedTo = debitedTipsCount;
                balance.PaidTipsCount -= debitedTipsCount;
            }
        }

        balanceHistoryCandidate.TotalTipsBalance = balance.TotalTipsCount;

        var balanceHistoryResult = await applicationContext.BalanceHistories
            .AddAsync(balanceHistoryCandidate, token);

        if (await applicationContext.SaveChangesAsync(token) > 0)
            return balanceHistoryResult.Entity;

        return null;
    }
}