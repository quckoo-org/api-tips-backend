using System.IO.Compression;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiTips.Integration.Brains.Client.Extensions.Application;

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

        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

        return app;
    }
}