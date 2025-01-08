using System.Text.Json.Serialization;

namespace ApiTips.Api.Models.Auth;

public class UserCredentials
{
    /// <summary>
    ///     Email пользователя
    /// </summary>
    [JsonPropertyName("email")] 
    public string? Email { get; set; }
    
    /// <summary>
    ///     Пароль в формате SHA256  
    /// </summary>
    [JsonPropertyName("password")] 
    public string? Password { get; set; }
}