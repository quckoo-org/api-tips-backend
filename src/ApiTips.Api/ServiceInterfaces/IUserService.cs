using ApiTips.Dal.schemas.system;

namespace ApiTips.Api.ServiceInterfaces;

public interface IUserService
{
    /// <summary>
    ///     Получение пользователя по идентификатору с детальной информацией (Личный кабинет)
    /// </summary>
    Task<User?> GetUserByIdDetailed(long? userId, CancellationToken token);

    /// <summary>
    ///     Получение пользователя по идентификатору с возможностями данной учетной записи
    /// </summary>
    Task<User?> GetUserByIdWithRoleAccess(long? userId, CancellationToken token);
}