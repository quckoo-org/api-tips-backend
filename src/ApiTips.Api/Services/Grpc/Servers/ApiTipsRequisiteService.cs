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

    private readonly BankRequisites _bankRequisites;
    private readonly CryptoRequisites _cryptoWallet;

    public ApiTipsRequisiteService(IHostEnvironment env,
        ILogger<ApiTipsRequisiteService> logger, IServiceProvider services, IOptions<BankRequisites> br, IOptions<CryptoRequisites> cr)
    {
        _logger = logger;
        Services = services;
        
        _bankRequisites = br.Value;
        _cryptoWallet = cr.Value;

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

    public override async Task<GetAllRequisitesResponse> GetAllRequisites (GetAllRequisitesRequest request, ServerCallContext context)
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
     
        var bank = new Requisite.PaymentDetails.BankAccount
        {
            BankName = "FakeBank",
            BankAddress = "1234 Imaginary St, Nowhere City",
            Swift = "FAKEBANKXX",
            AccountNumber = "9876543210",
            Iban = "FAKEIBAN1234567890",
            AdditionalInfo = "Some additional bank details",
            Type = "USD"
        };

        var crypto = new Requisite.PaymentDetails.CryptoWallet
        {
            Address = "0xFAKE1234ABCD5678EFGH91011IJKL",
            Wallet = "WALLET1234FAKE",
            Token = "super_secret_fake_token",
            Type = "USDT"
        };

        var reqv = new Requisite
        {
            PaymentType = PaymentType.Bank,
            IsBanned = false,
            PaymentRequisites = new Requisite.PaymentDetails
            {
                BankAccountDetails = bank
            }
        };
        
        var reqv2 = new Requisite
        {
            PaymentType = PaymentType.Crypto,
            IsBanned = false,
            PaymentRequisites = new Requisite.PaymentDetails
            {
                CryptoWalletDetails = crypto
            }
        };
        
        applicationContext.Requisites.AddRange(reqv, reqv2);
        await applicationContext.SaveChangesAsync();
        
        
        var requisites = await applicationContext
            .Requisites
            .ToListAsync(context.CancellationToken);
        
        try
        {
            // Подразумевается, что в базе данных может быть только один счёт крипто-кошелька, берётся первый реквизит типа Крипто
            var cryptoWallet = requisites.FirstOrDefault(x => x.PaymentType == PaymentType.Crypto);
            if (cryptoWallet is not null)
            {
                response.CryptoWallet = _mapper.Map<CryptoWallet>(_cryptoWallet);
                response.CryptoWallet.IsBanned = cryptoWallet.IsBanned;
            }

            // Подразумевается, что в базе данных может быть только один банковский счёт, берётся первый реквизит типа Банк
            var bankWallet = requisites.FirstOrDefault(x => x.PaymentType == PaymentType.Bank);
            if (bankWallet is not null)
            {
                response.BankAccount = _mapper.Map<BankAccount>(bankWallet);
                response.BankAccount.IsBanned = bankWallet.IsBanned;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Возникла ошибка при маппинге сущностей реквизитов: {exceptionMessage} | InnerException: {InnerMessage}"
                ,e.Message, e.InnerException?.Message);
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "An unexpected error occurred while retrieving payment requisites.";
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<SetRequisiteActivityResponse> SetRequisiteActivity(SetRequisiteActivityRequest request, ServerCallContext context)
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
        
        var updatedRows = await applicationContext
            .Requisites
            .ExecuteUpdateAsync(x => x
                .SetProperty(r => r.IsBanned, r => request.RequisiteStatus), context.CancellationToken);
        if (updatedRows == 0)
        {
            _logger.LogWarning("Не удалось изменить статус реквизита с идентификатором {RequisiteId}", request.RequisiteId);
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "The requisite status was not changed.";

            return response;
        }
        
        response.Response.Status = OperationStatus.Ok;
        
        return response;
    }
}