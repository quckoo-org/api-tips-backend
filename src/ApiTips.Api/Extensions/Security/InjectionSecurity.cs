namespace ApiTips.Api.Extensions.Security;

public static class InjectionSecurity
{
    // Конфигурация Cors policy
    public static WebApplicationBuilder ConfigureCorsPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ClientPermissionCombined", policy =>
            {
                // Разрешаем только необходимые методы
                policy
                    .WithMethods("GET", "POST", "OPTIONS", "PUT", "DELETE", "PATCH") 
                    .WithHeaders("Content-Type", "Authorization", "X-Grpc-Web", "Grpc-TimeOut", "X-Accept-Content-Transfer-Encoding", "X-User-Agent", "X-Grpc-Web")
                    .AllowCredentials() 
                    .SetIsOriginAllowed(origin => 
                            origin == "https://localhost:8080" || origin == "https://x10club.ru" 
                    )
                    .WithExposedHeaders("Content-Type", "Authorization", "Access-Control-Allow-Headers", "X-Grpc-Web", "Grpc-TimeOut"); 
            });
        });

        return builder;
    }
}