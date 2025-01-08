using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTips.Api.Models.Auth;

public class UserCredentials
{
    /// <summary>
    ///     Email пользователя
    /// </summary>
    [JsonPropertyName("email")]
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    ///     Пароль в формате SHA256  
    /// </summary>
    [JsonPropertyName("password")] 
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string? Password { get; set; }
}