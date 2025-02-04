using ApiTips.Api.Services;

namespace ApiTips.Api.Extensions.Application;

public static class InjectionPayments
{
    public  static WebApplicationBuilder ConfigAndAddPaymentRequisites(this WebApplicationBuilder builder)
    {

        builder.Services.Configure<BankRequisites>(builder.Configuration.GetSection("BankRequisites"));
        builder.Services.Configure<CryptoRequisites>(builder.Configuration.GetSection("CryptoRequisites"));
        
        return builder;
    }
}