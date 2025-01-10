using ApiTips.Api.Models.Rbac;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Api.Services;

public class RbacService : IRbac
{
    private readonly ILogger<RbacService> _logger;

    public RbacService(ILogger<RbacService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;
    }

    private IServiceProvider Services { get; }

    /// <summary>
    ///     Получение списка ролей и разрешений для пользователя
    /// </summary>
    /// <param name="email">Почта аутентифицированного через Azure пользователя</param>
    /// <param name="cancellationToken">токен отмены асинхронной операции</param>
    /// <returns>Модель с ролями и разрешениями</returns>
    public async Task<RbacModel?> GetRights(string email, CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var request = await applicationContext
            .Users
            .AsNoTracking()
            .Include(opt0 => opt0.Roles)
            .ThenInclude(opt1 => opt1.Permissions)
            .ThenInclude(opt2 => opt2.Methods)
            .FirstOrDefaultAsync(role => role.Email == email, cancellationToken);

        if (request is null)
        {
            _logger.LogWarning("Пользователь {Email} отсутствует в БД", email);
            return null;
        }

        var response = new RbacModel();

        request.Roles.ForEach(role =>
        {
            response.Roles.Add(new Role
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = new List<Permission>(role.Permissions.Select(permission => new Permission
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Methods = new List<Method>(permission.Methods.Select(method => new Method
                    {
                        Id = method.Id,
                        Name = method.Name
                    }))
                }))
            });
        });

        return response;
    }
}