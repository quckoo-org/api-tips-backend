using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.Invoice.V1;
using ApiTips.Api.MapperProfiles.Invoice;
using ProtoEnums = ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using DbInvoice = ApiTips.Dal.schemas.data.Invoice;
using InvoiceProto = ApiTips.Api.Invoice.V1;


namespace ApiTips.Api.Services.Grpc.Servers;

public class ApiTipsInvoiceService : InvoiceProto.ApiTipsInvoiceService.ApiTipsInvoiceServiceBase
{
    /// <summary>
    ///     Логгер
    /// </summary>
    private readonly ILogger<ApiTipsInvoiceService> _logger;

    /// <summary>
    ///     Маппер
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    ///     Зарегистрированные сервисы
    /// </summary>
    private IServiceProvider Services { get; }

    public ApiTipsInvoiceService(IHostEnvironment env, ILogger<ApiTipsInvoiceService> logger, IServiceProvider services)
    {
        _logger = logger;
        Services = services;
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(typeof(InvoiceProfile));
        });

        if (env.IsDevelopment())
        {
            config.CompileMappings();
            config.AssertConfigurationIsValid();
        }

        _mapper = new Mapper(config);
    }

    public override async Task<GetInvoicesResponse> GetInvoices(GetInvoicesRequest request, ServerCallContext context)
    {
        // Создание ответа по умолчанию
        var response = new GetInvoicesResponse
        {
            Response = new GeneralResponse
            {
                Status = ProtoEnums.OperationStatus.Unspecified
            },
            Invoices = { Capacity = 0 }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var invoices = await applicationContext
            .Invoices
            .AsNoTracking()
            .Include(i => i.Order)
            .ThenInclude(o => o.User)
            .ToListAsync(context.CancellationToken);
        var res = _mapper.Map<List<InvoiceProto.Invoice>>(invoices);
        response.Invoices.AddRange(res);
        response.Response.Status = ProtoEnums.OperationStatus.Ok;

        return response;
    }

    /// <summary>
    ///     Создание нового счёта
    /// </summary>
    public override async Task<CreateInvoiceResponse> CreateInvoice(CreateInvoiceRequest request,
        ServerCallContext context)
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


        var order = await applicationContext
            .Orders
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = $"Order with {request.OrderId} doesn't exist";
            return response;
        }

        var currency = CreateCurrency(request.TotalAmount.FromDecimal(), request.PaymentType);
        if (currency is null)
        {
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = "Unable to create invoice";
            _logger.LogError("Не удалось сформировать алиас для счёта к заказу {OrderId}", request.OrderId);
            return response;
        }


        var newInvoice = new DbInvoice
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Alias = await CreateAlias(applicationContext),
            AmountOfRequests = request.AmountOfRequests ?? 0,
            Description = request.Description,
            Order = order,
            CurrentCurrency = currency,
        };
        try
        {
            newInvoice.PayedAt = order.PaymentDateTime;
            var addedInvoice = applicationContext.Invoices.Add(newInvoice);
            await applicationContext.SaveChangesAsync(context.CancellationToken);
            var resd = _mapper.Map<InvoiceProto.Invoice>(addedInvoice.Entity);
            response.Invoice = resd;
            response.Response.Status = ProtoEnums.OperationStatus.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Не удалось создать счёт к заказу {OrderId}: Exception: {Message} | InnerException: {InnerMessage}",
                request.OrderId, ex.Message, ex.InnerException?.Message);
            response.Response.Status = ProtoEnums.OperationStatus.Error;
            response.Response.Description = "An error occurred while creating the invoice";
        }

        return response;
    }

    public override async Task<UpdateInvoiceResponse> UpdateInvoice(UpdateInvoiceRequest request,
        ServerCallContext context)
    {
        var response = new UpdateInvoiceResponse
        {
            Response = new GeneralResponse
            {
                Status = ProtoEnums.OperationStatus.Ok
            }
        };

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        if (!Guid.TryParse(request.InvoiceId, out var guid))
        {
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = "Invalid invoice id";

            _logger.LogWarning("Не удалось преобразовать идентификатор счёта {InvoiceId} в GUID", request.InvoiceId);

            return response;
        }

        var invoice = await applicationContext
            .Invoices
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == guid, context.CancellationToken);
        if (invoice is null)
        {
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = "Invoice not found";

            _logger.LogWarning("Счёт с идентификатором {InvoiceId} не найден", request.InvoiceId);

            return response;
        }

        if (request.HasRefNumber)
            invoice.RefNumber = request.RefNumber;
        if (request.AmountOfRequests.HasValue)
            invoice.AmountOfRequests = request.AmountOfRequests.Value;

        await applicationContext.SaveChangesAsync(context.CancellationToken);

        response.Invoice = _mapper.Map<InvoiceProto.Invoice>(invoice);
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
        var firstPart = $"{DateTime.Now.Year:D4}{DateTime.Now.Month:D2}";
        var dateFrom = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var dateTo = dateFrom.AddMonths(1);
        dateFrom = DateTime.SpecifyKind(dateFrom, DateTimeKind.Utc);
        dateTo = DateTime.SpecifyKind(dateTo, DateTimeKind.Utc);
        var lastAlias = await context
            .Invoices
            .AsNoTracking()
            .Where(x => x.CreatedAt > dateFrom && x.CreatedAt < dateTo)
            .OrderBy(x => x.CreatedAt)
            .LastOrDefaultAsync();
        int counterAliases = 1;
        var test = counterAliases.ToString("D3");
        
        if (lastAlias is null)
            return firstPart + counterAliases.ToString("D3");


        var lastAliasString = lastAlias.Alias[^3..];
        var number = int.Parse(lastAliasString);

        return $"{firstPart}{number:D3}";
    }
}