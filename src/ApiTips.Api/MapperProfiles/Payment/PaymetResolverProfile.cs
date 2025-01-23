using ApiTips.Dal.Enums;
using AutoMapper;

namespace ApiTips.Api.MapperProfiles.Payment;

public class PaymetResolverProfile : Profile
{
    public class PaymentTypeResolver : IValueResolver<Api.Payment.V1.Payment,Dal.schemas.data.Payment, 
        PaymentType>
    {
        public PaymentType Resolve(Api.Payment.V1.Payment source, Dal.schemas.data.Payment destination,
            PaymentType destMember, ResolutionContext context)
        {
            var res = PaymentType.Unspecified;
            switch (source.PaymentTypeCase)
            {
                case Api.Payment.V1.Payment.PaymentTypeOneofCase.BankAccount:
                {
                    res = PaymentType.Money;
                    break;
                }
                case Api.Payment.V1.Payment.PaymentTypeOneofCase.CryptoWallet:
                {
                    res = PaymentType.Crypto;
                    break;
                } 
            }
            return res;
        }
    }
}