using ApiTips.Api.Models.Rbac;

namespace ApiTips.Api.ServiceInterfaces;

public interface IRbac
{
    /// <summary>
    ///     Получение списка ролей и разрешений для пользователя
    /// </summary>
    /// <param name="email">Почта аутентифицированного пользователя</param>
    /// <param name="cancellationToken">токен отмены асинхронной операции</param>
    /// <returns>Модель с ролями и разрешениями</returns>
    Task<RbacModel?> GetRightsAsync(string email, CancellationToken cancellationToken);   
}