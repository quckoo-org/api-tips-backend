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
    IEmail email,
    IConfiguration config) : ControllerBase
{
    private IServiceProvider Services { get; } = services;

    private readonly string _domainBackEnd = config.GetValue<string>("App:DomainBackEnd") ?? string.Empty;
    private readonly string _domainFrontEnd = config.GetValue<string>("App:DomainFrontEnd") ?? string.Empty;

    private const string NameOfProduct = "the Hint Sales System";//TODO узнать имя продукта
    
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

        var guid = Guid.NewGuid();
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
                Message = $"User with email [{request.Email}] already exist"
            });

        // Добавление пользователя в БД
        await applicationContext.Users.AddAsync(new User
        {
            Email = request.Email,
            Password = request.Password.ComputeSha256Hash()!,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AccessToken = Guid.NewGuid(),
            Cca3 = request.Cca3 // TODO можно сделать проверку из нативных кодов C#
        });

        try
        {
            // Сохранение пользователя в БД
            if (await applicationContext.SaveChangesAsync(HttpContext.RequestAborted) <= 0)
                return BadRequest(new
                {
                    Message = $"Failed to register user with email [{request.Email}]"
                });

            await email.SendEmailAsync(request.Email, "Successful Registration",
                $"<h1>You have successfully registered</h1>" +
                $"<br>{request.FirstName} {request.LastName}, welcome to   <a href='https://{_domainBackEnd}'>\"{NameOfProduct}\"</a> !" +
                $"<br><br><b>Your account details: </b>" +
                $"<br><br><b>Your login: </b> {request.Email}" +
                $"<br><b>Your password: </b> {request.Password}" +
                $"<br><br>Please wait for your account activation. Our managers will contact you.");
            
            return Ok(new
            {
                Message = $"User with email [{request.Email}] has been successfully registered"
            });
        }
        catch (Exception e)
        {
            logger.LogError("An error was occured while saving user to database: {Error}", e.Message);
            return BadRequest(new
            {
                Message = $"Failed to register user with email [{request.Email}]"
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
            .FirstOrDefaultAsync(x => x.Email == request.Email && x.Password == request.Password,
                HttpContext.RequestAborted);

        // Если пользователь не существует, то возвращаем ошибку
        if (user is null)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                Message = $"The user with email [{request.Email}] does not exist or the password is incorrect"
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
            var (claims, message) = jwtService.ValidateJwtToken(refresh);
            if (claims is null)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error was occured while trying to validate JWT token [{message}]"
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

    /// <summary>
    ///     Метод обновления JWT токена
    /// </summary>
    /// <returns></returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> RefreshToken()
    {
        // Проверяем, содержит ли запрос куку с определённым именем
        if (HttpContext.Request.Cookies.TryGetValue("refresh", out var refreshToken))
        {
            var (claims, message) = jwtService.ValidateJwtToken(refreshToken);
            if (claims is null)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error was occured while trying to validate refresh token [{message}]"
                });
            
            var emailClaim = claims.Claims.FirstOrDefault(c =>
                c.Type is "email" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            if(string.IsNullOrWhiteSpace(emailClaim))
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to get email claim"
                });
            
            // Проверка наличия refresh в Redis
            var redisRefresh = await redis.GetStringKeyAsync($"{emailClaim}:refresh", HttpContext.RequestAborted);
            if(string.IsNullOrWhiteSpace(redisRefresh) || redisRefresh != refreshToken)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to get refresh token from Redis"
                });
            
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
                    Message = $"The user with email [{emailClaim}] does not exist or the password is incorrect"
                });
            
            // Генерация нового JWT токена
            var jwtToken = jwtService.GenerateJwtToken(user.Email, user.FirstName, user.LastName, user.Cca3,
                user.Roles.Select(x => x.Name).ToList());
            if (string.IsNullOrWhiteSpace(jwtToken))
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to generate JWT token"
                });
            
            // Установка нового JWT токена
            var setJwt = await redis.SetKeyAsync($"{user.Email}:jwt", jwtToken,
                jwtService.GetTokenExpirationTimeSeconds(jwtToken));
            if (!setJwt)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error was occured while trying to save JWT token"
                });
            
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
    ///     Метод для запроса восстановления пароля
    /// </summary>
    [HttpPost("recovery")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> RecoveryPassword([FromBody] PasswordRecovery model)
    {
        // Проверка модели на валидность
        if (string.IsNullOrWhiteSpace(model.Email))
            // Возврат 400 Bad Request с ошибками валидации
            return BadRequest(ModelState);

        // Проверка наличия пользователя в БД
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка наличия пользователя в БД
        var user = await applicationContext
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == model.Email, HttpContext.RequestAborted);

        // Если пользователь не существует, то возвращаем ошибку
        if (user is null)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                Message = $"The user with email [{model.Email}] does not exist or the password is incorrect"
            });

        var code = Guid.NewGuid().ToString();
        
        // Время действия кода восстановления пароля для пользователя
        const int activitySeconds = 900;
        
        var redisSetTemporarySecret = await redis.SetKeyAsync($"{user.Email}:reset", code, activitySeconds);

        if (!redisSetTemporarySecret)
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Password recovery error"
            });

        await email.SendEmailAsync(user.Email, "Password Reset",
            $"<h1>Password Reset</h1>" +
            $"<br>Dear {user.FirstName} {user.LastName}!" +
            $"<br><br>there was recently a request to change the password on your account. If you didn’t request to change your" +
            $" password, please ignore this email!" +
            $"<br><br><a href='https://{_domainFrontEnd}/reset?email={user.Email}&code={code}'>Your password Reset link</a>" +
            $"<br><h1>Attention! The link is active for {activitySeconds / 60} minutes</h1>");
        
        return Ok(new
        {
            Message = $"An email has been sent on [{user.Email}] to reset password"
        });
    }

    /// <summary>
    ///     Метод сброса пароля (ссылка из письма)
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordReset model)
    {
        if (string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.Email))
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                Message = "Not valid data to reset password"
            });

        var redisGetTemporarySecret = await redis.GetStringKeyAsync($"{model.Email}:reset", HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(redisGetTemporarySecret) || redisGetTemporarySecret != model.Code)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                Message = "Not valid data to reset password"
            });

        await redis.DeleteKeyAsync($"{model.Email}:jwt");
        await redis.DeleteKeyAsync($"{model.Email}:refresh");
        await redis.DeleteKeyAsync($"{model.Email}:reset");

        DeleteCookie("jwt");
        DeleteCookie("refresh");

        var code = Guid.NewGuid().ToString();
        const int activitySeconds = 60 * 5;
        var redisSetTemporarySecret = await redis.SetKeyAsync($"{model.Email}:recovery", code, activitySeconds);

        if (!redisSetTemporarySecret)
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Password recovery error"
            });
        
        return Ok(new
        {
            Code = code,
            Message = "Password reset code successfully received"
        });
    }

    /// <summary>
    ///     Метод для смены пароля (по коду из сброса)
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChange model)
    {
        // Проверка модели на валидность
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Code) ||
            string.IsNullOrWhiteSpace(model.Password))
            // Возврат 400 Bad Request с ошибками валидации
            return BadRequest(ModelState);

        var codeIsActive = await redis.GetStringKeyAsync($"{model.Email}:recovery", HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(codeIsActive) || codeIsActive != model.Code)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                Message = "Password recovery error | recovery time has expired or the code is incorrect"
            });

        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка наличия пользователя в БД
        var user = await applicationContext
            .Users
            .FirstOrDefaultAsync(x => x.Email == model.Email, HttpContext.RequestAborted);

        if (user is null)
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Password recovery error | user does not exist"
            });

        user.Password = model.Password.ComputeSha256Hash()!;

        try
        {
            if (await applicationContext.SaveChangesAsync() <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Password recovery error"
                });
        }
        catch (Exception e)
        {
            logger.LogError("An error was occured while saving user to database: {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Password recovery error"
            });
        }

        await redis.DeleteKeyAsync($"{model.Email}:recovery");
        
        await email.SendEmailAsync(user.Email, "Password Updated",
            $"<h1>Your password has been reset.</h1>" +
            $"<br>Dear {user.FirstName} {user.LastName}!" +
            $"<br><br>You have successfully reset your <a href='https://{_domainBackEnd}'>{NameOfProduct}</a>  account's password!" +
            $"<br><br><b>Your account details: </b>" +
            $"<br><br><b>Login: </b> {user.Email}" +
            $"<br><b>Password: </b> {model.Password}");

        return Ok(new
        {
            Message = $"User password with email [{model.Email}] successfully updated"
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
        var cookieOptions = new CookieOptions
        {
            Path = "/", 
            SameSite = SameSiteMode.None, 
            Secure = true
        };

        if (HttpContext.Request.Cookies.ContainsKey(key))
        {
            HttpContext.Response.Cookies.Delete(key, cookieOptions);
            logger.LogInformation("Cookie [{Key}] was deleted", key);
        }
        else
        {
            logger.LogDebug("Cookie [{Key}] not found", key);
        }
    }
}