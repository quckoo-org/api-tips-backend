using AutoMapper;
using Google.Protobuf.WellKnownTypes;


namespace ApiTips.Api.MapperProfiles.Order;

public class OrderResolverProfile : Profile
{
    public class CreateDateTimeDtResolver : IValueResolver<Dal.schemas.data.Order, Api.Order.V1.Order, Timestamp>
    {
        public Timestamp? Resolve(Dal.schemas.data.Order source, Api.Order.V1.Order destination, Timestamp destMember, ResolutionContext context)
        {
            return source.CreateDateTime.ToTimestamp();
        }
    }

    public class PaymentDateTimeDtResolver : IValueResolver<Dal.schemas.data.Order, Api.Order.V1.Order, Timestamp>
    {
        public Timestamp? Resolve(Dal.schemas.data.Order source, Api.Order.V1.Order destination, Timestamp destMember, ResolutionContext context)
        {
            return source.PaymentDateTime?.ToTimestamp();
        }
    }
}
