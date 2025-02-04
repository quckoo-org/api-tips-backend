namespace ApiTips.Integration.Brains.Server.Extensions.Security;

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
                            "https://beta.api-tips.quckoo.net/"
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