using System.Text.Json.Serialization;

namespace ApiTips.Api.Models.Auth;

public class UserCredentials
{
    /// <summary>
    ///     JWT токен
    /// </summary>
    [JsonPropertyName("jwt")]
    public required string Jwt { get; set; }

    /// <summary>
    ///     Refresh токен
    /// </summary>
    [JsonPropertyName("refresh")]
    public required string Refresh { get; set; }
}