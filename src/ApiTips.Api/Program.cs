using System.Collections;
using System.Reflection;
using ApiTips.Api.Extensions.Application;
using ApiTips.Api.Extensions.Infra;
using ApiTips.Api.Extensions.Security;
using DotNetEnv;
using Microsoft.OpenApi.Models;
using Serilog;

Env.Load();

var builder = WebApplication
    .CreateBuilder(new WebApplicationOptions
    {
        Args = args
    })
    .ConfigAndAddLogger()
    .ConfigAndAddKestrel()
    .AddPostgres()
    .AddRedis()
    .ConfigureCorsPolicy()
    .InjectServiceCollection()
    .ConfigAndAddGrpc()
    .ConfigureMetaInfo();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Afisha API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Лог с информацией о среде выполнения
Log
    .ForContext("Environment", app.Environment.EnvironmentName)
    .Debug("App activated in [{Environment}] mode", app.Environment.EnvironmentName);

app.UseForwardedHeaders();
if (app.Environment.IsProduction()) app.UseHsts();

app.UseRouting();

// Установка зарегистрированной Cors policy
app.UseCors("ClientPermissionCombined");

// Маппинг gRPC сервисов
app.MapGrpcServices();

// Маппинг сервиса gRPC рефлексии
app.MapGrpcReflectionService();

// Маппинг сервиса healthcheck
app.MapHealthChecks("/healthz");

// Маппинг gRPC сервиса healthcheck
app.MapGrpcHealthChecksService();

// Маппинг контроллеров
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Afisha API v1");
    options.RoutePrefix = string.Empty; // Set the Swagger UI at the root URL
});


app.Run();