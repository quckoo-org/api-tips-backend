using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTips.Api.Models.Auth;

public class PasswordReset
{
    /// <summary>
    ///     Email пользователя
    /// </summary>
    [JsonPropertyName("email")]
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    ///     Проверочный код
    /// </summary>
    [JsonPropertyName("code")]
    [Required]
    [MinLength(36, ErrorMessage = "Code must be at least 36 characters long")]
    public string? Code { get; set; }
}