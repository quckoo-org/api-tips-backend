using ApiTips.Integration.Brains.Server.Extensions.Application;
using ApiTips.Integration.Brains.Server.Extensions.Infra;
using ApiTips.Integration.Brains.Server.Extensions.Security;
using DotNetEnv;
using Serilog;

Env.Load();

var builder = WebApplication
    .CreateBuilder(new WebApplicationOptions
    {
        Args = args
    })
    .ConfigAndAddLogger()
    .ConfigAndAddKestrel()
    .ConfigureCorsPolicy()
    .InjectServiceCollection()
    .ConfigureMetaInfo();

var app = builder.Build();

// Лог с информацией о среде выполнения
Log
    .ForContext("Environment", app.Environment.EnvironmentName)
    .Debug("App activated in [{Environment}] mode", app.Environment.EnvironmentName);

app.UseForwardedHeaders();
if (app.Environment.IsProduction()) app.UseHsts();

app.UseRouting();

// Установка зарегистрированной Cors policy
app.UseCors("ClientPermissionAll");

// Маппинг контроллеров
app.MapControllers();

app.Run();