using System.Reflection;
using ApiTips.Api.Enums;
using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.MapperProfiles.Payment;
using ApiTips.Api.Payment.V1;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.Dal.Enums;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using User = ApiTips.Dal.schemas.system.User;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsPaymentService
    : Payment.V1.ApiTipsPaymentService.ApiTipsPaymentServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsPaymentService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    ///     Зарегистрированные сервисы
    /// </summary>
    private IServiceProvider Services { get; }

    private readonly IEnumerable<IRequisites> _requisites;
    private readonly BankRequisites bankRequisites;
    private readonly CryptoRequisites _cryptoWallet;

    public ApiTipsPaymentService(IHostEnvironment env,IEnumerable<IRequisites> requisites,
        ILogger<ApiTipsPaymentService> logger, IServiceProvider services, IOptions<BankRequisites> br, IOptions<CryptoRequisites> cr)
    {
        _requisites = requisites;
        _logger = logger;
        Services = services;
        
        bankRequisites = br.Value;
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

    // public override async Task<GetPaymentResponse> GetPayment(GetPaymentRequest request, ServerCallContext context)
    // {
    //     // Создание базового ответа
    //     var response = new GetPaymentResponse
    //     {
    //         Response = new GeneralResponse
    //         {
    //             Status = OperationStatus.Unspecified
    //         }
    //     };
    //
    //     // Получение контекста базы данных из сервисов коллекций
    //     await using var scope = Services.CreateAsyncScope();
    //     await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    //
    //     var paymentFromDb = await applicationContext
    //         .Payments
    //         .Include(x => x.User)
    //         .Include(x => x.Details)
    //         .FirstOrDefaultAsync(x => x.Id == request.Id, context.CancellationToken);
    //
    //     if (paymentFromDb is null)
    //     {
    //         _logger.LogWarning(
    //             "Платёжные реквизиты с идентификатором {id} не были найдены в базе данных для пользователя",
    //             request.Id);
    //
    //         response.Response.Status = OperationStatus.Error;
    //         response.Response.Description = $"Payment with identifier {request.Id} doesn't exist";
    //
    //         return response;
    //     }
    //
    //     response.Response.Status = OperationStatus.Ok;
    //     response.Payment = _mapper.Map<Payment.V1.Payment>(paymentFromDb);
    //     return response;
    // }

    public override async Task<GetAllPaymentsResponse> GetAllPayments(GetAllPaymentsRequest request, ServerCallContext context)
    {
        // Создание базового ответа
        var response = new GetAllPaymentsResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
            Payments = new Payment.V1.Payment()
        };

        try
        {
            response.Payments.CryptoWallet = _mapper.Map<CryptoWallet>(_cryptoWallet);
            response.Payments.BankAccount = _mapper.Map<BankAccount>(bankRequisites);
        }
        catch (Exception e)
        {
            _logger.LogError("Возникла ошибка при маппинге сущностей: {exceptionMessage} | InnerException: {InnerMessage}"
                ,e.Message, e.InnerException?.Message);
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "An unexpected error occurred while retrieving payment requisites.";
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }
}