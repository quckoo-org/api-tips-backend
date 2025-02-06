using ApiTips.Api.ServiceInterfaces;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ApiTips.Api.Interceptors;

public class RbacInterceptor(ILogger<RbacInterceptor> logger, IRbac rbac) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        /*
         * Пропускать gRPC запросы хелсчека без проверки токена в заголовке
         */
        if (context.Method.EndsWith("Health/Check") || 
            context.Method.EndsWith("ApiTipsTariffService/GetTariffs"))
            return await continuation(request, context);

        /*
         * Проверяем существует ли в метадате поле с email пользователя (уже провалидированного токеном)
         */
        var userEmail = context.RequestHeaders
            .FirstOrDefault(entry => entry
                .Key
                .Equals("email", StringComparison.OrdinalIgnoreCase))?
            .Value;

        /*
         * Не пропускаем gRPC запрос дальше без валидированной почты
         */
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            logger.LogWarning("В заголовке отсутствует email");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Have no email in your header"),
                new Metadata
                {
                    { "grpc-status", nameof(StatusCode.InvalidArgument) },
                    { "grpc-message", "Have no email in your header" }
                });
        }

        /*
         * Если есть роли и пермишшены добавляем в метадату и пропускаем gRPC запрос дальше
         */
        var rbacRights = await rbac.GetRightsAsync(userEmail, context.CancellationToken);
        if (rbacRights is null)
        {
            logger.LogWarning("Пользователь {Email} не имеет никаких прав доступа", userEmail);
        }
        else
        {
            var methods = new List<string>();

            rbacRights
                .Roles
                .ForEach(role =>
                    {
                        context.RequestHeaders.Add("role", role.Name);
                        role
                            .Permissions
                            .ForEach(permission => permission
                                .Methods
                                .ForEach(method => { methods.Add(method.Name); })
                            );
                    }
                );

            foreach (var method in methods.Distinct())
                context.RequestHeaders.Add("permission", method);
        }

        /*
         * Провека доступа к вызову метода и передача управления
         */
        if (GetAuthRightsByMetaData(context)) return await continuation(request, context);

        logger.LogWarning("У пользователя {Email} нет разрешений для вызова метода {Method}",
            userEmail, context.Method);
        throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission Denied"),
            new Metadata
            {
                { "grpc-status", nameof(StatusCode.PermissionDenied) },
                { "grpc-message", "Permission Denied" }
            });
    }

    /// <summary>
    ///     Сравнение разрещенных методов из метаданных с названием вызываемого метода
    /// </summary>
    private bool GetAuthRightsByMetaData(ServerCallContext context)
    {
        return context
            .RequestHeaders
            .Any(entry => context
                .Method
                .EndsWith(entry.Value)
            );
    }
}