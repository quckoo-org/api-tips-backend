using ApiTips.Api.Enums;
using ApiTips.Dal.Enums;

namespace ApiTips.Api.ServiceInterfaces;

public interface IRequisites
{
    Task<Dictionary<string, string?>> GetRequisites();
    PaymentTypeEnum PaymentType { get; set; }
    public bool IsBanned { get; set; }
}