using ApiTips.Api.Enums;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal.Enums;

namespace ApiTips.Api.Services;

public class CryptoRequisites : IRequisites
{
    public string Address { get; set; }

    public string Wallet { get; set; }

    public string Token { get; set; }
        
    public string Crypto { get; set; }



    public Task<Dictionary<string, string?>> GetRequisites()
    {
        var result = new Dictionary<string, string?>();
        result.TryAdd("Address", Address);
        result.TryAdd("Wallet", Wallet);
        result.TryAdd("Token", Token);
        result.TryAdd("Crypto", Crypto);
        
        return Task.FromResult(result);
    }

    public PaymentTypeEnum PaymentType { get; set; } = PaymentTypeEnum.Cryptocurrency;
    public bool IsBanned { get; set; } = false;
}