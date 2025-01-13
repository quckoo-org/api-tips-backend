using System.Security.Claims;
using ApiTips.Api.Models.Auth;

namespace ApiTips.Api.ServiceInterfaces;

public interface IJwtService
{
    /// <summary>
    ///     Создание JWT токена для пользователя
    /// </summary>
    UserCredentials? CreateUserCredentials(UserRegister model, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Генерация JWT токена
    /// </summary>
    string? GenerateJwtToken(string email, string firstname, string lastname, string cca3, List<string> roles);

    /// <summary>
    ///     Генерация Refresh токена
    /// </summary>
    string? GenerateRefreshToken(string email);

    /// <summary>
    ///     Получение времени жизни токена в секундах
    /// </summary>
    long GetTokenExpirationTimeSeconds(string? existToken);

    /// <summary>
    ///     Валидация и расшифровка JWT токена
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Tuple<ClaimsPrincipal?,string?> ValidateJwtToken(string? token);
}