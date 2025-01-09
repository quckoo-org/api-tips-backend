using ApiTips.Api.Access.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.GeneralEntities.V1;
using Grpc.Core;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsAccessService(ILogger<ApiTipsAccessService> logger,IServiceProvider services) : Access.V1.ApiTipsAccessService.ApiTipsAccessServiceBase
{
    public async override Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetUsersResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
    }
    
    public async override Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new GetUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
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
        
        // code
        
        return response;
    }

    public async override Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new UpdateUserResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            }
        };
        
        // code
        
        return response;
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
        
        // code
        
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