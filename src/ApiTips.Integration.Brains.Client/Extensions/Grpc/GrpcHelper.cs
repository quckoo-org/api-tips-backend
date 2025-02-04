using System.Security.Cryptography;
using System.Text;
using Grpc.Core;

namespace ApiTips.Integration.Brains.Client.Extensions.Grpc;

public static class GrpcHelper
{
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
}