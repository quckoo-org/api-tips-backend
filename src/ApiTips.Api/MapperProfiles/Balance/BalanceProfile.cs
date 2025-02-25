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

        CreateMap<Dal.schemas.data.BalanceHistory, Api.Balance.V1.Operation>()
         .ForMember(dst => dst.Id, opt =>
            opt.MapFrom(src => src.Id))
         .ForMember(dst => dst.CreditedFreeTipsCount, opt =>
         {
             opt.PreCondition(src => src.FreeTipsCountChangedTo is not null 
                                     && src.OperationType == DalBalanceOperationType.Crediting);
             opt.MapFrom(src => src.FreeTipsCountChangedTo);
         })
         .ForMember(dst => dst.CreditedPaidTipsCount, opt =>
         {
             opt.PreCondition(src => src.PaidTipsCountChangedTo is not null 
                                     && src.OperationType == DalBalanceOperationType.Crediting);
             opt.MapFrom(src => src.PaidTipsCountChangedTo);
         })
         .ForMember(dst => dst.DebitedTipsCount, opt =>
         {
             opt.PreCondition(src => src.TotalTipsCountChangedTo is not null 
                                     && src.OperationType == DalBalanceOperationType.Debiting);
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
    }
}
