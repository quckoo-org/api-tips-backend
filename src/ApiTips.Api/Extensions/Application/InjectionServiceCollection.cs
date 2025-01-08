using ApiTips.Api.Services;
using StackExchange.Redis;

namespace ApiTips.Api.Extensions.Application;

public static class InjectionServiceCollection
{
    // Коллекция сервисов
    public static WebApplicationBuilder InjectServiceCollection(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();

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
        
        builder.Services.AddControllers();

        return builder;
    }
}