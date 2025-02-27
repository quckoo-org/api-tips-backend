using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.MapperProfiles.Order;
using ApiTips.Api.Order.V1;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.Dal.Enums;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using BalanceOperationType = ApiTips.Dal.Enums.BalanceOperationType;
using OrderStatus = ApiTips.Dal.Enums.OrderStatus;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsOrderService :
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
    
    /// <summary>
    ///     Сервис для работы со счетами
    /// </summary>
    private readonly IInvoiceService _invoiceService;

    public ApiTipsOrderService(IHostEnvironment env, ILogger<ApiTipsOrderService> logger, IServiceProvider services,
        IBalanceService balanceService, IInvoiceService invoiceService)
    {
        _logger = logger;
        Services = services;
        _balanceService = balanceService;
        _invoiceService = invoiceService;

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
            .Include(x => x.Invoice)
            .AsNoTracking();

        // Фильтрация списка заказов
        if (request.Filter != null)
        {
            if (request.Filter.HasOrderStatus)
                orders = orders.Where(order =>
                    order.Status == _mapper.Map<OrderStatus>(request.Filter.OrderStatus)
                );

            if (request.Filter.HasUserEmail)
                orders = orders.Where(order =>
                    order.User.Email == request.Filter.UserEmail
                );
        }

        // Отправка запроса после фильтрации в базу данных
        var result = await orders.ToListAsync(context.CancellationToken);

        if (result.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "No orders found matching the specified filters";
            _logger.LogWarning("Не найдены заказы по заданным фильтрам");
            return response;
        }

        try
        {
            response.Orders.AddRange(_mapper.Map<List<Order.V1.Order>>(result));
            response.Response.Status = OperationStatus.Ok;
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка получения заказов из БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error receiving orders from the DB";
        }

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
            response.Response.Description = "Order does not exist";
            _logger.LogWarning("Заказ с идентификатором {Id} не существует", request.OrderId);

            return response;
        }

        try
        {
            // Маппинг заказа из БД в ответ
            response.Order = _mapper.Map<Order.V1.Order>(order);
            response.Response.Status = OperationStatus.Ok;
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка получения заказа {Id} из БД: {Message} | InnerException: {InnerMessage}",
                request.OrderId, e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error receiving order from the DB";
        }

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

        Dal.schemas.system.User? user;

        // Поиск пользователя в базе:
        // если запрос делает администратор, то учитывается запрос по идентификатору пользователя
        // если запрос сделан обычным клиентом, то берётся его почта из jwt-токена
        if (request.HasUserId
            && context.GetUserRoles().Contains("admin", StringComparer.OrdinalIgnoreCase))
        {
            user = await applicationContext.Users
                .FirstOrDefaultAsync(x => x.Id == request.UserId);
        }
        else
        {
            var userEmail = context.GetUserEmail();
            user = await applicationContext.Users
                .FirstOrDefaultAsync(x => x.Email == userEmail);
        }

        if (user is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "User does not exist";
            _logger.LogWarning("Пользователя с идентификатором {id} не существует", request.UserId);

            return response;
        }

        if (user.LockDateTime != null)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Unable to create an order for a blocked user";
            _logger.LogWarning("Невозможно создать заказ для заблокированного пользователя {id}", request.UserId);

            return response;
        }

        // Поиск тарифа в базе
        var tariff = await applicationContext.Tariffs
            .FirstOrDefaultAsync(x => x.Id == request.TariffId);

        if (tariff is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Tariff does not exist";
            _logger.LogWarning("Тарифа с идентификатором {id} не существует", request.TariffId);

            return response;
        }

        var orderCandidate = new Dal.schemas.data.Order
        {
            Status = OrderStatus.Created,
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
            response.Response.Description = "Error adding order to DB";
            _logger.LogError("Ошибка добавления заказа в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка добавления заказа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error adding order to DB";
        }

        return response;
    }

    public override async Task<SetOrderStatusPaidResponse> SetOrderStatusPaid(SetOrderStatusPaidRequest request,
        ServerCallContext context)
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
            .Include(x => x.Invoice)
            .Include(x => x.Tariff)
            .Include(x => x.User)
            .ThenInclude(x => x.Balance)
            .Include(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);

        if (order is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Order does not exist";
            _logger.LogWarning("Заказа с идентификатором {id} не существует", request.OrderId);

            return response;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Unable to set status to 'Paid' because order has been cancelled";
            _logger.LogWarning("Невозможно установить статус 'Оплачен', так как заказ {id} отменён", request.OrderId);

            return response;
        }

        await using var transaction =
            await applicationContext.Database.BeginTransactionAsync(context.CancellationToken);

        if (order.Invoice is null)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Unable to set status to 'Paid' because invoice does not exist";
            _logger.LogWarning("Невозможно установить статус 'Оплачен', к заказу {id}, счет не создан", request.OrderId);

            return response;
        }

        try
        {
            if (order.Status != OrderStatus.Paid)
            {
                order.Status = OrderStatus.Paid;

                if (order.User.Balance is null)
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description =
                        "Unable to credit tips when paying for an order, the user has no balance";
                    _logger.LogError("Невозможно начислить подсказки при оплате заказа, у пользователя с id {UserId} нет баланса", 
                        order.User.Id);

                    return response;
                }

                //Пополнение баланса согласно тарифу заказа
                var updateBalanceResult = await _balanceService.CreditTipsToBalance(
                    applicationContext,
                    order.User.Balance.Id,
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

            // В любом случае меняется дата оплаты, даже если статус уже установлен на оплачен
            // TODO tadius: точно ли это валидно?
            order.PaymentDateTime = request.PaymentDate.ToDateTime();
            
            // Обновление счета, установка такого же статуса, как у заказа
            if (order.Invoice is not null && _invoiceService.UpdateInvoiceStatus(order.Invoice, applicationContext, InvoiceStatusEnum.Paid))
            {
                _logger.LogInformation(
                    "Счет {InvoiceGuid}, который привязан к заказу переведён в статус Paid", 
                    order.Invoice.Id);
                order.Invoice.PayedAt = request.PaymentDate.ToDateTime();
            }
            

            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Order = _mapper.Map<Order.V1.Order>(order);

                await transaction.CommitAsync(context.CancellationToken);
                return response;
            }

            
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error, no changes were made to the order with id";
            _logger.LogError("Не было внесено никаких изменений в заказ  с идентификатором {OrderId}", order.Id);
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка обновления заказа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = $"Error updating order with id {order.Id} in DB";
        }

        await transaction.RollbackAsync(context.CancellationToken);
        return response;
    }

    public override async Task<SetOrderStatusCancelledResponse> SetOrderStatusCancelled(
        SetOrderStatusCancelledRequest request, ServerCallContext context)
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
            .Include(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);

        if (order is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Order does not exist";
            _logger.LogWarning("Заказа с идентификатором {id} не существует", request.OrderId);

            return response;
        }

        await using var transaction =
            await applicationContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            if (order.Status == OrderStatus.Paid)
            {
                if (order.User.Balance is null)
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description =
                        "Unable to debiting tips when canceling a paid order, the user has no balance";
                    _logger.LogError(
                        "Невозможно списать подсказки при отмене оплаченного заказа, у пользователя нет баланса");

                    return response;
                }
                // Списание баланса согласно тарифу заказа
                var updateBalanceResult = await _balanceService.DebitTipsFromBalance(
                    applicationContext,
                    order.User.Balance.Id,
                    BalanceOperationType.Debiting.ToString(),
                    context.CancellationToken,
                    (order.Tariff.FreeTipsCount ?? 0) + (order.Tariff.PaidTipsCount ?? 0)
                );

                if (updateBalanceResult is null)
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description = "Error debiting tips when canceling a paid order";
                    _logger.LogError("Ошибка списания подсказок при отмене оплаченного заказа {Id}", order.Id);

                    return response;
                }
            }

            order.Status = OrderStatus.Cancelled;

            // Обновление счета, установка такого же статуса, как у заказа
            if (order.Invoice is not null &&
                _invoiceService.UpdateInvoiceStatus(order.Invoice, applicationContext, InvoiceStatusEnum.Cancelled))
            {
                _logger.LogWarning(
                    "Счет {InvoiceGuid}, который привязан к заказу с id {OrderId} был переведён в статус Canceled", 
                    order.Invoice?.Id, order.Id);
            }
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Order = _mapper.Map<Order.V1.Order>(order);

                await transaction.CommitAsync(context.CancellationToken);
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error, no changes were made to the order";
            _logger.LogError("Не было внесено никаких изменений в заказ");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка обновления заказа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Error updating order in DB";
        }

        await transaction.RollbackAsync();
        return response;
    }

    /// <summary>
    ///     Получение заказов для клиента 
    /// </summary>
    public override async Task<GetOrdersForClientResponse> GetOrdersForClient(GetOrdersForClientRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetOrdersForClientResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Получение почты пользователя из контекста запроса
        var userEmail = context.GetUserEmail();

        // Получение всех заказов пользователя
        var orders = await applicationContext
            .Orders
            .AsNoTracking()
            .Include(x => x.Invoice)
            .Include(x => x.Tariff)
            .Where(x => x.User.Email == userEmail)
            .ToListAsync(context.CancellationToken);

        // Если заказы не найдены, возвращается статус NoData
        if (orders.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "User does not have any orders";
            _logger.LogWarning("У пользователя с email {Email} нет заказов", userEmail);

            return response;
        }

        // Попытка смапить сущности в заказы gRPC
        try
        {
            response.Orders.AddRange(_mapper.Map<List<Order.V1.Order>>(orders));
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "An unexpected error occurred while processing the request";

            _logger.LogError("Возникла ошибка во время маппинга заказов: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            return response;
        }

        response.Response.Status = OperationStatus.Ok;

        return response;
    }
}