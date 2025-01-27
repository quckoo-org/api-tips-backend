using ApiTips.Api.Enums;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal.Enums;

namespace ApiTips.Api.Services;

public class BankRequisites : IRequisites
{
    public string? BankName { get; set; }

    public string? BankAddress { get; set; }

    public string? Swift { get; set; }

    public string? AccountNumber { get; set; }

    public string? Iban { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? CurrencyType { get; set; }

    public Task<Dictionary<string, string?>> GetRequisites()
    {
        var result = new Dictionary<string, string?>();

        result.TryAdd("BankName", BankName);
        result.TryAdd("BankAddress", BankAddress);
        result.TryAdd("Swift", Swift);
        result.TryAdd("AccountNumber", AccountNumber);
        result.TryAdd("Iban", Iban);
        result.TryAdd("AdditionalInfo", AdditionalInfo);
        result.TryAdd("CurrencyType", CurrencyType);

        return Task.FromResult(result);
    }

    public PaymentTypeEnum PaymentType { get; set; } = PaymentTypeEnum.BankTransfer;
    public bool IsBanned { get; set; }
}