using ApiTips.Api.Access.V1;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsAccessService(ILogger<ApiTipsAccessService> logger,IServiceProvider services, IJwtService jwt) : Access.V1.ApiTipsAccessService.ApiTipsAccessServiceBase
{
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
        await using var scope = services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();


        var users = applicationContext.Users.AsNoTracking();
        
        // Фильтрация по Email пользователя
        if (!string.IsNullOrWhiteSpace(request.Filter.Email))
            users = users.Where(user => user.Email == request.Filter.Email);
        
        // Получение только заблокированных пользователей
        if (request.Filter.IsBlocked == true)
            users = users.Where(user => user.LockDateTime < DateTime.UtcNow);
        
        // Получение только верифицированных пользователей
        if (request.Filter.IsVerified == true)
            users = users.Where(user => user.VerifyDateTime != null);
        
        // Получение только удаленных пользователей
        if (request.Filter.IsDeleted == true)
            users = users.Where(user => user.DeleteDateTime != null);
        
        // Отправка запроса после фильтрации в Базу данных
        var result = await users.ToListAsync(context.CancellationToken);
        
        if (result.Count == 0)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены пользователи по заданным фильтрам";
            return response;
        }
        
        // Маппинг пользователей из БД в ответ
        // TODO перенести в AutoMapper
        response.Users.AddRange(result.Select(user => new User
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Cca3= user.Cca3,
            CreatedAt = user.CreateDateTime.ToTimestamp(),
            BlockedAt = user.LockDateTime?.ToTimestamp(),
            DeletedAt = user.DeleteDateTime?.ToTimestamp(),
            VerifiedAt = user.VerifyDateTime?.ToTimestamp()
        }));

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
        await using var scope = services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        var user = await applicationContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == request.UserId,
                context.CancellationToken);

        if (user is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найден пользователь по заданному ID";
            
            return response;
        }

        // Маппинг пользователя из БД в ответ
        // TODO перенести в AutoMapper
        response.User = new User
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Cca3= user.Cca3,
            CreatedAt = user.CreateDateTime.ToTimestamp(),
            BlockedAt = user.LockDateTime?.ToTimestamp(),
            DeletedAt = user.DeleteDateTime?.ToTimestamp(),
            VerifiedAt = user.VerifyDateTime?.ToTimestamp()
        };
        
        return response;
    }
    
    public async override Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
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
        await using var scope = services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        // Проверка на дублирование по почте пользователя
        var existingUser = await applicationContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email);
        if (existingUser is not null)
        {
            response.Response.Status = OperationStatus.Duplicate;
            response.Response.Description = "Пользователь с таким Email уже существует";
            
            return response;
        }
        
        var roles = await applicationContext.Roles
            .Where(role => request.RolesIds.Contains(role.Id))
            .ToListAsync(context.CancellationToken);
        if (roles.Count == 0)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Не найдены роли по заданным идентификаторам";
            
            return response;
        }
        
        var userToAdd = new ApiTips.Dal.schemas.system.User
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Cca3 = request.Cca3,
            Roles = roles
        };
        
        await applicationContext.Users.AddAsync(userToAdd, context.CancellationToken);
        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) == 0)
            {
                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Ошибка добавления пользователя в БД";
                
                return response;
            }
        }
        catch (Exception e)
        {
            logger.LogError("Ошибка добавления пользователя в БД: {Message} | InnerException: {InnerMessage}",
                e.Message, e.InnerException?.Message);
            
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка добавления пользователя в БД";
        }

        response.Response.Status = OperationStatus.Ok;
            
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
        await using var scope = services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        var user = await applicationContext.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.Id == request.UserId,
                context.CancellationToken);

        if (user is null)
        {
            response.Response.Description = "Не найден пользователь по заданному идентификатору";
            response.Response.Status = OperationStatus.NoData;
            return response;
        }

        if (request.HasEmail)
            user.Email = request.Email;
        if (request.HasFirstName)
            user.FirstName = request.FirstName;
        if (request.HasLastName)
            user.LastName = request.LastName;
        if (request.HasCca3)
            user.Cca3 = request.Cca3;
        if (request.HasIsVerified)
            user.VerifyDateTime = request.IsVerified ? DateTime.UtcNow : null;
        if (request.HasIsBlocked)
            user.LockDateTime = request.IsBlocked ? DateTime.UtcNow : null;
        if (request.HasIsDeleted)
            user.DeleteDateTime = request.IsDeleted ? DateTime.UtcNow : null;
        if (request.RolesIds.Count > 0)
           user.Roles = await GetRolesAsync(applicationContext, request.RolesIds.ToList(), context.CancellationToken);


        try
        {
            if (await applicationContext.SaveChangesAsync(context.CancellationToken) == 0)
            {
                response.Response.Status = OperationStatus.Error;
                response.Response.Description = "Ошибка обновления пользователя в БД";
                logger.LogError("Ошибка обновления пользователя в БД");
                
                return response;
            }
        }
        catch (Exception e)
        {
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "Ошибка обновления пользователя в БД";
            logger.LogError("Ошибка во время обновления пользователя в БД");
                
            return response;
        }

        response.Response.Status = OperationStatus.Ok;
        
        return response;
    }
    
    private async Task<List<ApiTips.Dal.schemas.system.Role>> GetRolesAsync(ApplicationContext context, 
        List<long> ids, CancellationToken token)
    {
        return await context.Roles
            .Where(role => ids.Contains(role.Id))
            .ToListAsync(token);
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
        await using var scope = services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        var roles = await applicationContext
            .Roles
            .AsNoTracking()
            .Include(x => x.Permissions)
            .ThenInclude(x => x.Methods)
            .ToListAsync();

        if (roles.Count == 0)
        {
            logger.LogWarning("Не найдены роли в базе данных");
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдены роли в базе данных";

            return response;
        }
        
        // Маппинг ролей из БД в ответ
        // TODO перенести в AutoMapper
        response.Roles.AddRange(roles.Select(role => new Role
        {
            Id = role.Id,
            Name = role.Name,
        }));

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
        
        // code
        // Получение контекста базы данных из сервисов коллекций
        await using var scope = services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        if (request.RoleId is null)
        {
            response.Response.Status = OperationStatus.Declined;
            response.Response.Description = "Не указан идентификатор роли";

            return response;
        }
        
        var role = await applicationContext.Roles
            .FirstOrDefaultAsync(x => x.Id == request.RoleId, context.CancellationToken);

        if (role is null)
        {
            response.Response.Status = OperationStatus.NoData;
            response.Response.Description = "Не найдена роль по заданному идентификатору";

            return response;
        }
        
        // Маппинг роли из БД в ответ
        // TODO перенести в AutoMapper
        response.Role = new Role
        {
            Id = role.Id,
            Name = role.Name,
        };
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
        
        // code
        
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
        
        // code
        
        return response;
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
        
        // code
        
        return response;
    }
    
    public override async Task<GetPermissionsResponse> GetPermissions(GetPermissionsRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetPermissionsResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
    }
    
    public override async Task<GetPermissionResponse> GetPermission(GetPermissionRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetPermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
    }
    
    public override async Task<AddPermissionResponse> AddPermission(AddPermissionRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new AddPermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
    }
    
    public override async Task<UpdatePermissionResponse> UpdatePermission(UpdatePermissionRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdatePermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
    }
    
    public override async Task<DeletePermissionResponse> DeletePermission(DeletePermissionRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new DeletePermissionResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
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
        
        // code
        
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
        
        // code
        
        return response;
    }
    
    public override async Task<UpdateMethodResponse> UpdateMethod(UpdateMethodRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdateMethodResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
    }
    
    public override async Task<DeleteMethodResponse> DeleteMethod(DeleteMethodRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new DeleteMethodResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
    }
}