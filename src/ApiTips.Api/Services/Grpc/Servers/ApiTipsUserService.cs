using ApiTips.Api.User.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.GeneralEntities.V1;
using Grpc.Core;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsUserService(ILogger<ApiTipsUserService> logger,IServiceProvider services) : User.V1.ApiTipsUserService.ApiTipsUserServiceBase
{
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

    public async override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        // Дефолтный объект
        var response = new CreateUserResponse
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
}