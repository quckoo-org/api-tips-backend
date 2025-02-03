using ApiTips.Api.MapperProfiles.Order;
using ApiTips.Api.Order.V1;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using BalanceOperationType = ApiTips.Dal.Enums.BalanceOperationType;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsOrderService:
    Order.V1.ApiTipsOrderService.ApiTipsOrderServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsOrderService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    ///     Сервис для работы с балансом
    /// </summary>
    private readonly IBalanceService _balanceService;

    public ApiTipsOrderService(IHostEnvironment env, ILogger<ApiTipsOrderService> logger, IServiceProvider services,
        IBalanceService balanceService)
    {
        _logger = logger;
        Services = services;
        _balanceService = balanceService;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(OrderProfile));
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

    public override async Task<GetOrdersResponse> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetOrdersResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var orders = applicationContext
            .Orders
            .Include(x => x.Tariff)
            .Include(x => x.User)
            .AsNoTracking();

        // Фильтрация для получения заказов по статусу
        if (request.Filter != null)
        {
            if (request.Filter.HasOrderStatus)
                orders = orders.Where(order =>
                    order.Status == _mapper.Map<Dal.Enums.OrderStatus>(request.Filter.OrderStatus)
                );
        }

        // Отправка запроса после фильтрации в базу данных
        var result = await orders.ToListAsync(context.CancellationToken);

        if (result.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены заказы по заданным фильтрам";
            _logger.LogWarning("Не найдены заказы по заданным фильтрам");
            return response;
        }

        response.Orders.AddRange(_mapper.Map<List<Order.V1.Order>>(result));
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetOrderResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var order = await applicationContext
            .Orders
            .Include(x => x.Tariff)
            .Include(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == request.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден заказ по заданному параметру";
            _logger.LogWarning("Не найден заказ по заданному параметру");

            return response;
        }

        // Маппинг заказа из БД в ответ
        response.Order = _mapper.Map<Order.V1.Order>(order);
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<AddOrderResponse> AddOrder(AddOrderRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddOrderResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Поиск пользователя в базе
        var user = await applicationContext.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId);

        if (user is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Пользователя с таким идентификатором не существует";
            _logger.LogWarning("Пользователя с идентификатором {id} не существует", request.UserId);

            return response;
        }

        if (user.LockDateTime != null)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не возможно создать заказ для заблакированного пользователя";
            _logger.LogWarning("Не возможно создать заказ для заблакированного пользователя {id}", request.UserId);

            return response;
        }

        // Поиск тарифа в базе
        var tariff = await applicationContext.Tariffs
            .FirstOrDefaultAsync(x => x.Id == request.TariffId);

        if (tariff is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Тарифа с таким идентификатором не существует";
            _logger.LogWarning("Тарифа с идентификатором {id} не существует", request.TariffId);

            return response;
        }

        var orderCandidate = new Dal.schemas.data.Order
        {
            Status = Dal.Enums.OrderStatus.Created,
            User = user,
            Tariff = tariff
        };

        var orderResult = await applicationContext.Orders.AddAsync(orderCandidate, context.CancellationToken);

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Order = _mapper.Map<Order.V1.Order>(orderResult.Entity);
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления заказа в БД";
            _logger.LogError("Ошибка добавления заказа в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка добавления заказа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления заказа в БД";
        }

        return response;
    }

    public override async Task<SetOrderStatusPaidResponse> SetOrderStatusPaid(SetOrderStatusPaidRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new SetOrderStatusPaidResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Поиск заказа в базе
        var order = await applicationContext.Orders
            .Include(x => x.Tariff)
            .Include(x => x.User)
            .ThenInclude(x => x.Balance)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);

        if (order is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Заказа с таким идентификатором не существует";
            _logger.LogWarning("Заказа с идентификатором {id} не существует", request.OrderId);

            return response;
        }

        if (order.Status == Dal.Enums.OrderStatus.Cancelled)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не возможно установить статус 'Оплачен', так как заказ отменён";
            _logger.LogWarning("Не возможно установить статус 'Оплачен', так как заказ {id} отменён", request.OrderId);

            return response;
        }

        await using var transaction = await applicationContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            if (order.Status != Dal.Enums.OrderStatus.Paid)
            {
                order.Status = Dal.Enums.OrderStatus.Paid;

                if (order.User.Balance is null)
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description = "Unable to credit tips when paying for an order, the user has no balance";
                    _logger.LogError("Невозможно начислить подсказки при оплате заказа, у пользователя нет баланса");

                    return response;
                }

                //Пополнение баланса согласно тарифу заказа
                var updateBalanceResult = await _balanceService.UpdateBalance(
                    applicationContext,
                    order.User.Balance.Id,
                    BalanceOperationType.Crediting,
                    BalanceOperationType.Crediting.ToString(),
                    context.CancellationToken,
                    order.Tariff.FreeTipsCount,
                    order.Tariff.PaidTipsCount
                );

                if (updateBalanceResult is null)
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description = "Error crediting tips when paying for an order";
                    _logger.LogError("Ошибка начислиения подсказок при оплате заказа {Id}", order.Id);

                    return response;
                }
            }

            order.PaymentDateTime = request.PaymentDate.ToDateTime();

            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Order = _mapper.Map<Order.V1.Order>(order);

                await transaction.CommitAsync(context.CancellationToken);
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не было внесено никаких изменений в заказ";
            _logger.LogError("Не было внесено никаких изменений в заказ");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка обновления заказа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления заказа в БД";
        }

        await transaction.RollbackAsync(context.CancellationToken);
        return response;
    }

    public override async Task<SetOrderStatusCancelledResponse> SetOrderStatusCancelled(SetOrderStatusCancelledRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new SetOrderStatusCancelledResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Поиск заказ в базе
        var order = await applicationContext.Orders
            .Include(x => x.Tariff)
            .Include(x => x.User)
            .ThenInclude(x => x.Balance)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);

        if (order is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Заказа с таким идентификатором не существует";
            _logger.LogWarning("Заказа с идентификатором {id} не существует", request.OrderId);

            return response;
        }

        await using var transaction = await applicationContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            if (order.Status == Dal.Enums.OrderStatus.Paid)
            {
                if (order.User.Balance is null)
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description = "Unable to debiting tips when canceling a paid order, the user has no balance";
                    _logger.LogError("Невозможно списать подсказки при отмене оплаченного заказа, у пользователя нет баланса");

                    return response;
                }

                //Пополнение баланса согласно тарифу заказа
                var updateBalanceResult = await _balanceService.UpdateBalance(
                    applicationContext,
                    order.User.Balance.Id,
                    BalanceOperationType.Debiting,
                    BalanceOperationType.Debiting.ToString(),
                    context.CancellationToken,
                    order.Tariff.FreeTipsCount,
                    order.Tariff.PaidTipsCount
                );

                if (updateBalanceResult is null)
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description = "Error debiting tips when canceling a paid order";
                    _logger.LogError("Ошибка списания подсказок при отмене оплаченного заказа {Id}", order.Id);

                    return response;
                }
            }

            order.Status = Dal.Enums.OrderStatus.Cancelled;

            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Order = _mapper.Map<Order.V1.Order>(order);

                await transaction.CommitAsync(context.CancellationToken);
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не было внесено никаких изменений в заказ";
            _logger.LogError("Не было внесено никаких изменений в заказ");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка обновления заказа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления заказа в БД";
        }

        await transaction.RollbackAsync();
        return response;
    }
}
