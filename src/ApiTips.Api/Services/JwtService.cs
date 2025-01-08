using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiTips.Api.Services;

public class JwtService(IServiceProvider services, ILogger<JwtService> logger, IConfiguration configuration)
    : IJwtService
{
    private IServiceProvider Services { get; } = services;
    private IConfiguration Configuration { get; } = configuration;


    public async Task<string?> GetUserJwt(string email, string password, CancellationToken cancellationToken = default)
    {
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var user = await applicationContext
            .Users
            .FirstOrDefaultAsync(x => x.Email == email && x.Password == password, cancellationToken);

        if (user is not null)
        {
            logger.LogInformation("User {Email} was found", email);
            return GenerateJwtToken(email);
        }

        logger.LogWarning("User {Email} was not found", email);
        return null;
    }

    private string? GenerateJwtToken(string email)
    {
        var jwtSecretKey = Configuration.GetValue<string>("JwtSettings:SecretKey");
        var jwtAudience = Configuration.GetValue<string>("JwtSettings:Audience");
        var jwtIssuer = Configuration.GetValue<string>("JwtSettings:Issuer");
        var jwtExpirationInHours = Configuration.GetValue<int>("JwtSettings:ExpirationInHours");

        if (jwtSecretKey is null || jwtAudience is null || jwtIssuer is null || jwtExpirationInHours == 0)
        {
            logger.LogError("Jwt settings are not configured");
            return null;
        }

        // Указываем ключ шифрования
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        // Payload
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, email)
        };

        // Создаем JWT
        var token = new JwtSecurityToken(
            jwtIssuer,
            jwtAudience,
            claims,
            expires: DateTime.UtcNow.AddHours(jwtExpirationInHours),
            signingCredentials: signingCredentials
        );

        // Возвращаем строковое представление токена
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}