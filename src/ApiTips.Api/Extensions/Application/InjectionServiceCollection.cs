using ApiTips.Api.ServiceInterfaces;
using ApiTips.Api.Services;

namespace ApiTips.Api.Extensions.Application;

/// <summary>
///     Расширение для регистрации сервисов в коллекции сервисов
/// </summary>
public static class InjectionServiceCollection
{
    /// <summary>
    ///     Регистрация сервисов в коллекции сервисов
    /// </summary>
    public static WebApplicationBuilder InjectServiceCollection(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();
        
        builder.Services.AddHttpClient();

        // Регистрация сервиса для работы с Rbac в коллекции сервисов (авторизация)
        builder.Services.AddScoped<IRbac, RbacService>();
        
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IEmail, EmailService>();

        // Регистрация параметризированного типа сделки в коллекции сервисов
        //  Параметры : WinLoss, ActionPercent, RakeBackPercent, HandsCount
        //  Используется агентами: DK
        builder.Services.AddTransient<IRequisites, CryptoRequisites>();

        // Регистрация параметризированного типа сделки в коллекции сервисов
        //  Параметры : WinLoss, ActionPercent
        //  Используется агентами: Rich
        builder.Services.AddTransient<IRequisites, BankRequisites>();
        
        
        builder.Services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

        return builder;
    }
}