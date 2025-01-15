using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace ApiTips.Api.MapeerProfiles.Tariff;

public class TariffResolverProfile : Profile
{
    public class StartDateTimeDtResolver : IValueResolver<Dal.schemas.data.Tariff, Api.Tariff.V1.Tariff, Timestamp>
    {
        public Timestamp? Resolve(Dal.schemas.data.Tariff source, Api.Tariff.V1.Tariff destination, Timestamp destMember, ResolutionContext context)
        {
            return source.StartDateTime.ToTimestamp();
        }
    }

    public class EndDateTimeDtResolver : IValueResolver<Dal.schemas.data.Tariff, Api.Tariff.V1.Tariff, Timestamp>
    {
        public Timestamp? Resolve(Dal.schemas.data.Tariff source, Api.Tariff.V1.Tariff destination, Timestamp destMember, ResolutionContext context)
        {
            return source.EndDateTime?.ToTimestamp();
        }
    }

    public class HideDateTimeDtResolver : IValueResolver<Dal.schemas.data.Tariff, Api.Tariff.V1.Tariff, Timestamp>
    {
        public Timestamp? Resolve(Dal.schemas.data.Tariff source, Api.Tariff.V1.Tariff destination, Timestamp destMember, ResolutionContext context)
        {
            return source.HideDateTime?.ToTimestamp();
        }
    }
}
