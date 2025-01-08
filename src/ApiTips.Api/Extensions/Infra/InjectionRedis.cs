using ApiTips.Api.Services;
using StackExchange.Redis;

namespace ApiTips.Api.Extensions.Infra;

/// <summary>
///     Расширение для конфигурации и регистрации подключения к БД (Redis)
/// </summary>
public static class InjectionRedis
{
    /// <summary>
    ///     Регистрация и конфигурация подключения к БД
    /// </summary>
    public static WebApplicationBuilder AddRedis(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints =
                {
                    {
                        builder.Configuration.GetValue<string>("Redis:Host") ?? string.Empty,
                        builder.Configuration.GetValue<int>("Redis:Port")
                    }
                },
                AbortOnConnectFail = builder.Configuration.GetValue<bool>("Redis:AbortConnect"),
                Password = builder.Configuration.GetValue<string>("Redis:Password"),
                ConnectRetry = builder.Configuration.GetValue<int>("Redis:ConnectRetry"),
                ConnectTimeout = builder.Configuration.GetValue<int>("Redis:ConnectTimeoutSeconds") * 1000,
                ReconnectRetryPolicy = new ExponentialRetry(10 * 1000)
            })
        );
        
        builder.Services.AddSingleton<RedisService>();
        
        return builder;
    }
}