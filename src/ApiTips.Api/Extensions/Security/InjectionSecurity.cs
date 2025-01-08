namespace ApiTips.Api.Extensions.Security;

/// <summary>
///     Класс расширения для конфигурации Cors policy
/// </summary>
public static class InjectionSecurity
{
    /// <summary>
    ///     Конфигурация Cors policy
    /// </summary>
    public static WebApplicationBuilder ConfigureCorsPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ClientPermissionCombined", policy =>
            {
                // Разрешаем только необходимые методы
                policy
                    .WithMethods("GET", "POST", "OPTIONS", "PUT", "DELETE", "PATCH")
                    .WithHeaders("Content-Type", "Authorization", "X-Grpc-Web", "Grpc-TimeOut",
                        "X-Accept-Content-Transfer-Encoding", "X-User-Agent", "X-Grpc-Web")
                    .AllowCredentials()
                    .SetIsOriginAllowed(origin =>
                        origin is
                            "https://localhost:8080" or
                            "https://localhost:3000" or
                            "https://beta.api-tips.quckoo.net/"
                    )
                    .WithExposedHeaders("Content-Type", "Authorization", "Access-Control-Allow-Headers", "X-Grpc-Web",
                        "Grpc-TimeOut");
            });

            options.AddPolicy("ClientPermissionAll", policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowCredentials();
            });
        });

        return builder;
    }
}