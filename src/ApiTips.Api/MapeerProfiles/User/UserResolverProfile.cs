using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace ApiTips.Api.MapeerProfiles.User;

public class UserResolverProfile : Profile
{
        public class DtResolver : IValueResolver<Dal.schemas.system.User, Access.V1.User, Timestamp>
        {
            public Timestamp? Resolve(Dal.schemas.system.User source, Access.V1.User destination, Timestamp destMember, ResolutionContext context)
            {
                return source.LockDateTime?.ToTimestamp(); 
            }
        }
}