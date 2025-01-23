using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.MapperProfiles.Payment;
using ApiTips.Api.Payment.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.Dal.Enums;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
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

    public ApiTipsPaymentService(IHostEnvironment env, ILogger<ApiTipsPaymentService> logger, IServiceProvider services)
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
    ///     Добавление платежных реквизитов  
    /// </summary>
    public override async Task<AddPaymentResponse> AddPayment(AddPaymentRequest request, ServerCallContext context)
    {
        // Создание базового ответа
        var response = new AddPaymentResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Получение email пользователя
        var userEmail = context.GetUserEmail();

        // Поиск пользователя по email в базе данных
        var user = await applicationContext.Users
            .Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Email == userEmail, context.CancellationToken);

        // Обработка ситуации, когда пользователя не удалось найти в базе данных
        if (user is null)
        {
            _logger.LogError("Пользователь с email {email} не найден", userEmail);
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = $"User with e-mail {userEmail} doesn't exist";

            return response;
        }

        var payment = _mapper.Map<Dal.schemas.data.Payment>(request.Payment);
        payment.User = user;

        await applicationContext.Payments.AddAsync(payment, context.CancellationToken);

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) == 0)
            {
                _logger.LogWarning("Платёжные реквизиты не были сохранены в базу данных для пользователя {userEmail}",
                    userEmail);

                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Payment doesn't saved in system";
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                "Данные не сохранены в базу данных: Exception: {exception} | InnerException {innerException}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Payment doesn't saved in system";
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<UpdatePaymentResponse> UpdatePayment(UpdatePaymentRequest request,
        ServerCallContext context)
    {
        // Создание базового ответа
        var response = new UpdatePaymentResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Получение email пользователя
        var userEmail = context.GetUserEmail();

        // Поиск пользователя по email в базе данных
        var user = await applicationContext.Users
            .Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Email == userEmail, context.CancellationToken);

        // Обработка ситуации, когда пользователя не удалось найти в базе данных
        if (user is null)
        {
            _logger.LogError("Пользователь с email {email} не найден", userEmail);
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = $"User with e-mail {userEmail} doesn't exist";

            return response;
        }

        var paymentFromDb = await applicationContext
            .Payments
            .Include(x => x.User)
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == request.Payment.Id, context.CancellationToken);

        if (paymentFromDb is null)
        {
            _logger.LogWarning(
                "Платёжные реквизиты с идентификатором {id} не были найдены в базе данных для пользователя {userEmail}",
                request.Payment.Id, userEmail);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = $"Payment with identifier {request.Payment.Id} doesn't exist";

            return response;
        }

        if ((request.Payment.PaymentTypeCase is Payment.V1.Payment.PaymentTypeOneofCase.BankAccount &&
             paymentFromDb.PaymentType != PaymentType.Money) ||
            (request.Payment.PaymentTypeCase is Payment.V1.Payment.PaymentTypeOneofCase.CryptoWallet &&
             paymentFromDb.PaymentType != PaymentType.Crypto))
        {
            _logger.LogWarning(
                "Платёжные реквизиты не будут изменены из-за разных платёжных систем для пользователя {userEmail}",
                userEmail);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Payment doesn't saved in system";
            return response;
        }

        paymentFromDb.Details = _mapper.Map<Dal.schemas.data.Payment.PaymentDetails>(request.Payment);
        paymentFromDb.User = user;
        paymentFromDb.IsBanned = request.Payment.IsBanned ?? false;


        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) == 0)
            {
                _logger.LogWarning("Платёжные реквизиты не были сохранены в базу данных для пользователя {userEmail}",
                    userEmail);

                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Payment doesn't saved in system";
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                "Данные не сохранены в базу данных: Exception: {exception} | InnerException {innerException}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Payment doesn't saved in system";
        }

        response.Response.Status = OperationStatus.Ok;
        response.Payment = _mapper.Map<Payment.V1.Payment>(paymentFromDb);

        return response;
    }

    public override async Task<GetPaymentResponse> GetPayment(GetPaymentRequest request, ServerCallContext context)
    {
        // Создание базового ответа
        var response = new GetPaymentResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var paymentFromDb = await applicationContext
            .Payments
            .Include(x => x.User)
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == request.Id, context.CancellationToken);

        if (paymentFromDb is null)
        {
            _logger.LogWarning(
                "Платёжные реквизиты с идентификатором {id} не были найдены в базе данных для пользователя",
                request.Id);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = $"Payment with identifier {request.Id} doesn't exist";

            return response;
        }

        response.Response.Status = OperationStatus.Ok;
        response.Payment = _mapper.Map<Payment.V1.Payment>(paymentFromDb);
        return response;
    }

    public override async Task<GetAllPaymentsResponse> GetAllPayments(GetAllPaymentsRequest request, ServerCallContext context)
    {
        // Создание базового ответа
        var response = new GetAllPaymentsResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
            Payments = { Capacity = 0}
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var paymentsFromDb = await applicationContext
            .Payments
            .Include(x => x.User)
            .Include(x => x.Details)
            .ToListAsync(context.CancellationToken);
        
        if (paymentsFromDb.Count == 0)
        {
            _logger.LogWarning(
                "Платёжные реквизиты не были найдены в базе данных");

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = $"Payments doesn't exist";

            return response;
        }

        
        response.Response.Status = OperationStatus.Ok;
        response.Payments.AddRange(_mapper.Map<List<Payment.V1.Payment>>(paymentsFromDb));
        return response;
    }
}