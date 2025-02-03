using AutoMapper;
using DalBalanceOperationType = ApiTips.Dal.Enums.BalanceOperationType;
using ProtoBalanceOperationType = ApiTips.CustomEnums.V1.BalanceOperationType;

namespace ApiTips.Api.MapperProfiles.Balance;

public class BalanceProfile : Profile
{
    public BalanceProfile()
    {
        CreateMap<DalBalanceOperationType, ProtoBalanceOperationType>().ConvertUsing((value, dest) =>
        {
            return value switch
            {
                DalBalanceOperationType.Crediting => ProtoBalanceOperationType.Crediting,
                DalBalanceOperationType.Debiting => ProtoBalanceOperationType.Debiting,
                _ => ProtoBalanceOperationType.Unspecified
            };
        });

        CreateMap<Dal.schemas.data.BalanceHistory, Api.Balance.V1.DetailedHistory>()
         .ForMember(dst => dst.Id, opt =>
            opt.MapFrom(src => src.Id))
         .ForMember(dst => dst.FreeTipsCountChangedTo, opt =>
         {
             opt.PreCondition(src => src.FreeTipsCountChangedTo is not null);
             opt.MapFrom(src => src.FreeTipsCountChangedTo);
         })
         .ForMember(dst => dst.PaidTipsCountChangedTo, opt =>
         {
             opt.PreCondition(src => src.PaidTipsCountChangedTo is not null);
             opt.MapFrom(src => src.PaidTipsCountChangedTo);
         })
         .ForMember(dst => dst.TotalTipsCountChangedTo, opt =>
         {
             opt.PreCondition(src => src.TotalTipsCountChangedTo is not null);
             opt.MapFrom(src => src.TotalTipsCountChangedTo);
         })
         .ForMember(dst => dst.OperationType, opt =>
            opt.MapFrom(src => src.OperationType))
         .ForMember(dst => dst.OperationDate, opt =>
            opt.MapFrom<BalanceResolverProfile.OperationDateTimeDtResolver>())
         .ForMember(dst => dst.Reason, opt =>
            opt.MapFrom(src => src.ReasonDescription))
         .ForMember(dst => dst.TotalTipsBalance, opt =>
            opt.MapFrom(src => src.TotalTipsBalance))
         ;

        CreateMap<Dal.schemas.system.User, Api.Balance.V1.User>()
         .ForMember(dst => dst.Id, opt =>
            opt.MapFrom(src => src.Id))
         .ForMember(dst => dst.Email, opt =>
            opt.MapFrom(src => src.Email))
         .ForMember(dst => dst.FirstName, opt =>
            opt.MapFrom(src => src.FirstName))
         .ForMember(dst => dst.LastName, opt =>
            opt.MapFrom(src => src.LastName))
         ;
    }
}
