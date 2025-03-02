using ApiTips.Api.Balance.V1;
using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.MapperProfiles.Balance;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.Dal.schemas.data;
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
    ///     Получение истории операций сгруппированной для всех пользователей
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

        try
        {
            var historiesByYear = await _balanceService.GetHistories(startDate, endDate, context.CancellationToken);

            if (historiesByYear is null)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                return response;
            }

            response.Years.AddRange(historiesByYear);
            response.Response.Status = OperationStatus.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка получения аггрегированной истории баланса: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error receiving balance history";
        }

        return response;
    }

    /// <summary>
    ///     Получение детальной истории операций для пользователей за месяц
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
        };

        var date = request.Date.ToDateTime();    

        var startDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        try
        {
            var historiesByYear = await _balanceService.GetHistories(startDate, endDate, context.CancellationToken,
                null, request.UserIds.ToList());

            var month = historiesByYear?.FirstOrDefault()?.Months.FirstOrDefault();
            if (month is null)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                return response;
            }

            response.Month = month;
            response.Response.Status = OperationStatus.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка получения детальной истории баланса: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error receiving detailed balance history";
        }

        return response;
    }

    /// <summary>
    ///     Получение текущего баланса пользователя
    /// </summary>
    public override async Task<GetUserBalanceResponse> GetUserBalance(GetUserBalanceRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetUserBalanceResponse
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

        var balance = await applicationContext.Balances
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.User.Id == request.UserId, context.CancellationToken);

        if (balance is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Balance does not exist in DB for user";
            return response;
        }

        response.Response.Status = OperationStatus.Ok;
        response.Balance = balance.TotalTipsCount;
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

        try
        {
            BalanceHistory? balanceHistory;

            switch (request.OperationType)
            {
                case CustomEnums.V1.BalanceOperationType.Crediting:
                    if (request is { HasCreditedFreeTipsCount: false, HasCreditedPaidTipsCount: false })
                    {
                        response.Response.Status = OperationStatus.Error;
                        response.Response.Description = "The operation does not contain any balance changes";
                        _logger.LogWarning("Операция не содержит изменений баланса");

                        return response;
                    }

                    //Пополняем подсказки на балансе
                    balanceHistory = await _balanceService.CreditTipsToBalance(applicationContext, user.Balance.Id, request.Reason,
                        context.CancellationToken,
                        request.HasCreditedFreeTipsCount ? request.CreditedFreeTipsCount : null,
                        request.HasCreditedPaidTipsCount ? request.CreditedPaidTipsCount : null);
                    break;
                case CustomEnums.V1.BalanceOperationType.Debiting:
                    if (!request.HasDebitedTipsCount)
                    {
                        response.Response.Status = OperationStatus.Error;
                        response.Response.Description = "The operation does not contain any balance changes";
                        _logger.LogWarning("Операция не содержит изменений баланса");

                        return response;
                    }

                    //Списываем подсказки с баланса
                    balanceHistory = await _balanceService.DebitTipsFromBalance(applicationContext, user.Balance.Id, request.Reason,
                        context.CancellationToken, request.DebitedTipsCount);
                    break;
                default:
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description = "An unknown operation type was specified";
                    _logger.LogWarning("Указан неизвестный тип операции");

                    return response;
            }

            if (balanceHistory is null)
            {
                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Balance update error";
                _logger.LogError("Не смогли изменить баланс");

                return response;
            }

            response.Response.Status = OperationStatus.Ok;
            response.Operation = _mapper.Map<Operation>(balanceHistory);
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

        // Получение балансов пользователей, у которых баланс положительный
        // прим. обнулённые балансы и балансы отрицательные нельзя изменять
        var balances = await applicationContext.Balances
            .Where(x => x.FreeTipsCount > 0 || x.PaidTipsCount > 0)
            .ToListAsync(context.CancellationToken);

        // Если нет балансов, которые можно обнулить, то возвращется NoData
        if (balances.Count == 0)
        {
            _logger.LogWarning("Балансы всех пользователей уже списаны");

            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "All balances are already zeroed out";
            
            return response;
        }

        try
        {
            // Проход по всем балансам 
            foreach (var balance in balances)
            {

                //Создаем запись с историей изменения баланса
                var balanceHistoryCandidate = new BalanceHistory
                {
                    FreeTipsCountChangedTo = balance.FreeTipsCount,
                    PaidTipsCountChangedTo = balance.PaidTipsCount,
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

    /// <summary>
    ///     Получение истории операций сгруппированной по годам для одного пользователя
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
        };

        if (request.StartDate >= request.EndDate)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description =
                "The end date of the history collection period must be greater than the start date of the period";
            _logger.LogWarning("Дата окончания периода сбора истории должна быть больше даты начала периода");
            return response;
        }

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var user = await applicationContext.Users
            .FirstOrDefaultAsync(x => x.Email == context.GetUserEmail(), context.CancellationToken);

        if (user is null)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "User does not exist in DB";
            _logger.LogWarning("Не нашли пользователя с почтой {Email} в базе", context.GetUserEmail());
            return response;
        }

        var startDate = request.StartDate.ToDateTime();
        var endDate = request.EndDate.ToDateTime();

        try
        {
            var historiesByYear = await _balanceService.GetHistories(startDate, endDate, context.CancellationToken, [user.Id]);

            if (historiesByYear is null)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                return response;
            }

            response.Years.AddRange(historiesByYear);
            response.Response.Status = OperationStatus.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка получения аггрегированной истории баланса: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error receiving balance history";
        }

        return response;
    }

    /// <summary>
    /// Получение детальной истории операций за месяц для одного пользователя
    /// </summary>
    public override async Task<GetDetailedHistoriesByUserResponse> GetDetailedHistoriesByUser(
        GetDetailedHistoriesByUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetDetailedHistoriesByUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var user = await applicationContext.Users
            .FirstOrDefaultAsync(x => x.Email == context.GetUserEmail(), context.CancellationToken);

        if (user is null)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "User does not exist in DB";
            _logger.LogWarning("Не нашли пользователя с почтой {Email} в базе", context.GetUserEmail());
            return response;
        }

        var date = request.Date.ToDateTime();

        var startDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        try
        {
            var historiesByYear = await _balanceService.GetHistories(startDate, endDate, context.CancellationToken,
                [user.Id], [user.Id]);

            var month = historiesByYear?.FirstOrDefault()?.Months.FirstOrDefault();
            if (month is null)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = "There is no balance history for the entered time period";
                return response;
            }

            response.Month = month;
            response.Response.Status = OperationStatus.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка получения детальной истории баланса: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error receiving detailed balance history";
        }

        return response;
    }
}