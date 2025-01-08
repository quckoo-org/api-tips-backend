using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiTips.Api.Models.Auth;
using ApiTips.Api.ServiceInterfaces;
using Microsoft.IdentityModel.Tokens;

namespace ApiTips.Api.Services;

public class JwtService(ILogger<JwtService> logger, IConfiguration configuration) : IJwtService
{
    private readonly string? _jwtAudience = configuration.GetValue<string>("JwtSettings:Audience");
    private readonly int _jwtExpirationInHours = configuration.GetValue<int>("JwtSettings:JwtExpirationInHours");
    private readonly string? _jwtIssuer = configuration.GetValue<string>("JwtSettings:Issuer");

    private readonly string? _jwtSecretKey = configuration.GetValue<string>("JwtSettings:SecretKey");

    private readonly int _refreshExpirationInHours =
        configuration.GetValue<int>("JwtSettings:RefreshExpirationInHours");

    public UserCredentials? CreateUserCredentials(UserRegister model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем токен отмены перед выполнением операции
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.FirstName) ||
                string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.Cca3)
               )
            {
                logger.LogError("Model is not valid");
                return null;
            }

            var jwtToken = GenerateJwtToken(model.Email, model.FirstName, model.LastName, model.Cca3, []);
            var refreshToken = GenerateRefreshToken(model.Email);

            if (jwtToken is null || refreshToken is null) return null;

            return new UserCredentials
            {
                Jwt = jwtToken,
                Refresh = refreshToken
            };
        }
        catch (Exception e)
        {
            logger.LogError("An error was occured while trying to create user credentials: {Message}", e.Message);
            return null;
        }
    }

    public string? GenerateJwtToken(string email, string firstname, string lastname, string cca3, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("email", email),
            new("firstname", firstname),
            new("lastname", lastname),
            new("cca3", cca3)
        };

        roles.ForEach(role => claims.Add(new Claim("roles", role)));

        return GenerateJwtToken(claims, _jwtExpirationInHours);
    }

    public string? GenerateRefreshToken(string email)
    {
        var claims = new List<Claim>
        {
            new("email", email)
        };

        return GenerateJwtToken(claims, _refreshExpirationInHours);
    }

    public long GetTokenExpirationTimeSeconds(string? existToken)
    {
        if (string.IsNullOrWhiteSpace(existToken))
        {
            logger.LogError("JWT cannot be null or empty");
            return 0;
        }

        try
        {
            // Создаём обработчик для JWT
            var jwtHandler = new JwtSecurityTokenHandler();

            // Проверяем, что строка действительно является JWT
            if (!jwtHandler.CanReadToken(existToken))
            {
                logger.LogError("Invalid JWT format");
                return 0;
            }

            // Декодируем токен
            var token = jwtHandler.ReadJwtToken(existToken);

            // Извлекаем claim `exp` (время истечения)
            var expClaim = token.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp);

            if (expClaim == null || !long.TryParse(expClaim.Value, out var expUnixTime))
            {
                logger.LogError("JWT does not contain a valid [exp] claim");
                return 0;
            }

            // Вычисляем оставшееся время до истечения токена
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTime);
            var remainingTimeSeconds = (expirationTime - DateTimeOffset.UtcNow).TotalSeconds;

            // Если токен уже истёк, возвращаем 0
            return remainingTimeSeconds > 0 ? (long)remainingTimeSeconds : 0;
        }
        catch (Exception ex)
        {
            logger.LogError("An error was occured while trying to get token expiration time: {Message}", ex.Message);
            return 0;
        }
    }

    public ClaimsPrincipal? ValidateJwtToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(_jwtSecretKey) ||
            string.IsNullOrWhiteSpace(_jwtAudience) ||
            string.IsNullOrWhiteSpace(_jwtIssuer) ||
            _jwtExpirationInHours <= 0)
        {
            logger.LogError("Jwt settings are not configured");
            return null;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogError("Token cannot be null or empty");
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Отключаем сопоставление стандартных типов Claims
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Отключаем сопоставление стандартных типов Claims
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Проверка ключа подписи
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey)), // Секретный ключ
                ValidateIssuer = true, // Проверка Issuer
                ValidIssuer = _jwtIssuer, // Ожидаемый Issuer
                ValidateAudience = true, // Проверка Audience
                ValidAudience = _jwtAudience, // Ожидаемая Audience
                ValidateLifetime = true, // Проверка срока действия токена
                ClockSkew = TimeSpan.Zero // Убираем стандартный допуск времени (5 минут)
            };

            // Проверяем токен и возвращаем ClaimsPrincipal
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

#if DEBUG
            if (principal != null)
                foreach (var claim in principal.Claims)
                    logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
#endif

            // Проверяем, использовался ли корректный алгоритм подписи
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return principal;

            logger.LogError("Invalid token algorithm");
            return null;
        }
        catch (SecurityTokenExpiredException)
        {
            logger.LogError("Token has expired");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            logger.LogError("Token is invalid: {Message}", ex.Message);
            return null;
        }
    }

    private string? GenerateJwtToken(List<Claim> claims, int expirationInHours)
    {
        // Проверяем, что все настройки для JWT заполнены
        if (string.IsNullOrWhiteSpace(_jwtSecretKey) ||
            string.IsNullOrWhiteSpace(_jwtAudience) ||
            string.IsNullOrWhiteSpace(_jwtIssuer) ||
            _jwtExpirationInHours <= 0)
        {
            logger.LogError("Jwt settings are not configured");
            return null;
        }

        try
        {
            // Создаём ключ для подписи токена
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));

            // Проверяем, что ключ имеет длину не менее 256 бит
            if (secretKey.KeySize < 256)
            {
                logger.LogError("JWT secret key is invalid");
                return null;
            }

            // Создаём учётные данные для подписи токена
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // Создаём JWT токен
            var token = new JwtSecurityToken(
                _jwtIssuer,
                _jwtAudience,
                claims,
                expires: DateTime.UtcNow.AddHours(expirationInHours),
                signingCredentials: signingCredentials
            );

            // Возвращаем строковое представление токена
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            logger.LogError("Error generating JWT: {Message}", ex.Message);
            return null;
        }
    }
}