using ApiTips.Api.Models.Auth;
using ApiTips.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiTips.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration configuration, RedisService redisService) : ControllerBase
{
    /// <summary>
    ///     Метод получения JWT токена по коду авторизации
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("credentials")]
    public async Task<IActionResult> ExchangeCode([FromBody] UserCredentials request)
    {
        // TODO: Implement the method
        
        return Ok(" -> JWT token");
    }
}
