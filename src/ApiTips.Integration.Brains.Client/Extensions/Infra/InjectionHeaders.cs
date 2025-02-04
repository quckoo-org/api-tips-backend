using Microsoft.AspNetCore.HttpOverrides;

namespace ApiTips.Integration.Brains.Client.Extensions.Infra;

/// <summary>
///     Расширение для конфигурации и регистрации заголовков
/// </summary>
public static class InjectionHeaders
{
    /// <summary>
    ///     Конфигурация и регистрация заголовков
    /// </summary>
    public static WebApplicationBuilder ConfigureMetaInfo(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        return builder;
    }
}