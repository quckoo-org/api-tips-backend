using ApiTips.Api.Extensions.Grpc;
using ApiTips.Api.Invoice.V1;
using ApiTips.Api.MapperProfiles.Invoice;
using ProtoEnums = ApiTips.CustomEnums.V1;
using ApiTips.Dal;
using ApiTips.GeneralEntities.V1;
using AutoMapper;
using Google.Protobuf;
using Grpc.Core;
using iTextSharp.text;
using iTextSharp.text.pdf;
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

        // Получение счёта вместе с заказом и пользователем, оформившим заказ
        var invoices = await applicationContext
            .Invoices
            .AsNoTracking()
            .Include(i => i.Order)
            .ThenInclude(o => o.User)
            .ToListAsync(context.CancellationToken);

        // Попытка преобразовать счёт из базы данных в счёт gRPC
        var res = _mapper.Map<List<InvoiceProto.Invoice>>(invoices);

        //Добавление в ответ список массив
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


        // Получение Заказа из базы данных
        var order = await applicationContext
            .Orders
            .Include(x => x.User)
            .Include(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId,
                context.CancellationToken);

        // Если заказ не найден в базе данных, то возвращается NoData
        if (order is null)
        {
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = $"Order with {request.OrderId} doesn't exist";
            return response;
        }

        // Какую библиотеку использовать для создания PDF-файлов

        // Если заказ уже создан для сверки, то новый создать нельзя
        if (order.Invoice is not null)
        {
            response.Response.Status = ProtoEnums.OperationStatus.Error;
            response.Response.Description = $"Invoice for order with id: {request.OrderId} already exists";
            _logger.LogWarning("Для заказа с идентификатором {OrderId} уже существует счёт", request.OrderId);
            return response;
        }

        // Создание валюты JsonB формата
        var currency = CreateCurrency(request.TotalAmount.FromDecimal(), request.PaymentType);
        if (currency is null)
        {
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = "Unable to create invoice";
            _logger.LogError("Не удалось сформировать валюту для счёта к заказу {OrderId}", request.OrderId);
            return response;
        }

        // Создание счёта для записи в базу данных
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

        // Попытка сохранить сущность в базу данных и смаппить сохранённую сущность в gRPC-ответ 
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

    /// <summary>
    ///     Обновление счёта
    /// </summary>
    public override async Task<UpdateInvoiceResponse> UpdateInvoice(UpdateInvoiceRequest request,
        ServerCallContext context)
    {
        // Формирование ответа
        var response = new UpdateInvoiceResponse
        {
            Response = new GeneralResponse
            {
                Status = ProtoEnums.OperationStatus.Unspecified
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

        // Если в запросе заполнен RefNumber, то он изменяется в счёте
        if (request.HasRefNumber)
            invoice.RefNumber = request.RefNumber;
        // Если в запросе заполнено количество запросов, то значение изменяется в счёте
        if (request.AmountOfRequests.HasValue)
            invoice.AmountOfRequests = request.AmountOfRequests.Value;

        // Попытка сохранить изменённый счет в базу данных
        await applicationContext.SaveChangesAsync(context.CancellationToken);

        // Маппинг сущности счёта в ответ для gRPC
        response.Invoice = _mapper.Map<InvoiceProto.Invoice>(invoice);

        response.Response.Status = ProtoEnums.OperationStatus.Ok;

        return response;
    }

    public override async Task<GeneratePdfForInvoiceResponse> GeneratePdfForInvoice(
        GeneratePdfForInvoiceRequest request, ServerCallContext context)
    {
        // Формирование ответа
        var response = new GeneratePdfForInvoiceResponse
        {
            Response = new GeneralResponse
            {
                Status = ProtoEnums.OperationStatus.Unspecified
            }
        };


        if (!Guid.TryParse(request.InvoiceId, out var guid))
        {
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = "Invalid invoice id";

            _logger.LogWarning("Не удалось преобразовать идентификатор счёта {InvoiceId} в GUID", request.InvoiceId);

            return response;
        }

        // Получение контекста базы данных из сервисов коллекций
        await using var scope = Services.CreateAsyncScope();
        await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var invoice = await applicationContext
            .Invoices
            .Include(x => x.Order)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == guid, context.CancellationToken);

        if (invoice is null)
        {
            _logger.LogError("Счёт с идентификатором {InvoiceId} не найден", request.InvoiceId);
            response.Response.Status = ProtoEnums.OperationStatus.NoData;
            response.Response.Description = $"Invoice with identifier {request.InvoiceId} not found";
            return response;
        }

     
        var doc = CreateDocument(invoice.Order.User, invoice, invoice.Order);


        response.InvoicePdf = ByteString.CopyFrom(doc);
        return response;
    }

    private byte[] CreateDocument( Dal.schemas.system.User user, Dal.schemas.data.Invoice invoice, Dal.schemas.data.Order order)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            Document doc = new Document(PageSize.A4, 50, 50, 25, 25);
            PdfWriter writer = PdfWriter.GetInstance(doc, memoryStream);
            doc.Open();

            // Add logo
            string logoPath = "logo.png"; // Замените на путь к вашему логотипу
            if (File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(100, 50);
                logo.Alignment = Element.ALIGN_RIGHT;
                doc.Add(logo);
            }

            // Add header
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            Font subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.ITALIC);

            Paragraph companyName = new Paragraph("BDA RESEARCH LIMITED", headerFont);
            Paragraph tagline = new Paragraph("Transform data into insights", subHeaderFont);

            doc.Add(companyName);
            doc.Add(tagline);
            doc.Add(new Paragraph("\n"));

            // Add company address
            Font bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            Paragraph address = new Paragraph(
                "Tower Business Centre, level 3, 2175\n" +
                "Tower Street, Swatar\n" +
                "Birkirkara, BKR4013\n" +
                "Malta", bodyFont);
            doc.Add(address);
            doc.Add(new Paragraph("\n"));

            // Invoice details
            PdfPTable invoiceTable = new PdfPTable(2);
            invoiceTable.WidthPercentage = 100;
            invoiceTable.SetWidths(new float[] { 1, 2 });

            invoiceTable.AddCell(CreateCell("DATE:", bodyFont, true));
            invoiceTable.AddCell(CreateCell(invoice.CreatedAt.ToLongDateString(), bodyFont, false));

            invoiceTable.AddCell(CreateCell("INVOICE #:", bodyFont, true));
            invoiceTable.AddCell(CreateCell(invoice.Alias, bodyFont, false));

            invoiceTable.AddCell(CreateCell("PAYMENT TERMS:", bodyFont, true));
            invoiceTable.AddCell(CreateCell("10 business days", bodyFont, false));

            doc.Add(invoiceTable);
            doc.Add(new Paragraph("\n"));

            // Billing details
            PdfPTable billTable = new PdfPTable(1);
            billTable.WidthPercentage = 100;
            billTable.AddCell(CreateCell("BILL TO", headerFont, true));
            billTable.AddCell(CreateCell($"Full Name: {user.FirstName} {user.LastName} {user.SecondName}\nCountry: {user.Cca3}\nemail: {user.Email}",
                bodyFont, false));
            doc.Add(billTable);
            doc.Add(new Paragraph("\n"));

            // Description table
            PdfPTable descTable = new PdfPTable(2);
            descTable.WidthPercentage = 100;
            descTable.SetWidths(new float[] { 3, 1 });

            descTable.AddCell(CreateCell("DESCRIPTION", headerFont, true));
            descTable.AddCell(CreateCell($"AMOUNT ({invoice.CurrentCurrency.CurrencyType})", headerFont, true));

            descTable.AddCell(CreateCell("Service description goes here", bodyFont, false));
            descTable.AddCell(CreateCell($"{invoice.CurrentCurrency.TotalAmount}", bodyFont, false));

            doc.Add(descTable);
            doc.Add(new Paragraph("\n"));

            // Payment details
            PdfPTable paymentTable = new PdfPTable(2);
            paymentTable.WidthPercentage = 100;
            paymentTable.SetWidths(new float[] { 1, 1 });

            paymentTable.AddCell(CreateCell($"NET ({invoice.CurrentCurrency.CurrencyType})", bodyFont, true));
            paymentTable.AddCell(CreateCell($"{invoice.CurrentCurrency.TotalAmount}", bodyFont, false));

            paymentTable.AddCell(CreateCell($"VAT ({invoice.CurrentCurrency.CurrencyType})", bodyFont, true));
            paymentTable.AddCell(CreateCell("0", bodyFont, false));

            paymentTable.AddCell(CreateCell($"Other taxes ({invoice.CurrentCurrency.TotalAmount})", bodyFont, true));
            paymentTable.AddCell(CreateCell("0", bodyFont, false));

            paymentTable.AddCell(CreateCell($"GROSS ({invoice.CurrentCurrency.TotalAmount})", bodyFont, true));
            paymentTable.AddCell(CreateCell($"{invoice.CurrentCurrency.TotalAmount}", bodyFont, false));

            doc.Add(paymentTable);

            doc.Close();

            // Convert PDF to Base64 without saving to file
            byte[] pdfBytes = memoryStream.ToArray();

            return pdfBytes;
        }
    }

    private static PdfPCell CreateCell(string text, Font font, bool isHeader)
    {
        PdfPCell cell = new PdfPCell(new Phrase(text, font));
        if (isHeader)
        {
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        }

        cell.Border = Rectangle.NO_BORDER;
        return cell;
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