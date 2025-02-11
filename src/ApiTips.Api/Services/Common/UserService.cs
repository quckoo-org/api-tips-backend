using ApiTips.Api.MapperProfiles.Balance;
using ApiTips.Api.MapperProfiles.User;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal;
using ApiTips.Dal.schemas.system;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Api.Services.Common;

public class UserService : IUserService
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<UserService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    public UserService(IHostEnvironment env, ILogger<UserService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(UserProfile));
        });

        if (env.IsDevelopment())
        {
            config.CompileMappings();
            config.AssertConfigurationIsValid();
        }

        _mapper = new Mapper(config);
    }

    /// <summary>
    ///     Зарегистрированные сервисы
    /// </summary>
    private IServiceProvider Services { get; }

    public async Task<User?> GetUserByIdDetailed(long? userId, CancellationToken token)
    {
        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        var dbUser = await applicationContext.Users
            .AsNoTracking()
            .Include(x => x.Balance)
            .FirstOrDefaultAsync(x => x.Id == userId, token);

        return dbUser;
    }
    
    public async Task<User?> GetUserByIdWithRoleAccess(long? userId, CancellationToken token)
    {
        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        if (!userId.HasValue)
            return null;
        
        var dbUser = await applicationContext.Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Methods)
            .FirstOrDefaultAsync(user => user.Id == userId,
                token);

        return dbUser;
    }
}
