using ApiTips.Api.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ApiTips.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
[Consumes("application/json")]
[Produces("application/json")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    /// <summary>
    ///     Метод получения JWT токена по коду авторизации
    /// </summary>
    [HttpPost("credentials")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> ExchangeCode([FromBody] UserCredentials request)
    {
        // Проверка модели на валидность
        if (!ModelState.IsValid)
        {
            // Возврат 400 Bad Request с ошибками валидации
            return BadRequest(ModelState);
        }

        // TODO: Implement the method

        // Возврат 200 OK с сообщением об успешном сохранении токена в куки
        HttpContext.Response.Cookies.Append("guid", Guid.NewGuid().ToString(), new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            Expires = DateTime.UtcNow.AddSeconds(60), // TODO определить время жизни токена пока 60 секунд для проверки
            SameSite = SameSiteMode.Strict
        });

        return Ok(new
        {
            Message = "Token saved successfully to cookie"
        });
    }
}