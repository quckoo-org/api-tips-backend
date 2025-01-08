using ApiTips.Api.Extensions.Logger;
using Serilog;

namespace ApiTips.Api.Extensions.Application;

/// <summary>
///     Класс расширения для конфигурации логгера
/// </summary>
public static class InjectionLogger
{
    /// <summary>
    ///     Конфигурация логгера
    /// </summary>
    public static WebApplicationBuilder ConfigAndAddLogger(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, lc) => lc
            .Enrich.WithCaller()
            .WriteTo.Console()
            .ReadFrom.Configuration(context.Configuration)
        );

        return builder;
    }
}