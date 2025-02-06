using ApiTips.Api.ServiceInterfaces;
using ApiTips.Api.Services;
using ApiTips.Api.Services.Common;

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

        builder.Services.AddScoped<IBalanceService, BalanceService>();

        builder.Services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

        return builder;
    }
}