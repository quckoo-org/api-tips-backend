using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.Invoices.V1;
using ProtoEnums = ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using DbInvoice = ApiTips.Dal.schemas.data.Invoice;
using InvoiceProto = ApiTips.Api.Invoices.V1;


namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsInvoiceService : Invoices.V1.ApiTipsInvoicesService.ApiTipsInvoicesServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsInvoiceService> _logger;

    // /// <summary>
    // ///     Маппер
    // /// </summary>
    //private readonly IMapper _mapper;
    
    /// <summary>
    ///     Зарегистрированные сервисы
    /// </summary>
    private IServiceProvider Services { get; }
    
    public ApiTipsInvoiceService(IHostEnvironment env, ILogger<ApiTipsInvoiceService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;

    }

    /// <summary>
    ///     Создание нового счёта
    /// </summary>
    public override async Task<CreateInvoiceResponse> CreateInvoice(CreateInvoiceRequest request, ServerCallContext context)
    {
        // Создание ответа по умолчанию
        var response = new CreateInvoiceResponse
        {
            Response = new GeneralResponse
            {
                Status = ProtoEnums.OperationStatus.Unspecified
            }
        };
        
        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        if (request.HasEmail)
        {
            var user = await applicationContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (user == null)
            {
                response.Response.Status = ProtoEnums.OperationStatus.NoData;
                response.Response.Description = $"Не найден пользователь с почтой {request.Email}";
                return response;
            }

            var order = await applicationContext.Orders.FirstOrDefaultAsync(x => x.Id == request.OrderId, context.CancellationToken);
            if (order is null)
            {
                response.Response.Status = ProtoEnums.OperationStatus.NoData;
                response.Response.Description = $"Не найден пользователь с почтой {request.Email}";
                return response;
            }
            var currency = CreateCurrency(request.TotalAmount.FromDecimal(), request.PaymentType);
            if (currency is null)
            {
                response.Response.Status = ProtoEnums.OperationStatus.NoData;
                response.Response.Description = $"Не найден пользователь с почтой {request.Email}";
                return response;
            }
            var newInvoice = new DbInvoice
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Alias = await CreateAlias(applicationContext),
                AmountOfRequests = request.AmountOfRequests ?? 0,
                Description = request.Description,
                Payer = user,
                Order = order,
                CurrentCurrency = currency
            };
        }
        
        
        return response;
    }

    private DbInvoice.Currency? CreateCurrency(decimal amount, ProtoEnums.PaymentType paymentType)
    {
        DbInvoice.Currency? result = null;
        switch (paymentType)
        {
            // Проверка на наличие методов
            case ProtoEnums.PaymentType.Bank:
                result = new DbInvoice.Currency
                {
                    TotalAmount = amount,
                    Type = Dal.Enums.PaymentType.Bank,
                    CurrencyType = "USD"
                    
                };
                break;
            case ProtoEnums.PaymentType.Crypto:
                result = new DbInvoice.Currency
                {
                    TotalAmount = amount,
                    Type = Dal.Enums.PaymentType.Bank,
                    CurrencyType = "USD"
                    
                };
                break;
            case ProtoEnums.PaymentType.Unspecified:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(paymentType), paymentType, null);
        }

        return result;
    }
    private async Task<string> CreateAlias(ApplicationContext context)
    {
        // Сделай мне алиас по правилу текущий год и текущий месяц в формате строки
        var firstPart = $"{DateTime.Now.Year}{DateTime.Now.Month}";
        var dateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 0);
        var dateTo = dateFrom.AddMonths(1);
        var lastAlias = await context
            .Invoices
            .AsNoTracking()
            .Where(x => x.CreatedAt > dateFrom && x.CreatedAt < dateTo)
            .OrderBy(x => x.CreatedAt)
            .LastOrDefaultAsync();
        int counterAliases = 1;
        if (lastAlias is null)
            return firstPart + counterAliases.ToString("D3");
        
        
        var lastAliasString = lastAlias.Alias[^3..];
        var number = int.Parse(lastAliasString);

        return $"{firstPart}{number:D3}";
    }
} 