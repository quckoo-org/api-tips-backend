using ApiTips.Api.MapperProfiles.Payment;
using ApiTips.Api.Requisites.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.Dal.Enums;
using ApiTips.Dal.schemas.data;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsRequisiteService
    : ApiTipsRequisitesService.ApiTipsRequisitesServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsRequisiteService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    ///     Зарегистрированные сервисы
    /// </summary>
    private IServiceProvider Services { get; }

    public ApiTipsRequisiteService(IHostEnvironment env,
        ILogger<ApiTipsRequisiteService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(PaymentProfile));
        });

        if (env.IsDevelopment())
        {
            config.CompileMappings();
            config.AssertConfigurationIsValid();
        }

        _mapper = new Mapper(config);
    }

    /// <summary>
    ///     Получение всех реквизитов
    /// </summary>
    public override async Task<GetAllRequisitesResponse> GetAllRequisites(GetAllRequisitesRequest request,
        ServerCallContext context)
    {
        // Создание базового ответа
        var response = new GetAllRequisitesResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        var requisites = await applicationContext
            .Requisites
            .ToListAsync(context.CancellationToken);

        try
        {
            // Подразумевается, что в базе данных может быть только один счёт крипто-кошелька, берётся первый реквизит типа Крипто
            var cryptoWallet = requisites.FirstOrDefault(x => x.PaymentType == PaymentType.Crypto);
            if (cryptoWallet is not null)
            {
                response.CryptoWallet = _mapper.Map<CryptoWallet>(cryptoWallet.PaymentRequisites.CryptoWalletDetails);
                response.CryptoWallet.IsBanned = cryptoWallet.IsBanned;
                response.CryptoWallet.RequisiteId = cryptoWallet.Id;
            }

            // Подразумевается, что в базе данных может быть только один банковский счёт, берётся первый реквизит типа Банк
            var bankWallet = requisites.FirstOrDefault(x => x.PaymentType == PaymentType.Bank);
            if (bankWallet is not null)
            {
                response.BankAccount = _mapper.Map<BankAccount>(bankWallet.PaymentRequisites.BankAccountDetails);
                response.BankAccount.IsBanned = bankWallet.IsBanned;
                response.BankAccount.RequisiteId = bankWallet.Id;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Возникла ошибка при маппинге сущностей реквизитов: {exceptionMessage} | InnerException: {InnerMessage}"
                , e.Message, e.InnerException?.Message);
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "An unexpected error occurred while retrieving payment requisites.";
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    /// <summary>
    ///     Установка статуса активности счёта
    /// </summary>
    public override async Task<SetRequisiteActivityResponse> SetRequisiteActivity(SetRequisiteActivityRequest request,
        ServerCallContext context)
    {
        // Создание базового ответа
        var response = new SetRequisiteActivityResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        try
        {
            var requisite = await applicationContext.Requisites
                .FirstOrDefaultAsync(x => x.Id == request.RequisiteId,
                    context.CancellationToken);
            // Если реквизиты не найдены, то отмена выполнения метода
            if (requisite is null)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = $"The requisite with the identifier {request.RequisiteId} was not found.";

                return response; 
            }

            // Если статус счёта уже совпадает с тем, что отображено в запросе, то отмена выполнения метода
            if (requisite.IsBanned == request.IsBanned)
            {
                response.Response.Status = OperationStatus.NoData;
                response.Response.Description = $"The requisite with the identifier {request.RequisiteId} already has this status.";

                return response; 
            }

            requisite.IsBanned = request.IsBanned;
            
            await applicationContext.SaveChangesAsync(context.CancellationToken);
            
            response.Response.Status = OperationStatus.Ok;
            response.IsBanned = request.IsBanned;
            response.RequisiteId = requisite.Id;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Не удалось изменить статус реквизита с идентификатором {RequisiteId} Exception: {ExMessage} | Inner Exception: {InnerMessage}",
                request.RequisiteId, e.Message, e.InnerException?.Message);
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "The requisite status was not changed.";
        }

        return response;
    }
}