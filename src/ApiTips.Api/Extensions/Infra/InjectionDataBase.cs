// namespace ApiTips.Api.Extensions.Infra;
//
// public static class InjectionDataBase
// {
//     // Получение данных для подключения к БД из конфига appsettings.json
//     public static WebApplicationBuilder AddPostgres(this WebApplicationBuilder builder)
//     {
//         var pgsqlHost = builder.Configuration.GetValue<string>("Postgres:Host");
//         var pgsqlPort = builder.Configuration.GetValue<string>("Postgres:Port");
//         var pgsqlUser = builder.Configuration.GetValue<string>("Postgres:User");
//         var pgsqlPassword = builder.Configuration.GetValue<string>("Postgres:Password");
//         var pgsqlDb = builder.Configuration.GetValue<string>("Postgres:Database");
//
//         // Формирование строки с данными для подключения к БД
//         var connectionString =
//             $"Host={pgsqlHost};Port={pgsqlPort};Database={pgsqlDb};Username={pgsqlUser};Password={pgsqlPassword};";
//
//         // Конфигурация подключения к БД ( DB context )
//         builder.Services.AddDbContext<ApplicationContext>(context =>
//         {
//             context.UseNpgsql(connectionString, opt =>
//             {
//                 opt.MigrationsAssembly("ApiTips.Api");
//                 opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
//             });
// #if DEBUG
//             context.EnableSensitiveDataLogging();
//             context.EnableDetailedErrors();
// #endif
//         });
//
//         return builder;
//     }
// }