using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace ApiTips.Api.Extensions.Application;

/// <summary>
///     Класс расширения для конфигурации Kestrel
/// </summary>
public static class InjectionKestrel
{
    // Конфигурация Kestrel
    public static WebApplicationBuilder ConfigAndAddKestrel(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel((_, opt) =>
        {
            var appHost = builder.Configuration.GetValue<string>("App:Host");
            var httpPort = builder.Configuration.GetValue<int>("App:Ports:Http1");

            opt.Limits.MinRequestBodyDataRate = null;

            opt.Listen(IPAddress.Parse(appHost ?? "0.0.0.0"), httpPort, listenOptions =>
            {
                Log.Information(
                    "The application [{AppName}] is successfully started at [{StartTime}] (UTC) | protocol http/https(debug) (http1) | port : [{Port}]",
                    AppDomain.CurrentDomain.FriendlyName,
                    DateTime.UtcNow.ToString("F"),
                    httpPort);

#if DEBUG
               listenOptions.UseHttps();
#endif

                listenOptions.Protocols = HttpProtocols.Http1;
            });

            opt.AllowAlternateSchemes = true;
        });

        builder.WebHost.ConfigureKestrel((_, opt) =>
        {
            var appHost = builder.Configuration.GetValue<string>("App:Host");
            var grpcPort = builder.Configuration.GetValue<int>("App:Ports:Http2");

            opt.Limits.MinRequestBodyDataRate = null;

            opt.Listen(IPAddress.Parse(appHost ?? "0.0.0.0"), grpcPort, listenOptions =>
            {
                Log.Information(
                    "The application [{AppName}] is successfully started at [{StartTime}] (UTC) | protocol gRPC (http2) | port : [{Port}]",
                    AppDomain.CurrentDomain.FriendlyName,
                    DateTime.UtcNow.ToString("F"),
                    grpcPort);

                listenOptions.Protocols = HttpProtocols.Http2;
            });

            opt.AllowAlternateSchemes = true;
        });

        return builder;
    }
}