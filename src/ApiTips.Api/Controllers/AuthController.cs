using System.Text.Json;
using System.Text.Json.Serialization;
using ApiTips.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiTips.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration configuration, RedisService redisService) : ControllerBase
{
    private readonly string? _googleClientId = configuration.GetValue<string>("Google:ClientId");
    private readonly string? _googleClientSecret = configuration.GetValue<string>("Google:ClientSecret");
    private readonly string? _redirectUri = configuration.GetValue<string>("Google:RedirectUri");
    private readonly string? _tokenUri = configuration.GetValue<string>("Google:TokenUri");

    [HttpPost("exchange-code")]
    public async Task<IActionResult> ExchangeCode([FromBody] ExchangeCodeRequest request)
    {
        if (string.IsNullOrEmpty(request.Code)) return BadRequest("Authorization code is required.");

        using var httpClient = new HttpClient();
        var tokenRequestBody = new Dictionary<string, string>
        {
            { "code", request.Code },
            { "client_id", _googleClientId ?? string.Empty },
            { "client_secret", _googleClientSecret ?? string.Empty },
            { "redirect_uri", _redirectUri ?? string.Empty },
            { "grant_type", "authorization_code" }
        };

        var requestContent = new FormUrlEncodedContent(tokenRequestBody);

        var response = await httpClient.PostAsync(_tokenUri, requestContent, HttpContext.RequestAborted);
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(HttpContext.RequestAborted);
            return BadRequest($"Failed to exchange code: {errorResponse}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(HttpContext.RequestAborted);
        var tokens = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent); // TODO async

        var userGuid = Guid.NewGuid();
        var saved = await redisService.SetKeyAsync(userGuid.ToString(), responseContent, tokens?.ExpiresIn ?? 0);

        if (!saved) return BadRequest("Failed to save token.");
        
        HttpContext.Response.Cookies.Append("guid", userGuid.ToString(), new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            Expires = DateTime.UtcNow.AddSeconds(tokens?.ExpiresIn ?? 3600),
            SameSite = SameSiteMode.Strict
        });
            
        return Ok(new
        {
            Message = "Token saved successfully.",
        });
    }
}

public class ExchangeCodeRequest
{
    public string Code { get; set; }
}

public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }

    [JsonPropertyName("scope")] public string Scope { get; set; }

    [JsonPropertyName("token_type")] public string TokenType { get; set; }

    [JsonPropertyName("id_token")] public string IdToken { get; set; }
}