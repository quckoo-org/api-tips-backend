using ApiTips.Api.Payment.V1;
using Grpc.Core;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsPaymentService(IHostEnvironment env, ILogger<ApiTipsTariffService> logger, IServiceProvider services) 
    : Payment.V1.ApiTipsPaymentService.ApiTipsPaymentServiceBase
{
    public override Task<UpdatePaymentResponse> UpdatePayment(UpdatePaymentRequest request, ServerCallContext context)
    {
        
        
        return base.UpdatePayment(request, context);
    }
}