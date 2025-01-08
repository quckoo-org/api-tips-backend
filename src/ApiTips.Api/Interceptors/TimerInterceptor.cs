using System.Diagnostics;
using ApiTips.Api.Extensions.Grpc;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Serilog.Context;

namespace ApiTips.Api.Interceptors;

public class TimerInterceptor(ILogger<TimerInterceptor> logger, IServiceProvider services, IHostEnvironment environment)
    : Interceptor
{
    private const string MessageTemplate =
        "[grpc-method-query-time] Query {RequestMethod} responded in {Seconds},{MilliSeconds} ms";

    private static readonly List<string?> ExcludedMethods =
    [
        null,
        "Check",
        "ServerReflectionInfo"
    ];

    private IServiceProvider Services { get; } = services;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var queryTimer = new Stopwatch();
        queryTimer.Start();

        try
        {
            var response = await base.UnaryServerHandler(request, context, continuation);
            queryTimer.Stop();

            await context.WriteResponseHeadersAsync(new Metadata
            {
                { "grpc-method-query-time", $"{queryTimer.Elapsed.Seconds},{queryTimer.Elapsed.Milliseconds} sec" }
            });

            LogContext.PushProperty("GrpcMethod", context.Method);
            LogContext.PushProperty("GrpcMethodShort", context.Method.Split('/').LastOrDefault());
            LogContext.PushProperty("User", context.GetUserEmail());
            LogContext.PushProperty("ExecutedTime",
                string.Format($"{queryTimer.Elapsed.Seconds},{queryTimer.Elapsed.Milliseconds} sec"));

            logger.LogDebug(MessageTemplate,
                context.Method.Split('/').LastOrDefault() ?? "unknown",
                queryTimer.Elapsed.Seconds, queryTimer.Elapsed.Milliseconds);

            // if (ExcludedMethods.Contains(context.Method.Split('/').LastOrDefault()))
            //     return response;


            return response;
        }
        catch (Exception e)
        {
            logger.LogError("Произошла ошибка | Exception {Exception} | InnerException {InnerException}",
                e.Message, e.InnerException?.Message);
            return await base.UnaryServerHandler(request, context, continuation);
        }
        finally
        {
            queryTimer.Stop();
        }
    }
}