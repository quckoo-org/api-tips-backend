using ApiTips.Dal;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Integration.Brains.Client.Extensions.Infra;

/// <summary>
///     Расширение для конфигурации и регистрации подключения к БД (Postgres)
/// </summary>
public static class InjectionDataBase
{
    /// <summary>
    ///     Регистрация и конфигурация подключения к БД
    /// </summary>
    public static WebApplicationBuilder AddPostgres(this WebApplicationBuilder builder)
    {
        var pgsqlHost = builder.Configuration.GetValue<string>("Postgres:Host");
        var pgsqlPort = builder.Configuration.GetValue<string>("Postgres:Port");
        var pgsqlUser = builder.Configuration.GetValue<string>("Postgres:User");
        var pgsqlPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? string.Empty;
        var pgsqlDb = builder.Configuration.GetValue<string>("Postgres:Database");

        // Формирование строки с данными для подключения к БД
        var connectionString =
            $"Host={pgsqlHost};Port={pgsqlPort};Database={pgsqlDb};Username={pgsqlUser};Password={pgsqlPassword};";

        // Конфигурация подключения к БД ( DB context )
        builder.Services.AddDbContext<ApplicationContext>(context =>
        {
            context.UseNpgsql(connectionString, opt =>
            {
                opt.MigrationsAssembly("ApiTips.Api");
                opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
#if DEBUG
            context.EnableSensitiveDataLogging();
            context.EnableDetailedErrors();
#endif
        });

        return builder;
    }
}