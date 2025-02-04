using Serilog.Core;
using Serilog.Events;

namespace ApiTips.Integration.Brains.Client.Extensions.Logger;

public class CommitEnricher(string? commit) : ILogEventEnricher
{
    private readonly string _commit = commit ?? "<unknown>";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Commit", _commit));
    }
}