using ApiTips.Api.MapperProfiles.Balance;
using ApiTips.Api.Balance.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.WellKnownTypes;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsBalanceService:
    Balance.V1.ApiTipsBalanceService.ApiTipsBalanceServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsBalanceService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    public ApiTipsBalanceService(IHostEnvironment env, ILogger<ApiTipsBalanceService> logger, IServiceProvider services)
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
    /// Получение истории операций сгруппированных по показателю для всех пользователей
    /// </summary>
    public override async Task<GetAggregatedBalanceHistoriesResponse> GetAggregatedBalanceHistories(
        GetAggregatedBalanceHistoriesRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetAggregatedBalanceHistoriesResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        if (request.StartDate >= request.EndDate)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description =
                "The end date of the history collection period must be greater than the start date of the period";
            _logger.LogWarning("Дата окончания периода сбора истории должна быть больше даты начала периода");
            return response;
        }

        var startDate = request.StartDate.ToDateTime();
        var endDate = request.EndDate.ToDateTime();

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        if (request.HasAggregateByMonth && request.AggregateByMonth)
        {
            var histories = await applicationContext
                .BalanceHistories
                .AsNoTracking()
                .Where(x => x.OperationDateTime >= startDate && x.OperationDateTime <= endDate)
                .ToListAsync();

            if (histories.Count == 0)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                _logger.LogWarning("В базе нет истории баланса за указанный период {start} - {end}",
                    startDate.ToShortDateString(), endDate.ToShortDateString());
                return response;
            }

            var historiesByMonth = histories.GroupBy(x => new { x.OperationDateTime.Year, x.OperationDateTime.Month });

            foreach (var historyGroup in historiesByMonth)
            {
                var sortedHistory = historyGroup.OrderBy(x => x.OperationDateTime);

                //Создание объекта агрегированной истории баланса по месяцам
                var aggregatedBalanceHistoryByMonth = new AggregatedBalanceHistory();
                aggregatedBalanceHistoryByMonth.StartDate = sortedHistory.First().OperationDateTime.ToTimestamp();
                aggregatedBalanceHistoryByMonth.EndDate = sortedHistory.Last().OperationDateTime.ToTimestamp();

                var aggregatedData = new AggregatedBalanceHistoryData();

                foreach (var history in sortedHistory.ToArray())
                {
                    switch (history.OperationType)
                    {
                        case Dal.Enums.BalanceOperationType.Crediting:
                            if (history.FreeTipsCountChangedTo is not null)
                                aggregatedData.CreditedFreeTipsCount += history.FreeTipsCountChangedTo.Value;
                            if (history.PaidTipsCountChangedTo is not null)
                                aggregatedData.CreditedPaidTipsCount += history.PaidTipsCountChangedTo.Value;
                            break;

                        case Dal.Enums.BalanceOperationType.Debiting:
                            if (history.TotalTipsCountChangedTo is not null)
                                aggregatedData.DebitedTipsCount += history.TotalTipsCountChangedTo.Value;
                            break;
                    }
                }

                aggregatedBalanceHistoryByMonth.AggregatedBalanceHistoryData.Add(aggregatedData);

                //Добавляем аггрегированную историю за месяц в ответ
                response.AggregatedBalanceHistories.Add(aggregatedBalanceHistoryByMonth);
            }
        }

        if (request.HasAggregateByUser && request.AggregateByUser)
        {
            var histories = await applicationContext
                .BalanceHistories
                .Include(x => x.Balance)
                .ThenInclude(x => x.User)
                .AsNoTracking()
                .Where(x => x.OperationDateTime >= startDate && x.OperationDateTime <= endDate)
                .ToListAsync();

            if (histories.Count == 0)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                _logger.LogWarning("В базе нет истории баланса за указанный период {start} - {end}",
                    startDate.ToShortDateString(), endDate.ToShortDateString());
                return response;
            }

            var historiesByUser = histories.GroupBy(x => new { x.Balance.UserId });

            //Создание объекта агрегированной истории баланса по пользователям
            var aggregatedBalanceHistoryByUsers = new AggregatedBalanceHistory();
            aggregatedBalanceHistoryByUsers.StartDate = request.StartDate;
            aggregatedBalanceHistoryByUsers.EndDate = request.EndDate;

            foreach (var historyGroup in historiesByUser)
            {
                var aggregatedData = new AggregatedBalanceHistoryData();

                aggregatedData.User = _mapper.Map<User>(historyGroup.First().Balance.User);

                foreach (var history in historyGroup.ToArray())
                {
                    switch (history.OperationType)
                    {
                        case Dal.Enums.BalanceOperationType.Crediting:
                            if (history.FreeTipsCountChangedTo is not null)
                                aggregatedData.CreditedFreeTipsCount += history.FreeTipsCountChangedTo.Value;
                            if (history.PaidTipsCountChangedTo is not null)
                                aggregatedData.CreditedPaidTipsCount += history.PaidTipsCountChangedTo.Value;
                            break;

                        case Dal.Enums.BalanceOperationType.Debiting:
                            if (history.TotalTipsCountChangedTo is not null)
                                aggregatedData.DebitedTipsCount += history.TotalTipsCountChangedTo.Value;
                            break;
                    }
                }

                aggregatedBalanceHistoryByUsers.AggregatedBalanceHistoryData.Add(aggregatedData);
            }

            //Добавляем аггрегированную историю по всем пользователям за указанный период в ответ
            response.AggregatedBalanceHistories.Add(aggregatedBalanceHistoryByUsers);
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    /// <summary>
    /// Получение истрории операций сгруппированных по месяцам для одного пользователя
    /// </summary>
    public override async Task<GetAggregatedBalanceHistoriesByUserResponse> GetAggregatedBalanceHistoriesByUser(
        GetAggregatedBalanceHistoriesByUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetAggregatedBalanceHistoriesByUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        if (request.StartDate >= request.EndDate)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description =
                "The end date of the history collection period must be greater than the start date of the period";
            _logger.LogWarning("Дата окончания периода сбора истории должна быть больше даты начала периода");
            return response;
        }

        var startDate = request.StartDate.ToDateTime();
        var endDate = request.EndDate.ToDateTime();

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var histories = await applicationContext
                .BalanceHistories
                .Include(x => x.Balance)
                .AsNoTracking()
                .Where(x => x.OperationDateTime >= startDate
                    && x.OperationDateTime <= endDate
                    && x.Balance.UserId == request.UserId)
                .ToListAsync();

        if (histories.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "There is no balance history for the entered time period and user";
            _logger.LogWarning("В базе нет истории баланса за указанный период {start} - {end}, для пользователя c id {id}",
                startDate.ToShortDateString(), endDate.ToShortDateString(), request.UserId);
            return response;
        }

        var historiesByUser = histories.GroupBy(x => new { x.OperationDateTime.Year, x.OperationDateTime.Month });

        foreach (var historyGroup in historiesByUser)
        {
            var sortedHistory = historyGroup.OrderBy(x => x.OperationDateTime);

            //Создание объекта агрегированной истории баланса по месяцам для указанного пользователя
            var aggregatedBalanceHistoryByMonth = new AggregatedBalanceHistory();
            aggregatedBalanceHistoryByMonth.StartDate = sortedHistory.First().OperationDateTime.ToTimestamp();
            aggregatedBalanceHistoryByMonth.EndDate = sortedHistory.Last().OperationDateTime.ToTimestamp();

            var aggregatedData = new AggregatedBalanceHistoryData();

            foreach (var history in sortedHistory.ToArray())
            {
                switch (history.OperationType)
                {
                    case Dal.Enums.BalanceOperationType.Crediting:
                        if (history.FreeTipsCountChangedTo is not null)
                            aggregatedData.CreditedFreeTipsCount += history.FreeTipsCountChangedTo.Value;
                        if (history.PaidTipsCountChangedTo is not null)
                            aggregatedData.CreditedPaidTipsCount += history.PaidTipsCountChangedTo.Value;
                        break;

                    case Dal.Enums.BalanceOperationType.Debiting:
                        if (history.TotalTipsCountChangedTo is not null)
                            aggregatedData.DebitedTipsCount += history.TotalTipsCountChangedTo.Value;
                        break;
                }
            }

            aggregatedBalanceHistoryByMonth.AggregatedBalanceHistoryData.Add(aggregatedData);

            //Добавляем аггрегированную историю за месяц в ответ
            response.AggregatedBalanceHistories.Add(aggregatedBalanceHistoryByMonth);
        }

        response.UserId = request.UserId;
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    /// <summary>
    /// Получение детальной истории операций по пользователю
    /// </summary>
    public override async Task<GetBalanceHistoriesResponse> GetBalanceHistories(
        GetBalanceHistoriesRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetBalanceHistoriesResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        if (request.StartDate >= request.EndDate)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description =
                "The end date of the history collection period must be greater than the start date of the period";
            _logger.LogWarning("Дата окончания периода сбора истории должна быть больше даты начала периода");
            return response;
        }

        var startDate = request.StartDate.ToDateTime();
        var endDate = request.EndDate.ToDateTime();

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var histories = await applicationContext
                .BalanceHistories
                .Include(x => x.Balance)
                .AsNoTracking()
                .Where(x => x.OperationDateTime >= startDate
                    && x.OperationDateTime <= endDate
                    && x.Balance.UserId == request.UserId)
                .ToListAsync();

        if (histories.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "There is no balance history for the entered time period and user";
            _logger.LogWarning("В базе нет истории баланса за указанный период {start} - {end}, для пользователя c id {id}",
                startDate.ToShortDateString(), endDate.ToShortDateString(), request.UserId);
            return response;
        }

        response.TotalMonthHistories.AddRange(_mapper.Map<List<BalanceHistory>>(histories));
        response.UserId = request.UserId;
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    /// <summary>
    /// Добавить запись в историю баланса отдельного пользователя - способ изменить баланс для админа
    /// </summary>
    public override async Task<AddBalanceHistoryResponse> AddBalanceHistory(
        AddBalanceHistoryRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddBalanceHistoryResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        var user = await applicationContext.Users
            .Include(x => x.Balance)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, context.CancellationToken);

        if (user is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "The user with the specified ID does not exist";
            _logger.LogWarning("Пользователя с идентификатором {id} не существует", request.UserId);

            return response;
        }

        if (user.Balance is null)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "The user with the specified ID has no connection to the balance";
            _logger.LogError("У пользователя с идентификатором {id} нет связи с балансом", request.UserId);

            return response;
        }

        if (request.FreeTipsCount == 0 && request.PaidTipsCount == 0)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "The operation does not contain any balance changes";
            _logger.LogWarning("Операция не содержит изменений баланса");

            return response;
        }

        using (var transaction = applicationContext.Database.BeginTransaction())
        {
            try
            {
                switch (request.OperationType)
                {
                    case BalanceOperationType.Crediting:
                        user.Balance.FreeTipsCount += request.HasFreeTipsCount ? request.FreeTipsCount : 0;
                        user.Balance.PaidTipsCount += request.HasPaidTipsCount ? request.PaidTipsCount : 0;
                        break;
                    case BalanceOperationType.Debiting:
                        //На данный момент договорённость, что не опускаем значение баланса ниже 0
                        if (request.HasFreeTipsCount)
                            user.Balance.FreeTipsCount -= request.FreeTipsCount > user.Balance.FreeTipsCount ?
                                user.Balance.FreeTipsCount : request.FreeTipsCount;
                        if (request.HasPaidTipsCount)
                            user.Balance.PaidTipsCount -= request.PaidTipsCount > user.Balance.PaidTipsCount ?
                                user.Balance.PaidTipsCount : request.PaidTipsCount;
                        break;
                    default:
                        response.Response.Status = OperationStatus.Error;
                        response.Response.Description = "An unknown operation type was specified";
                        _logger.LogError("Узакан неизвестный тип операции");
                        break;
                }

                if (await applicationContext.SaveChangesAsync(context.CancellationToken) == 0)
                {
                    throw new Exception("Не смогли изменить значение баланса в БД");
                }

                var balanceHistoryCandidate = new Dal.schemas.data.BalanceHistory
                {
                    FreeTipsCountChangedTo = request.HasFreeTipsCount ? request.FreeTipsCount : null,
                    PaidTipsCountChangedTo = request.HasPaidTipsCount ? request.PaidTipsCount : null,
                    OperationType = _mapper.Map<Dal.Enums.BalanceOperationType>(request.OperationType),
                    ReasonDescription = request.Reason,
                    TotalTipsBalance = user.Balance.TotalTipsCount,
                    Balance = user.Balance
                };

                var balanceHistoryResult = await applicationContext.BalanceHistories.AddAsync(balanceHistoryCandidate);

                if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
                {
                    response.Response.Status = OperationStatus.Ok;
                    response.BalanceHistory = _mapper.Map<BalanceHistory>(balanceHistoryResult.Entity);
                    return response;
                }

                throw new Exception("Не смогли записать историю изменения баланса в БД");
            }
            catch (Exception e)
            {
                transaction.Rollback();

                _logger.LogError("Ошибка добавления записи изменения баланса в БД: {Message} | InnerException: {InnerMessage}",
                    e.Message, e.InnerException?.Message);

                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Error adding balance change record to DB";
            }
        }

        return response;
    }

    /// <summary>
    /// Списание всех подсказок со всех пользователей
    /// </summary>
    public override async Task<DebitAllTipsResponse> DebitAllTips(DebitAllTipsRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new DebitAllTipsResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var balances = await applicationContext.Balances
            .ToListAsync(context.CancellationToken);

        using (var transaction = applicationContext.Database.BeginTransaction())
        {
            try
            {
                foreach (var balance in balances)
                {
                    //Пропускаем пустой баланс
                    if (balance.TotalTipsCount == 0)
                        continue;

                    //Создаем запись с историей изменения баланса
                    var balanceHistoryCandidate = new Dal.schemas.data.BalanceHistory
                    {
                        FreeTipsCountChangedTo = balance.FreeTipsCount > 0 ? balance.FreeTipsCount : null,
                        PaidTipsCountChangedTo = balance.PaidTipsCount > 0 ? balance.PaidTipsCount : null,
                        OperationType = Dal.Enums.BalanceOperationType.Debiting,
                        ReasonDescription = Dal.Enums.BalanceOperationType.Debiting.ToString(),
                        TotalTipsBalance = 0,
                        Balance = balance
                    };

                    //Списываем все подсказки с баланса
                    balance.FreeTipsCount = 0;
                    balance.PaidTipsCount = 0;

                    if (await applicationContext.SaveChangesAsync(context.CancellationToken) == 0)
                    {
                        throw new Exception("Не смогли изменить значение баланса в БД");
                    }

                    var balanceHistoryResult = await applicationContext.BalanceHistories.AddAsync(balanceHistoryCandidate);

                    if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
                    {
                        response.Response.Status = OperationStatus.Ok;
                        return response;
                    }

                    throw new Exception("Не смогли записать историю изменения баланса в БД");
                }
            }
            catch (Exception e)
            {
                transaction.Rollback();

                _logger.LogError("Ошибка обнуления всех балансов: {Message} | InnerException: {InnerMessage}",
                    e.Message, e.InnerException?.Message);

                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Error resetting all balances";
            }
        }
        return response;
    }
}
