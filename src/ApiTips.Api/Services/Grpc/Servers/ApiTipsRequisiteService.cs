using ApiTips.Api.MapperProfiles.Payment;
using ApiTips.Api.Requisites.V1;
using ApiTips.CustomEnums.V1;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsRequisiteService
    : ApiTipsRequisitesService.ApiTipsRequisitesServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsRequisiteService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    ///     Зарегистрированные сервисы
    /// </summary>
    private IServiceProvider Services { get; }

    private readonly BankRequisites _bankRequisites;
    private readonly CryptoRequisites _cryptoWallet;

    public ApiTipsRequisiteService(IHostEnvironment env,
        ILogger<ApiTipsRequisiteService> logger, IServiceProvider services, IOptions<BankRequisites> br, IOptions<CryptoRequisites> cr)
    {
        _logger = logger;
        Services = services;
        
        _bankRequisites = br.Value;
        _cryptoWallet = cr.Value;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(PaymentProfile));
        });
        
        if (env.IsDevelopment())
        {
            config.CompileMappings();
            config.AssertConfigurationIsValid();
        }

        _mapper = new Mapper(config);
    }

    public override async Task<GetAllRequisitesResponse> GetAllRequisites (GetAllRequisitesRequest request, ServerCallContext context)
    {
        // Создание базового ответа
        var response = new GetAllRequisitesResponse
        {
            Response = new GeneralResponse
            {
                Status = OperationStatus.Unspecified
            },
        };

        try
        {
            response.CryptoWallet = _mapper.Map<CryptoWallet>(_cryptoWallet);
            response.BankAccount = _mapper.Map<BankAccount>(_bankRequisites);
        }
        catch (Exception e)
        {
            _logger.LogError("Возникла ошибка при маппинге сущностей: {exceptionMessage} | InnerException: {InnerMessage}"
                ,e.Message, e.InnerException?.Message);
            response.Response.Status = OperationStatus.Error;
            response.Response.Description = "An unexpected error occurred while retrieving payment requisites.";
        }

        response.Response.Status = OperationStatus.Ok;
        return response;
    }
}