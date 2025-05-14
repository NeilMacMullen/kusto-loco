using Microsoft.Extensions.Logging;

namespace Intellisense;

internal static class LoggerHelperExtensions
{
    public static IDisposable? BeginScope<T>(this ILogger<T> logger, Dictionary<string, object> properties) => logger.BeginScope(properties);
}
