using ApiTips.Integration.Brains.Client.Extensions.Logger;
using Serilog;

namespace ApiTips.Integration.Brains.Client.Extensions.Application;

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
            .Enrich.With(new CommitEnricher(builder.Configuration.GetValue<string>("App:Commit") ?? null))
            .WriteTo.Console()
            .ReadFrom.Configuration(context.Configuration)
        );

        return builder;
    }
}