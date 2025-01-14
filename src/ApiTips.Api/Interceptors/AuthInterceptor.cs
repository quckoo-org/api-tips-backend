using ApiTips.Api.ServiceInterfaces;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ApiTips.Api.Interceptors;

/// <summary>
///     gRPC Middleware
///     Основное назначение - проверка idToken из gRPC header на валидность и достоверность
///     1 - Получаем токен из заголовка запроса
///     2 - Валидация токена и получение всех клеймов
///     3 - Проверка клейма почты на корректность
///     4 - В случае успеха - передача в контекст нового claim email полученного из провалидированого токена
///     5 - Проверка JWT токена на наличие в Redis
///     6 - Передача управления дальше
///     Exceptions:
///     ПО не пропускает запрос дальше в случае:
///     1 - Отсутствие токена в заголовке (RpcException : InvalidArgument)
///     2 - Токен не валидный или истек (RpcException: Unauthenticated)
///     3 - Токен отсутствует (RpcException: PermissionDenied)
///     4 - Ошибка сервера (RpcException: Internal)
/// </summary>
public class AuthInterceptor(ILogger<AuthInterceptor> logger, IJwtService jwtService, IRedisService redisService)
    : Interceptor
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
         *  Authorization Bearer <jwt>
         *  Authorization <jwt>
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
            logger.LogWarning("В заголовке отсутствует jwt token");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Have no jwt token in your header"),
                new Metadata
                {
                    { "grpc-status", nameof(StatusCode.InvalidArgument) },
                    { "grpc-message", "Have no jwt token in your header" }
                });
        }

        /*
         * Валидация токена и получение всех клеймов
         */
        var (claims, message) = jwtService.ValidateJwtToken(token);
        if (claims is null)
        {
            logger.LogWarning("Полученый в заголовке jwt token не прошёл валидацию, [{Message}]", message);
            throw new RpcException(new Status(StatusCode.Unauthenticated, $"Jwt token wasn't validated [{message}]"),
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
         * Поиск клейма почты
         */
        var emailClaim = claims.Claims.FirstOrDefault(c =>
            c.Type is "email" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

        /*
         * Проверка клейма почты на корректность
         */
        if (emailClaim is null)
        {
            logger.LogWarning("Полученый в заголовке jwt token не содержит идентификатор почты");
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Jwt token wasn't contain email claim"),
                new Metadata
                {
                    { "grpc-status", nameof(StatusCode.Unauthenticated) },
                    { "grpc-message", "Jwt token wasn't contain email claim" }
                });
        }

        /*
         * Проверка JWT токена на наличие в Redis
         */
        var existJwt = await redisService.GetStringKeyAsync($"{emailClaim}:jwt");
        if (string.IsNullOrWhiteSpace(existJwt))
        {
            logger.LogWarning("Полученый в заголовке jwt token больше не действителен");
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Jwt token is expired or not exist"),
                new Metadata
                {
                    { "grpc-status", nameof(StatusCode.PermissionDenied) },
                    { "grpc-message", "Jwt token is expired" }
                });
        }

        /*
         * Присваиваем проверенный email
         */
        context.RequestHeaders.Add("email", emailClaim);

        return await continuation(request, context);
    }
}