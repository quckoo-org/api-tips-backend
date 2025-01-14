using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTips.Api.Models.Auth;

public class PasswordRecovery
{
    /// <summary>
    ///     Email пользователя
    /// </summary>
    [JsonPropertyName("email")]
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}