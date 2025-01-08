using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTips.Api.Models.Auth;

public class UserRegister
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

    /// <summary>
    ///     Имя пользователя
    /// </summary>
    [JsonPropertyName("firstname")]
    [Required]
    [MinLength(3, ErrorMessage = "FirstName must be at least 3 characters long")]
    public string? FirstName { get; set; }

    /// <summary>
    ///     Фамилия пользователя
    /// </summary>
    [JsonPropertyName("lastname")]
    [Required]
    [MinLength(3, ErrorMessage = "LastName must be at least 3 characters long")]
    public string? LastName { get; set; }

    /// <summary>
    ///     Имя пользователя
    /// </summary>
    [JsonPropertyName("cca3")]
    [Required]
    [MinLength(3, ErrorMessage = "Country code must be at least 3 characters long")]
    public string? Cca3 { get; set; }
}