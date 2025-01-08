using ApiTips.Api.ServiceInterfaces;
using ApiTips.Api.Services;
using StackExchange.Redis;

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
        builder.Services.AddHttpClient();
        
        builder.Services.AddScoped<IJwtService,JwtService>();
        
        builder.Services.AddControllers();

        return builder;
    }
}