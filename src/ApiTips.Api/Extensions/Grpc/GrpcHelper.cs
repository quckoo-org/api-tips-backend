using System.Security.Cryptography;
using System.Text;
using ApiTips.CustomEnums.V1;
using ApiTips.Dal.Enums;
using Grpc.Core;
using Decimal = ApiTips.CustomTypes.V1.Decimal;

namespace ApiTips.Api.Extensions.Grpc;

public static class GrpcHelper
{
    public static decimal FromDecimal(this Decimal value)
    {
        return value.Units + value.Nanos * (decimal)Math.Pow(10, -9);
    }

    public static Decimal ToDecimal(this decimal value)
    {
        var units = (long)Math.Truncate(value);
        var nanos = (int)((value - units) * (decimal)Math.Pow(10, 9));

        return new Decimal
        {
            Units = units,
            Nanos = nanos
        };
    }

    public static string GetUserEmail(this ServerCallContext context)
    {
        var requestedHeaders = context.RequestHeaders;

        var emailHeader = requestedHeaders
            .FirstOrDefault(entry => entry.Key.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;

        return emailHeader ?? "system";
    }

    public static string? ComputeSha256Hash(this string? rawData)
    {
        if (rawData is null) return null;

        // ComputeHash - returns byte array
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        // Convert byte array to a string
        var builder = new StringBuilder();
        foreach (var t in bytes)
            builder.Append(t.ToString("x2"));

        return builder.ToString();
    }

    /// <summary>
    ///     Конвертация статуса счета из слоя gRPC на слой БД
    /// </summary>
    /// <param name="invoiceStatus"></param>
    /// <returns></returns>
    public static InvoiceStatusEnum ConvertToDbInvoiceStatusEnum(this InvoiceStatus invoiceStatus)
    {
        switch (invoiceStatus)
        {
            case InvoiceStatus.Unspecified:
                return InvoiceStatusEnum.Unspecified;
            case InvoiceStatus.Created:
                return InvoiceStatusEnum.Created;
            case InvoiceStatus.Paid:
                return InvoiceStatusEnum.Paid;
            case InvoiceStatus.Cancelled:
                return InvoiceStatusEnum.Cancelled;
            default:
                return InvoiceStatusEnum.Unspecified;
        }
    }
    
    /// <summary>
    ///     Конвертация статуса счета из слоя БД на слой gRPC
    /// </summary>
    /// <param name="invoiceStatus"></param>
    /// <returns></returns>
    public static InvoiceStatus ConvertFromDbInvoiceStatus(this InvoiceStatusEnum invoiceStatus)
    {
        switch (invoiceStatus)
        {
            case InvoiceStatusEnum.Unspecified:
                return InvoiceStatus.Unspecified;
            case InvoiceStatusEnum.Created:
                return InvoiceStatus.Created;
            case InvoiceStatusEnum.Paid:
                return InvoiceStatus.Paid;
            case InvoiceStatusEnum.Cancelled:
                return InvoiceStatus.Cancelled;
            default:
                return InvoiceStatus.Unspecified;
        }
    }
}