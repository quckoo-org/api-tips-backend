using ApiTips.Api.ServiceInterfaces;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ApiTips.Api.Interceptors;

/// <summary>
///     gRPC Middleware
///     Основное назначение - проверка idToken из gRPC header на валидность и достоверность
///     1 - Получаем токен из заголовка запроса
///     2 - Получаем azure конфигурацию
///     3 - Валидация входящего токена
///     4 - В случае успеха - передача в контекст нового claim email полученного из провалидированого токена
///     5 - Передача управления дальше
///     Exceptions:
///     ПО не пропускает запрос дальше в случае:
///     1 - Отсутствие токена в заголовке (RpcException : InvalidArgument)
///     2 - Токен не валидный или истек (RpcException: Unauthenticated)
///     3 - Ошибка сервера (RpcException: Internal)
/// </summary>
public class AuthInterceptor(ILogger<AuthInterceptor> logger, IJwtService azure) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        /*
         * Пропускать gRPC запросы хелсчека без проверки токена в заголовке
         */
        if (context.Method.EndsWith("Health/Check"))
            return await continuation(request, context);

        /*
         * Проверка формата входящего токена в заголовке:
         * Пропускать :
         *  Authorization Bearer <idToken>
         *  Authorization <idToken>
         */
        var responseToken = context.RequestHeaders
            .FirstOrDefault(entry => entry
                .Key
                .Equals("Authorization", StringComparison.OrdinalIgnoreCase))?
            .Value
            .Split(' ');

        var token = responseToken?.Length switch
        {
            2 => responseToken[1],
            3 => responseToken[2],
            _ => null
        };

        /*
         * Проверка существования токена в заголовке
         */
        if (token is null)
        {
            logger.LogWarning("В заголовке отсутствует IdToken");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Have no token in your header"),
                new Metadata
                {
                    { "grpc-status", nameof(StatusCode.InvalidArgument) },
                    { "grpc-message", "Have no token in your header" }
                });
        }

        /*
         * Валидация токена через azure
         */
        var claims = azure.ValidateJwtToken(token);
        var userEmail = claims?.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            logger.LogWarning("Полученый в заголовке IdToken не прошёл валидацию azure");
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Token wasn't validated"),
                new Metadata
                {
                    { "grpc-status", nameof(StatusCode.Unauthenticated) },
                    { "grpc-message", "Token wasn't validated" }
                });
        }

        /*
         * Очищаем все метаданные
         */
        context.RequestHeaders.Clear();

        /*
         * Присваиваем проверенный email
         */
        context.RequestHeaders.Add("email", userEmail);

        return await continuation(request, context);
    }
}

