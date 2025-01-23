using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.MapperProfiles.Payment;
using ApiTips.Api.MapperProfiles.Tariff;
using ApiTips.Api.Order.V1;
using ApiTips.Api.Payment.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

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
    
    public ApiTipsPaymentService (IHostEnvironment env, ILogger<ApiTipsPaymentService> logger, IServiceProvider services)
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
                _logger.LogWarning("Платёжные реквизиты не были сохранены в базу данных для пользователя {userEmail}", userEmail);

                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Payment doesn't saved in system";
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("Данные не сохранены в базу данных: Exception: {exception} | InnerException {innerException}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Payment doesn't saved in system";
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }
}