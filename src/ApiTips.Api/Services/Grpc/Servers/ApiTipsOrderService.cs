using ApiTips.Api.MapperProfiles.Order;
using ApiTips.Api.Order.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

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

    public ApiTipsOrderService(IHostEnvironment env, ILogger<ApiTipsOrderService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;

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

        if (order.Status != Dal.Enums.OrderStatus.Paid)
        {
            order.Status = Dal.Enums.OrderStatus.Paid;

            // to do Пополнение баланса согласно тарифу заказа
        }

        order.PaymentDateTime = request.PaymentDate.ToDateTime();

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Order = _mapper.Map<Order.V1.Order>(order);
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
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);

        if (order is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Заказа с таким идентификатором не существует";
            _logger.LogWarning("Заказа с идентификатором {id} не существует", request.OrderId);

            return response;
        }

        if (order.Status == Dal.Enums.OrderStatus.Paid)
        {
            // to do Списывание подсказок с баланса согласно тарифу заказа
            // (на момент 20.01.2025 договорённость не опускать ниже 0)
        }

        order.Status = Dal.Enums.OrderStatus.Cancelled;

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Order = _mapper.Map<Order.V1.Order>(order);
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

        return response;
    }
}
