using ApiTips.Api.MapperProfiles.Balance;
using ApiTips.Api.Balance.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

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

    public override Task<GetAggregatedBalanceHistoriesResponse> GetAggregatedBalanceHistories(GetAggregatedBalanceHistoriesRequest request, ServerCallContext context)
    {
        return base.GetAggregatedBalanceHistories(request, context);
    }
}
