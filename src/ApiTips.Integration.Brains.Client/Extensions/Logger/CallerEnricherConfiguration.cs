using Serilog;
using Serilog.Configuration;

namespace ApiTips.Integration.Brains.Client.Extensions.Logger;

public static class CallerEnrichmentConfiguration
{
    public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<CallerEnricher>();
    }
}