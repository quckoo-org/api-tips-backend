using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.Models.Auth;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal;
using ApiTips.Dal.schemas.system;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
[Consumes("application/json")]
[Produces("application/json")]
public class AuthController(
    IRedisService redis,
    IJwtService jwtService,
    IServiceProvider services,
    ILogger<AuthController> logger,
    IEmail _email) : ControllerBase
{
    private IServiceProvider Services { get; } = services;

    /// <summary>
    ///     Метод регистрации пользователя
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> UserRegister([FromBody] UserRegister request)
    {
        // Проверка модели на валидность
        if (!ModelState.IsValid || request.Email == null || request.Password == null || request.FirstName == null ||
            request.LastName == null || request.Cca3 == null)
            // Возврат 400 Bad Request с ошибками валидации
            return BadRequest(ModelState);

        // Проверка наличия пользователя в БД
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка наличия пользователя в БД
        var user = await applicationContext
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email, HttpContext.RequestAborted);

        // Если пользователь уже существует, то возвращаем ошибку
        if (user is not null)
            // Возврат 400 Bad Request с ошибкой
            return BadRequest(new
            {
                Message = $"Пользователь с почтой [{request.Email}] уже существует"
            });

        // Добавление пользователя в БД
        await applicationContext.Users.AddAsync(new User
        {
            Email = request.Email,
            Password = request.Password.ComputeSha256Hash()!,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Cca3 = request.Cca3 // TODO можно сделать проверку из нативных кодов C#
        });

        try
        {
            // Сохранение пользователя в БД
            if (await applicationContext.SaveChangesAsync(HttpContext.RequestAborted) <= 0)
                return BadRequest(new
                {
                    Message = $"Не удалось зарегистрировать пользователя с почтой [{request.Email}]"
                });

            #region unnecesery

            // // Создание JWT токена и Refresh токена
            // var credentials = jwtService.CreateUserCredentials(request, HttpContext.RequestAborted);
            // if (credentials is null)
            //     return BadRequest(new
            //     {
            //         Message = $"Не удалось зарегистрировать пользователя с почтой [{request.Email}]"
            //     });

            // // Получение времени жизни JWT и Refresh
            // var jWtTimeoutSeconds = jwtService.GetTokenExpirationTimeSeconds(credentials.Jwt);
            // var refreshTimeoutSeconds = jwtService.GetTokenExpirationTimeSeconds(credentials.Refresh);
            //
            // // Сохранение JWT и Refresh в Redis
            // var setJwt = await redis.SetKeyAsync($"{request.Email}:jwt", credentials.Jwt, jWtTimeoutSeconds);
            // var setRefresh =
            //     await redis.SetKeyAsync($"{request.Email}:refresh", credentials.Refresh, refreshTimeoutSeconds);
            //
            // // Если не удалось сохранить JWT или Refresh, то возвращаем ошибку
            // if (!setJwt || !setRefresh)
            //     return BadRequest(new
            //     {
            //         Message = $"Не удалось зарегистрировать пользователя с почтой [{request.Email}]"
            //     });

            //SetCookie("jwt", credentials.Jwt, jWtTimeoutSeconds);
            //SetCookie("refresh", credentials.Refresh, refreshTimeoutSeconds, true);

            #endregion

            await _email.SendEmailAsync(request.Email, "Успешная регистрация", 
                $"<h1>Вы успешно зарегистрировались</h1>" +
                $"<br>Добро пожаловать {request.FirstName} {request.LastName}!" +
                $"<br><br>Данные для входа в <a href='https://beta.api-tips.quckoo.net'>систему продажи подсказок</a> :" +
                $"<br><br><b>Ваш логин : </b> {request.Email}" +
                $"<br><b>Ваш пароль: </b> {request.Password}" +
                $"<br><br>Пожалуйста ожидайте активации, c Вами свяжутся наши менеджеры");
            
            return Ok(new
            {
                Message = $"Пользователь с почтой [{request.Email}] успешно зарегистрирован"
            });
        }
        catch (Exception e)
        {
            logger.LogError("An error was occured while saving user to database: {Error}", e.Message);
            return BadRequest(new
            {
                Message = $"Не удалось зарегистрировать пользователя с почтой [{request.Email}]"
            });
        }
    }

    /// <summary>
    ///     Метод получения JWT токена по авторизационным данным
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> UserLogin([FromBody] UserLogin request)
    {
        // Проверка модели на валидность
        if (!ModelState.IsValid || request.Email == null || request.Password == null)
            // Возврат 400 Bad Request с ошибками валидации
            return BadRequest(ModelState);

        // Проверка наличия пользователя в БД
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка наличия пользователя в БД
        var user = await applicationContext
            .Users
            .Include(user => user.Roles)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email && x.Password == request.Password.ComputeSha256Hash(),
                HttpContext.RequestAborted);

        // Если пользователь не существует, то возвращаем ошибку
        if (user is null)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                Message = $"Пользователь с почтой [{request.Email}] не существует либо пароль не верный"
            });

        // Проверка наличия токенов в Redis
        var jwtToken = await redis.GetStringKeyAsync($"{request.Email}:jwt", HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            jwtToken = jwtService.GenerateJwtToken(user.Email, user.FirstName, user.LastName, user.Cca3,
                user.Roles.Select(x => x.Name).ToList());
            if (string.IsNullOrWhiteSpace(jwtToken))
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to generate JWT token"
                });
            var setJwt = await redis.SetKeyAsync($"{request.Email}:jwt", jwtToken,
                jwtService.GetTokenExpirationTimeSeconds(jwtToken));
            if (!setJwt)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to save JWT token"
                });
        }

        // unnecesery
        //SetCookie("jwt-test", jwtToken, jwtService.GetTokenExpirationTimeSeconds(jwtToken));

        var refreshToken = await redis.GetStringKeyAsync($"{request.Email}:refresh", HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            refreshToken = jwtService.GenerateRefreshToken(request.Email);
            if (string.IsNullOrWhiteSpace(refreshToken))
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to generate refresh token"
                });
            var setRefresh = await redis.SetKeyAsync($"{request.Email}:refresh", refreshToken,
                jwtService.GetTokenExpirationTimeSeconds(refreshToken));
            if (!setRefresh)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to save JWT token"
                });
        }

        SetCookie("refresh", refreshToken, jwtService.GetTokenExpirationTimeSeconds(refreshToken), true);

        return Ok(new
        {
            Jwt = jwtToken,
            Message = "Successfully logged in"
        });
    }

    /// <summary>
    ///     Метод логаута пользователя
    /// </summary>
    /// <returns></returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> UserLogout()
    {
        // Проверяем, содержит ли запрос куку с определённым именем
        if (HttpContext.Request.Cookies.TryGetValue("refresh", out var refresh))
        {
            var claims = jwtService.ValidateJwtToken(refresh);
            if (claims is null)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to validate JWT token"
                });

            var emailClaim = claims.Claims.FirstOrDefault(c =>
                c.Type is "email" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            await redis.DeleteKeyAsync($"{emailClaim}:jwt");
            await redis.DeleteKeyAsync($"{emailClaim}:refresh");

            DeleteCookie("jwt");
            DeleteCookie("refresh");
        }
        else
        {
            logger.LogWarning("Refresh token not found in cookies");
            return StatusCode(StatusCodes.Status401Unauthorized, new
            {
                Message = "Refresh token not found in cookies"
            });
        }

        // Возвращаем успешный результат
        return Ok(new { Message = "Logout processed" });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> RefreshToken()
    {
        // Проверяем, содержит ли запрос куку с определённым именем
        if (HttpContext.Request.Cookies.TryGetValue("refresh", out var refreshToken))
        {
            var claims = jwtService.ValidateJwtToken(refreshToken);
            if (claims is null)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to validate Refresh token"
                });

            var emailClaim = claims.Claims.FirstOrDefault(c =>
                c.Type is "email" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            await redis.DeleteKeyAsync($"{emailClaim}:jwt");
            DeleteCookie("jwt");

            // Проверка наличия пользователя в БД
            await using var scope = Services.CreateAsyncScope();
            await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            // Проверка наличия пользователя в БД
            var user = await applicationContext
                .Users
                .Include(user => user.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == emailClaim, HttpContext.RequestAborted);

            // Если пользователь не существует, то возвращаем ошибку
            if (user is null)
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Message = $"Пользователь с почтой [{emailClaim}] не существует либо пароль не верный"
                });

            // Проверка наличия токенов в Redis
            var jwtToken = jwtService.GenerateJwtToken(user.Email, user.FirstName, user.LastName, user.Cca3,
                user.Roles.Select(x => x.Name).ToList());
            if (string.IsNullOrWhiteSpace(jwtToken))
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to generate JWT token"
                });
            var setJwt = await redis.SetKeyAsync($"{user.Email}:jwt", jwtToken,
                jwtService.GetTokenExpirationTimeSeconds(jwtToken));
            if (!setJwt)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to save JWT token"
                });

            // unnecesery
            //SetCookie("jwt", jwtToken, jwtService.GetTokenExpirationTimeSeconds(jwtToken));

            return Ok(
                new
                {
                    Jwt = jwtToken,
                    Message = "Refreshed successfully"
                });
        }

        logger.LogWarning("Refresh token not found in cookies");
        return StatusCode(StatusCodes.Status401Unauthorized, new
        {
            Message = "Refresh token not found in cookies"
        });
    }

    /// <summary>
    ///     Метод установки Cookie
    /// </summary>
    private void SetCookie(string key, string value, long expTime, bool httpOnly = false)
    {
        // Сохранение JWT в Cookie
        HttpContext.Response.Cookies.Append(key, value, new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = true,
            Expires = DateTime.UtcNow.AddSeconds(expTime),
            SameSite = SameSiteMode.None
        });
    }

    /// <summary>
    ///     Метод удаления Cookie
    /// </summary>
    private void DeleteCookie(string key)
    {
        if (HttpContext.Request.Cookies.ContainsKey(key))
        {
            HttpContext.Response.Cookies.Delete(key);
            logger.LogInformation("Cookie [{Key}] was deleted", key);
        }
        else
        {
            logger.LogDebug("Cookie [{Key}] not found", key);
        }
    }
}