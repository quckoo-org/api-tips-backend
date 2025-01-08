namespace ApiTips.Api.ServiceInterfaces;

public interface IJwtService
{
    /// <summary>
    ///     Создание JWT токена для пользователя
    /// </summary>
    Task<string?> GetUserJwt(string email, string password, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     Создание JWT токена для пользователя
    /// </summary>
    long GetJwtExpirationTimeSeconds(string? jwt);
}