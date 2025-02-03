using ApiTips.Api.Balance.V1;
using ApiTips.Api.MapperProfiles.Balance;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using BalanceOperationType = ApiTips.Dal.Enums.BalanceOperationType;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsBalanceService :
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

    /// <summary>
    ///     Сервис для работы с балансом
    /// </summary>
    private readonly IBalanceService _balanceService;

    public ApiTipsBalanceService(IHostEnvironment env, ILogger<ApiTipsBalanceService> logger, IServiceProvider services,
        IBalanceService balanceService)
    {
        _logger = logger;
        Services = services;
        _balanceService = balanceService;

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
    ///     Получение истории операций сгруппированных по показателю для всех пользователей
    /// </summary>
    public override async Task<GetHistoriesResponse> GetHistories(
        GetHistoriesRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetHistoriesResponse
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

        if (request is { HasAggregateByMonth: true, AggregateByMonth: true })
        {
            var historiesByMonth = await _balanceService.GetHistoriesByMonth(startDate, endDate, context.CancellationToken);

            if (historiesByMonth is null)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                return response;
            }

            //Добавляем агрегированную историю за месяц в ответ
            response.Histories.AddRange(historiesByMonth);
        }

        if (request is { HasAggregateByUser: true, AggregateByUser: true })
        {
            var historiesByUsers = await _balanceService.GetHistoriesByUsers(startDate, endDate, context.CancellationToken);

            if (historiesByUsers is null)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                _logger.LogWarning("В базе нет истории баланса за указанный период {Start} - {End}",
                    startDate.ToShortDateString(), endDate.ToShortDateString());
                return response;
            }

            //Добавляем агрегированную историю за месяц в ответ
            response.Histories.AddRange(historiesByUsers);
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    /// <summary>
    ///     Получение истории операций сгруппированных по месяцам для одного пользователя
    /// </summary>
    public override async Task<GetHistoriesByUserResponse> GetHistoriesByUser(
        GetHistoriesByUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetHistoriesByUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
            UserId = request.UserId
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

        var historiesByMonth = await _balanceService
            .GetHistoriesByMonth(startDate, endDate, context.CancellationToken, request.UserId);

        if (historiesByMonth is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "There is no balance history for the entered time period and user";
            return response;
        }

        //Добавляем агрегированную историю за месяц в ответ
        response.Histories.AddRange(historiesByMonth);
        response.Response.Status = OperationStatus.Ok;

        return response;
    }

    /// <summary>
    ///     Получение детальной истории операций по пользователю
    /// </summary>
    public override async Task<GetDetailedHistoriesResponse> GetDetailedHistories(
        GetDetailedHistoriesRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetDetailedHistoriesResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
            UserId = request.UserId
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
                        && x.Balance.User.Id == request.UserId)
            .ToListAsync();

        if (histories.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "There is no balance history for the entered time period and user";
            _logger.LogWarning(
                "В базе нет истории баланса за указанный период {Start} - {End}, для пользователя c id {Id}",
                startDate.ToShortDateString(), endDate.ToShortDateString(), request.UserId);
            return response;
        }

        try
        {
            response.DetailedHistories.AddRange(_mapper.Map<List<DetailedHistory>>(histories));
            response.Response.Status = OperationStatus.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Ошибка маппинга записи изменения баланса: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error getting detailed history";
        }

        return response;
    }

    /// <summary>
    ///     Изменение баланса отдельного пользователя
    /// </summary>
    public override async Task<UpdateBalanceResponse> UpdateBalance(
        UpdateBalanceRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdateBalanceResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
            UserId = request.UserId
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
            _logger.LogWarning("Пользователя с идентификатором {Id} не существует", request.UserId);

            return response;
        }

        if (user.Balance is null)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "The user with the specified ID has no connection to the balance";
            _logger.LogError("У пользователя с идентификатором {Id} нет связи с балансом", request.UserId);

            return response;
        }

        if (request.HasFreeTipsCount is false && request.HasPaidTipsCount is false)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "The operation does not contain any balance changes";
            _logger.LogWarning("Операция не содержит изменений баланса");

            return response;
        }

        try
        {
            var balanceHistory = await _balanceService.UpdateBalance(
                applicationContext,
                user.Balance.Id,
                _mapper.Map<BalanceOperationType>(request.OperationType),
                request.Reason,
                context.CancellationToken,
                request.HasFreeTipsCount ? request.FreeTipsCount : null,
                request.HasPaidTipsCount ? request.PaidTipsCount : null);

            if (balanceHistory is null)
            {
                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Balance update error";
                _logger.LogError("Не смогли изменить баланс");

                return response;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Ошибка изменения баланса в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Balance update error";
        }

        return response;
    }

    /// <summary>
    ///     Списание всех подсказок со всех пользователей
    /// </summary>
    public override async Task<DebitAllTipsResponse> DebitAllTips(DebitAllTipsRequest request,
        ServerCallContext context)
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
                    OperationType = BalanceOperationType.Debiting,
                    ReasonDescription = BalanceOperationType.Debiting.ToString(),
                    TotalTipsBalance = 0,
                    Balance = balance
                };

                //Списываем все подсказки с баланса
                balance.FreeTipsCount = 0;
                balance.PaidTipsCount = 0;

                await applicationContext.BalanceHistories.AddAsync(balanceHistoryCandidate, context.CancellationToken);
            }

            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                return response;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка обнуления всех балансов: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error resetting all balances";
        }

        return response;
    }
}