using ApiTips.Api.Access.V1;
using ApiTips.Api.Extensions.Grpc;
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

        return response;
    }
}
