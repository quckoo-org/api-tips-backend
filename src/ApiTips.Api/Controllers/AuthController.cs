using ApiTips.Api.Models.Auth;
using ApiTips.Api.ServiceInterfaces;
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
public class AuthController(IConfiguration configuration, IRedisService redis, IJwtService jwtService) : ControllerBase
{
    /// <summary>
    ///     Метод получения JWT токена по коду авторизации
    /// </summary>
    [HttpPost("credentials")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> ExchangeCode([FromBody] UserCredentials request)
    {
        // Проверка модели на валидность
        if (!ModelState.IsValid || request.Email == null || request.Password == null)
            // Возврат 400 Bad Request с ошибками валидации
            return BadRequest(ModelState);

        // Проверка наличия токена в Redis
        var token = await redis.GetStringKeyAsync(request.Email, HttpContext.RequestAborted);

        // Если токен не найден, то формируем новый (если пользователь есть в БД)
        if (token is not null)
            return Ok(new
            {
                Message = "JWT saved successfully to cookie"
            });


        // Получение JWT токена
        var jwt = await jwtService.GetUserJwt(request.Email, request.Password, HttpContext.RequestAborted);
        if (jwt is null)
            return BadRequest(new
            {
                Message = "User not found"
            });

        var expTime = jwtService.GetJwtExpirationTimeSeconds(jwt);

        // Сохранение JWT в Redis
        await redis.SetKeyAsync(request.Email, jwt, expTime, HttpContext.RequestAborted);

        // Сохранение JWT в Cookie
        HttpContext.Response.Cookies.Append("jwt", jwt, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            Expires = DateTime.UtcNow.AddSeconds(expTime),
            SameSite = SameSiteMode.Strict
        });

        return Ok(new
        {
            Message = "JWT saved successfully to cookie"
        });
    }
}