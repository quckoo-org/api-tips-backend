using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace ApiTips.Api.MapperProfiles.Balance;

public class BalanceResolverProfile : Profile
{
    public class OperationDateTimeDtResolver : IValueResolver<Dal.schemas.data.BalanceHistory, Api.Balance.V1.BalanceHistory, Timestamp>
    {
        public Timestamp? Resolve(Dal.schemas.data.BalanceHistory source, Api.Balance.V1.BalanceHistory destination, Timestamp destMember, ResolutionContext context)
        {
            return source.OperationDateTime.ToTimestamp();
        }
    }
}