using System.IO.Compression;
using ApiTips.Api.Interceptors;
using ApiTips.Api.Services.Grpc.Servers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiTips.Api.Extensions.Application;

/// <summary>
///     Расширение для конфигурации и регистрации gRPC сервиса
/// </summary>
public static class InjectionGrpc
{
    // Регистрация и конфигурация gRPC сервиса в коллекции сервисов
    public static WebApplicationBuilder ConfigAndAddGrpc(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc(options =>
        {
            // Промежуточное ПО для работы с Azure (аутентификация)
            options.Interceptors.Add<AuthInterceptor>();
            // Промежуточное ПО для работы с Rbac (авторизация)
            options.Interceptors.Add<RbacInterceptor>();
            // Промежуточное ПО для работы с логгированием времени запросов
            options.Interceptors.Add<TimerInterceptor>();

            options.IgnoreUnknownServices = false;
            options.MaxReceiveMessageSize = null;
            options.MaxSendMessageSize = null;
            options.ResponseCompressionLevel = CompressionLevel.Optimal;
            options.ResponseCompressionAlgorithm = "gzip";
            options.EnableDetailedErrors = false;
        });


        // Регистрация и конфигурация состояния gRPC сервиса в коллекции сервисов
        builder.Services.AddGrpcHealthChecks().AddCheck("grpc_health_check", () => HealthCheckResult.Healthy());

        // Регистрация сервсиа gRPC рефлексии в коллекции сервисов
        builder.Services.AddGrpcReflection();

        return builder;
    }

    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.UseRouting();

        // Добавьте поддержку gRPC-web
        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

        // Маппинг gRPC сервиса для работы с доступом
        app.MapGrpcService<ApiTipsAccessService>().EnableGrpcWeb();

        // Маппинг gRPC сервиса для работы с тарифами
        app.MapGrpcService<ApiTipsTariffService>().EnableGrpcWeb();

        // Маппинг gRPC сервиса для работы с заказами
        app.MapGrpcService<ApiTipsOrderService>().EnableGrpcWeb();

        return app;
    }
}