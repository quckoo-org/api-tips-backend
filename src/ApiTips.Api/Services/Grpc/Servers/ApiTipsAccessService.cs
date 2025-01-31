using ApiTips.Api.Access.V1;
using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.MapperProfiles.Method;
using ApiTips.Api.MapperProfiles.Permission;
using ApiTips.Api.MapperProfiles.Role;
using ApiTips.Api.MapperProfiles.User;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Method = ApiTips.Dal.schemas.system.Method;
using Permission = ApiTips.Dal.schemas.system.Permission;
using Role = ApiTips.Dal.schemas.system.Role;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsAccessService
    : Access.V1.ApiTipsAccessService.ApiTipsAccessServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsAccessService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    ///     Сервис для работы с балансом
    /// </summary>
    private readonly IBalanceService _balanceService;

    public ApiTipsAccessService(IHostEnvironment env, ILogger<ApiTipsAccessService> logger, IServiceProvider services,
        IBalanceService balanceService)
    {
        _logger = logger;
        Services = services;
        _balanceService = balanceService;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(UserProfile));
            cfg.AddProfile(typeof(RoleProfile));
            cfg.AddProfile(typeof(PermissionProfile));
            cfg.AddProfile(typeof(MethodProfile));
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

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetUsersResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var users = applicationContext
            .Users
            .Include(x => x.Roles)
            .AsNoTracking();

        // Фильтрация по Email пользователя
        if (!string.IsNullOrWhiteSpace(request.Filter?.Email))
            users = users.Where(user => user.Email == request.Filter.Email);

        // Фильтрация для получения заблокированных/не заблокированных пользователей
        if (request.Filter?.IsBlocked != null)
            users = users.Where(user =>
                request.Filter.IsBlocked == true
                    ? user.LockDateTime != null
                    : user.LockDateTime == null
            );

        // Фильтрация для получения верифицированных/не верифицированных пользователей
        if (request.Filter?.IsVerified != null)
            users = users.Where(user =>
                request.Filter.IsVerified == true
                    ? user.VerifyDateTime != null
                    : user.VerifyDateTime == null
            );

        // Фильтрация для получения удаленных/не удаленных пользователей
        if (request.Filter?.IsDeleted != null)
            users = users.Where(user =>
                request.Filter.IsDeleted == true
                    ? user.DeleteDateTime != null
                    : user.DeleteDateTime == null
            );

        // Отправка запроса после фильтрации в Базу данных
        var result = await users.ToListAsync(context.CancellationToken);

        if (result.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены пользователи по заданным фильтрам";
            _logger.LogWarning("Не найдены пользователи по заданным фильтрам");
            return response;
        }

        response.Users.AddRange(_mapper.Map<List<User>>(result));
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var user = await applicationContext.Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Methods)
            .FirstOrDefaultAsync(user => user.Id == request.UserId,
                context.CancellationToken);

        if (user is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден пользователь по заданному параметру";
            _logger.LogWarning("Не найден пользователь по заданному параметру");

            return response;
        }

        // Маппинг пользователя из БД в ответ
        response.User = _mapper.Map<User>(user);
        response.Response.Status = OperationStatus.Ok;
        return response;
    }

    public override async Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка на дублирование по почте пользователя
        var user = await applicationContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user is not null)
        {
            response.Response.Status = OperationStatus.Duplicate;
            response.Response.Description = "Пользователь с таким email уже существует";
            _logger.LogWarning("Пользователь с таким email уже существует");

            return response;
        }

        // Поиск ролей соответствующих идентификаторам
        var roles = await applicationContext
            .Roles
            .Where(x => request.RolesIds.Any(y => y == x.Id))
            .ToListAsync(context.CancellationToken);

        // Проверка на наличие ролей
        if (roles.Count == 0)
        {
            response.Response.Description = "Не найдены роли по заданным идентификаторам";
            _logger.LogWarning("Не найдены роли по заданным идентификаторам");
        }

        if (
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Cca3)
        )
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не все обязательные поля заполнены";
            _logger.LogError("Не все обязательные поля заполнены");

            return response;
        }

        var password = Guid.NewGuid();

        var userCandidate = new Dal.schemas.system.User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Cca3 = request.Cca3,
            Password = password.ToString().ComputeSha256Hash()!, //! потому что тут мы явно генерируем пароль
            Roles = roles
        };

        var addResult = await applicationContext.Users.AddAsync(userCandidate, context.CancellationToken);

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                if (!await _balanceService.AddBalance(addResult.Entity.Id, context.CancellationToken))
                {
                    response.Response.Status = OperationStatus.Error;
                    response.Response.Description = "Error adding balance for new user to DB";
                    _logger.LogError("Ошибка добавления баланса для нового пользователя в БД");
                }
                
                response.Response.Status = OperationStatus.Ok;
                response.User = _mapper.Map<User>(userCandidate);

                // TODO : send password to user [ $password ]

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления пользователя в БД";
            _logger.LogError("Ошибка добавления пользователя в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка добавления пользователя в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления пользователя в БД";
        }

        return response;
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdateUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var user = await applicationContext.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.Id == request.UserId,
                context.CancellationToken);

        if (user is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден пользователь по заданному идентификатору";
            _logger.LogError("Не найден пользователь по заданному идентификатору");
            return response;
        }

        if (request.HasEmail && !string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (request.HasFirstName && !string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (request.HasLastName && !string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (request.HasCca3 && !string.IsNullOrWhiteSpace(request.Cca3))
            user.Cca3 = request.Cca3;

        if (request.HasIsDeleted)
            user.DeleteDateTime = request.IsDeleted ? DateTime.UtcNow : null;

        if (request.HasIsBlocked)
            user.LockDateTime = request.IsBlocked ? DateTime.UtcNow : null;

        if (request.HasIsVerified)
            user.VerifyDateTime = request.IsVerified ? DateTime.UtcNow : null;

        if (request.RolesIds.Count > 0)
        {
            // Поиск ролей соответствующих идентификаторам
            var roles = await applicationContext
                .Roles
                .Where(x => request.RolesIds.Any(y => y == x.Id))
                .ToListAsync(context.CancellationToken);

            switch (roles.Count)
            {
                // Проверка на наличие ролей
                case 0:
                    response.Response.Description = "Не найдены роли по заданным идентификаторам";
                    _logger.LogWarning("Не найдены роли по заданным идентификаторам");
                    break;
                case > 0:
                    user.Roles = roles;
                    break;
            }
        }


        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.User = _mapper.Map<User>(user);

                // TODO : send password to user [ $password ] | if update

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления пользователя в БД";
            _logger.LogError("Ошибка обновления пользователя в БД");

            return response;
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления пользователя в БД";
            _logger.LogError(
                "Ошибка во время обновления пользователя в БД | {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            return response;
        }
    }

    public override async Task<GetRolesResponse> GetRoles(GetRolesRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetRolesResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var roles = await applicationContext
            .Roles
            .AsNoTracking()
            .Include(x => x.Permissions)
            .ThenInclude(x => x.Methods)
            .ToListAsync(context.CancellationToken);

        if (roles.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены роли в базе данных";
            _logger.LogWarning("Не найдены роли в базе данных");

            return response;
        }

        // Маппинг ролей из БД в ответ
        response.Roles.AddRange(_mapper.Map<List<Access.V1.Role>>(roles));
        response.Response.Status = OperationStatus.Ok;

        return response;
    }

    public override async Task<GetRoleResponse> GetRole(GetRoleRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetRoleResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        if (request.RoleId is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не указан идентификатор роли";
            _logger.LogError("Не указан идентификатор роли");

            return response;
        }

        var role = await applicationContext
            .Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.RoleId, context.CancellationToken);

        if (role is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдена роль по заданному идентификатору";
            _logger.LogWarning("Не найдена роль по заданному идентификатору");

            return response;
        }

        // Маппинг роли из БД в ответ
        response.Role = _mapper.Map<Access.V1.Role>(role);
        response.Response.Status = OperationStatus.Ok;

        return response;
    }

    public override async Task<AddRoleResponse> AddRole(AddRoleRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddRoleResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка на дублирование по наименованию роли
        var role = await applicationContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == request.Name);

        if (role is not null)
        {
            response.Response.Status = OperationStatus.Duplicate;
            response.Response.Description = "Роль с таким наименованием уже существует";
            _logger.LogError("Роль с таким наименованием уже существует");

            return response;
        }

        // Поиск ролей соответствующих идентификаторам
        var permissions = await applicationContext
            .Permissions
            .Where(x => request.PermissionsIds.Any(y => y == x.Id))
            .ToListAsync(context.CancellationToken);

        // Проверка на наличие ролей
        if (permissions.Count == 0)
            _logger.LogWarning("Не найдены разрешения по заданным идентификаторам");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не все обязательные поля заполнены";
            _logger.LogError("Не все обязательные поля заполнены");

            return response;
        }

        var roleCandidate = new Role
        {
            Name = request.Name,
            Permissions = permissions
        };

        await applicationContext.Roles.AddAsync(roleCandidate, context.CancellationToken);

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Role = _mapper.Map<Access.V1.Role>(roleCandidate);

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления роли в БД";
            _logger.LogError("Ошибка добавления роли в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка добавления роли в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления роли в БД";
        }

        return response;
    }

    public override async Task<UpdateRoleResponse> UpdateRole(UpdateRoleRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdateRoleResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var role = await applicationContext.Roles
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == request.RoleId,
                context.CancellationToken);

        if (role is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдена роль по заданному идентификатору";
            _logger.LogError("Не найдена роль по заданному идентификатору");

            return response;
        }

        if (request.HasName && !string.IsNullOrWhiteSpace(request.Name))
            role.Name = request.Name;

        if (request.PermissionsIds.Count > 0)
        {
            // Поиск ролей соответствующих идентификаторам
            var permissions = await applicationContext
                .Permissions
                .Where(x => request.PermissionsIds.Any(y => y == x.Id))
                .ToListAsync(context.CancellationToken);

            switch (permissions.Count)
            {
                // Проверка на наличие разрешений
                case 0:
                    _logger.LogWarning("Не найдены разрешения по заданным идентификаторам");
                    break;
                case > 0:
                    role.Permissions = permissions;
                    break;
            }
        }

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Role = _mapper.Map<Access.V1.Role>(role);

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления роли в БД";
            _logger.LogError("Ошибка обновления роли в БД");

            return response;
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления роли в БД";
            _logger.LogError(
                "Ошибка во время обновления роли в БД | {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            return response;
        }
    }

    public override async Task<DeleteRoleResponse> DeleteRole(DeleteRoleRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new DeleteRoleResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var role = await applicationContext.Roles
            .FirstOrDefaultAsync(user => user.Id == request.RoleId,
                context.CancellationToken);

        if (role is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдена роль по заданному идентификатору";
            _logger.LogError("Не найдена роль по заданному идентификатору");

            return response;
        }

        try
        {
            applicationContext.Roles.Remove(role);

            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.RoleId = request.RoleId;
                response.Response.Status = OperationStatus.Ok;
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка удаления роли из БД";
            _logger.LogError("Ошибка удаления роли из БД");

            return response;
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка удаления роли из БД";
            _logger.LogError(
                "Ошибка во время удаления роли из БД | {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            return response;
        }
    }

    public override async Task<GetPermissionsResponse> GetPermissions(GetPermissionsRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetPermissionsResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var permissions = await applicationContext
            .Permissions
            .AsNoTracking()
            .Include(x => x.Methods)
            .ToListAsync(context.CancellationToken);

        if (permissions.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены разрешения в базе данных";
            _logger.LogWarning("Не найдены разрешения в базе данных");

            return response;
        }

        // Маппинг разрешений из БД в ответ
        response.Permissions.AddRange(_mapper.Map<List<Access.V1.Permission>>(permissions));
        response.Response.Status = OperationStatus.Ok;

        return response;
    }

    public override async Task<GetPermissionResponse> GetPermission(GetPermissionRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetPermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        if (request.PermissionId is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не указан идентификатор разрешения";
            _logger.LogError("Не указан идентификатор разрешения");

            return response;
        }

        var permission = await applicationContext
            .Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PermissionId, context.CancellationToken);

        if (permission is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдено разрешение по заданному идентификатору";
            _logger.LogWarning("Не найдено разрешение по заданному идентификатору");

            return response;
        }

        // Маппинг разрешения из БД в ответ
        response.Permission = _mapper.Map<Access.V1.Permission>(permission);
        response.Response.Status = OperationStatus.Ok;

        return response;
    }

    public override async Task<AddPermissionResponse> AddPermission(AddPermissionRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddPermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка на дублирование по наименованию разрешения
        var permission = await applicationContext.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == request.Name);

        if (permission is not null)
        {
            response.Response.Status = OperationStatus.Duplicate;
            response.Response.Description = "Разрешение с таким наименованием уже существует";
            _logger.LogError("Разрешение с таким наименованием уже существует");

            return response;
        }

        // Поиск методов соответствующих идентификаторам
        var methods = await applicationContext
            .Methods
            .Where(x => request.MethodsIds.Any(y => y == x.Id))
            .ToListAsync(context.CancellationToken);

        // Проверка на наличие методов
        if (methods.Count == 0)
            _logger.LogWarning("Не найдены методы по заданным идентификаторам");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не все обязательные поля заполнены";
            _logger.LogError("Не все обязательные поля заполнены");

            return response;
        }

        var permissionCandidate = new Permission
        {
            Name = request.Name,
            Methods = methods
        };

        await applicationContext.Permissions.AddAsync(permissionCandidate, context.CancellationToken);

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Permission = _mapper.Map<Access.V1.Permission>(permissionCandidate);

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления разрешения в БД";
            _logger.LogError("Ошибка добавления разрешения в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка добавления разрешения в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления разрешения в БД";
        }

        return response;
    }

    public override async Task<UpdatePermissionResponse> UpdatePermission(UpdatePermissionRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdatePermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var permission = await applicationContext.Permissions
            .Include(x => x.Methods)
            .FirstOrDefaultAsync(x => x.Id == request.PermissionId,
                context.CancellationToken);

        if (permission is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдено разрешение по заданному идентификатору";
            _logger.LogError("Не найдено разрешение по заданному идентификатору");

            return response;
        }

        if (request.HasName && !string.IsNullOrWhiteSpace(request.Name))
            permission.Name = request.Name;

        if (request.MethodsIds.Count > 0)
        {
            // Поиск методов соответствующих идентификаторам
            var methods = await applicationContext
                .Methods
                .Where(x => request.MethodsIds.Any(y => y == x.Id))
                .ToListAsync(context.CancellationToken);

            switch (methods.Count)
            {
                // Проверка на наличие методов
                case 0:
                    _logger.LogWarning("Не найдены методы по заданным идентификаторам");
                    break;
                case > 0:
                    permission.Methods = methods;
                    break;
            }
        }

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Permission = _mapper.Map<Access.V1.Permission>(permission);

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления разрешения в БД";
            _logger.LogError("Ошибка обновления разрешения в БД");

            return response;
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления разрешения в БД";
            _logger.LogError(
                "Ошибка во время обновления разрешения в БД | {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);
        }

        return response;
    }

    public override async Task<DeletePermissionResponse> DeletePermission(DeletePermissionRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new DeletePermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var permission = await applicationContext.Permissions
            .FirstOrDefaultAsync(user => user.Id == request.PermissionId,
                context.CancellationToken);

        if (permission is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдено разрешение по заданному идентификатору";
            _logger.LogError("Не найдено разрешение по заданному идентификатору");

            return response;
        }

        try
        {
            applicationContext.Permissions.Remove(permission);

            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.PermissionId = request.PermissionId;
                response.Response.Status = OperationStatus.Ok;
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка удаления разрешения из БД";
            _logger.LogError("Ошибка удаления разрешения из БД");

            return response;
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка удаления разрешения из БД";
            _logger.LogError(
                "Ошибка во время удаления разрешения из БД | {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);
        }

        return response;
    }

    public override async Task<GetMethodsResponse> GetMethods(GetMethodsRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetMethodsResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var methods = await applicationContext
            .Methods
            .AsNoTracking()
            .ToListAsync(context.CancellationToken);

        if (methods.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены методы в базе данных";
            _logger.LogWarning("Не найдены методы в базе данных");

            return response;
        }

        // Маппинг методов из БД в ответ
        response.Methods.AddRange(_mapper.Map<List<Access.V1.Method>>(methods));
        response.Response.Status = OperationStatus.Ok;

        return response;
    }

    public override async Task<AddMethodResponse> AddMethod(AddMethodRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddMethodResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка на дублирование по наименованию методоа
        var method = await applicationContext.Methods
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == request.Name);

        if (method is not null)
        {
            response.Response.Status = OperationStatus.Duplicate;
            response.Response.Description = "Метод с таким наименованием уже существует";
            _logger.LogError("Метод с таким наименованием уже существует");

            return response;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не все обязательные поля заполнены";
            _logger.LogError("Не все обязательные поля заполнены");

            return response;
        }

        var methodCandidate = new Method
        {
            Name = request.Name
        };

        await applicationContext.Methods.AddAsync(methodCandidate, context.CancellationToken);

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Method = _mapper.Map<Access.V1.Method>(methodCandidate);

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления метода в БД";
            _logger.LogError("Ошибка добавления метода в БД");
        }
        catch (Exception e)
        {
            _logger.LogError("Ошибка добавления метода в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления метода в БД";
        }

        return response;
    }

    public override async Task<UpdateMethodResponse> UpdateMethod(UpdateMethodRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdateMethodResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var method = await applicationContext.Methods
            .FirstOrDefaultAsync(x => x.Id == request.MethodId,
                context.CancellationToken);

        if (method is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден метод по заданному идентификатору";
            _logger.LogError("Не найден метод по заданному идентификатору");

            return response;
        }

        var uniqMethod = await applicationContext.Methods
            .FirstOrDefaultAsync(x => x.Name == request.Name,
                context.CancellationToken);

        if (uniqMethod is not null)
        {
            response.Response.Status = OperationStatus.Duplicate;
            response.Response.Description = "Метод с таким наименованием уже существует";
            _logger.LogError("Метод с таким наименованием уже существует");

            return response;
        }

        if (request.HasName && !string.IsNullOrWhiteSpace(request.Name))
            method.Name = request.Name;

        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.Response.Status = OperationStatus.Ok;
                response.Method = _mapper.Map<Access.V1.Method>(method);

                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления метода в БД";
            _logger.LogError("Ошибка обновления метода в БД");

            return response;
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления метода в БД";
            _logger.LogError(
                "Ошибка во время обновления метода в БД | {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);
        }

        return response;
    }

    public override async Task<DeleteMethodResponse> DeleteMethod(DeleteMethodRequest request,
        ServerCallContext context)
    {
        // Дефолтный объект
        var response = new DeleteMethodResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var method = await applicationContext.Methods
            .FirstOrDefaultAsync(user => user.Id == request.MethodId,
                context.CancellationToken);

        if (method is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден метод по заданному идентификатору";
            _logger.LogError("Не найден метод по заданному идентификатору");

            return response;
        }

        try
        {
            applicationContext.Methods.Remove(method);

            if (await applicationContext.SaveChangesAsync(context.CancellationToken) > 0)
            {
                response.MethodId = request.MethodId;
                response.Response.Status = OperationStatus.Ok;
                return response;
            }

            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка удаления метода из БД";
            _logger.LogError("Ошибка удаления метода из БД");

            return response;
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка удаления разрешения из БД";
            _logger.LogError(
                "Ошибка во время удаления разрешения из БД | {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);
        }

        return response;
    }
}