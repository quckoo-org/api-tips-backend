using ApiTips.Api.Access.V1;
using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.MapperProfiles.Tariff;
using ApiTips.Api.Tariff.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsTariffService:
    Tariff.V1.ApiTipsTariffService.ApiTipsTariffServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsTariffService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    public ApiTipsTariffService (IHostEnvironment env, ILogger<ApiTipsTariffService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(TariffProfile));
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

    public override async Task<GetTariffsResponse> GetTariffs(GetTariffsRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetTariffsResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var tariffs = applicationContext
            .Tariffs
            .AsNoTracking();

        // Фильтрация для получения скрытых/не скрытых тарифов
        if (request.Filter?.IsHidden != null)
            tariffs = tariffs.Where(tariff =>
                request.Filter.IsHidden == true
                    ? tariff.HideDateTime != null
                    : tariff.HideDateTime == null
            );

        // Отправка запроса после фильтрации в Базу данных
        var result = await tariffs.ToListAsync(context.CancellationToken);

        if (result.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены тарифы по заданным фильтрам";
            _logger.LogWarning("Не найдены тарифы по заданным фильтрам");
            return response;
        }

        response.Tariffs.AddRange(_mapper.Map<List<Tariff.V1.Tariff>>(result));
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<GetTariffResponse> GetTariff(GetTariffRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetTariffResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var tariff = await applicationContext.Tariffs
            .AsNoTracking()
            .FirstOrDefaultAsync(tariff => tariff.Id == request.TariffId,
                context.CancellationToken);

        if (tariff is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден тариф по заданному параметру";
            _logger.LogWarning("Не найден тариф по заданному параметру");

            return response;
        }

        // Маппинг тарифа из БД в ответ
        response.Tariff = _mapper.Map<Tariff.V1.Tariff>(tariff);
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<AddTariffResponse> AddTariff(AddTariffRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddTariffResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка на дублирование по названию тарифа
        var tariff = await applicationContext.Tariffs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == request.Name);

        if (tariff is not null)
        {
            response.Response.Status = OperationStatus.Duplicate;
            response.Response.Description = "Тариф с таким именем уже существует";
            _logger.LogWarning("Тариф с именем {name} уже существует", request.Name);

            return response;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не заполнено название тарифа";
            _logger.LogError("Не заполнено название тарифа");

            return response;
        }

        var tariffCandidate = new Dal.schemas.data.Tariff
        {
            Name = request.Name,
            Currency = "USD", // TODO исправить
            FreeTipsCount = request.HasFreeTipsCount ? request.FreeTipsCount : null,
            PaidTipsCount = request.HasPaidTipsCount ? request.PaidTipsCount : null,
            TotalPrice = request.TotalPrice.FromDecimal(),
            StartDateTime = request.StartDate.ToDateTime(),
            EndDateTime = request.EndDate != default ? request.EndDate.ToDateTime() : null,
            HideDateTime = request.HasIsHidden ? (request.IsHidden ? DateTime.UtcNow : null) : null
        };

        var tariffResult = await applicationContext.Tariffs.AddAsync(tariffCandidate, context.CancellationToken);

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Tariff = _mapper.Map<Tariff.V1.Tariff>(tariffResult.Entity);
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления тарифа в БД";
            _logger.LogError("Ошибка добавления тарифа в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка добавления тарифа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления тарифа в БД";
        }

        return response;
    }

    public override async Task<UpdateTariffResponse> UpdateTariff(UpdateTariffRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdateTariffResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Поиск редактируемого тарифа
        var tariff = await applicationContext.Tariffs
            .FirstOrDefaultAsync(x => x.Id == request.TariffId);

        if (tariff is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден тариф по заданному идентификатору";
            _logger.LogError("Не найден тариф по заданному идентификатору {id}", request.TariffId);

            return response;
        }

        if (request.HasName && !string.IsNullOrWhiteSpace(request.Name))
            tariff.Name = request.Name;
        if (request.HasFreeTipsCount)
            tariff.FreeTipsCount = request.FreeTipsCount;
        if (request.HasPaidTipsCount)
            tariff.PaidTipsCount = request.PaidTipsCount;
        if (request.TotalPrice != default)
            tariff.TotalPrice = request.TotalPrice.FromDecimal();
        if (request.StartDate != default)
            tariff.StartDateTime = request.StartDate.ToDateTime();
        if (request.EndDate != default)
            tariff.EndDateTime = request.EndDate.ToDateTime();
        if (request.HasIsHidden && request.IsHidden)
            tariff.HideDateTime = DateTime.UtcNow;

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Tariff = _mapper.Map<Tariff.V1.Tariff>(tariff);
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления тарифа в БД";
            _logger.LogError("Ошибка обновления тарифа в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка обновления тарифа в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления тарифа в БД";
        }

        return response;
    }
}

