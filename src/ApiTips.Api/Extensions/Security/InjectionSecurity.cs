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
            options.AddPolicy("ClientPermissionCombinedOld", policy =>
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
                            "http://localhost:8080" or
                            "https://localhost:3000" or
                            "http://localhost:3000" or
                            "https://dev.api-tips.quckoo.net/" or
                            "https://stage.api-tips.quckoo.net/" or
                            "https://prod.api-tips.quckoo.net/"
                    )
                    .WithExposedHeaders("Content-Type", "Authorization", "Access-Control-Allow-Headers", "X-Grpc-Web",
                        "Grpc-TimeOut");
            });
            
            options.AddPolicy("ClientPermissionCombined", policy =>
            {
                // Разрешаем только необходимые методы
                policy
                    .WithMethods("GET", "POST", "OPTIONS", "PUT", "DELETE", "PATCH")
                    .WithHeaders(
                        "keep-alive",
                        "user-agent",
                        "cache-control",
                        "content-type",
                        "content-transfer-encoding",
                        "x-accept-content-transfer-encoding",
                        "x-accept-response-streaming",
                        "x-user-agent",
                        "x-grpc-web",
                        "grpc-timeout",
                        "authorization",
                        "grpc-method-query-time",
                        "cookie",
                        "grpc-status",
                        "grpc-message",
                        "set-cookie")
                    .AllowCredentials()
                    .SetIsOriginAllowed(origin =>
                        origin is
                            "https://localhost:8080" or
                            "https://localhost:3000" or
                            "https://dev.api-tips.quckoo.net/" or
                            "https://stage.api-tips.quckoo.net/" or
                            "https://prod.api-tips.quckoo.net/"
                    )
                    .WithExposedHeaders(
                        "Content-Type", 
                        "Authorization", 
                        "Access-Control-Allow-Headers", 
                        "X-Grpc-Web",
                        "Grpc-TimeOut",
                        "grpc-status",
                        "grpc-message");
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