using Jab;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogSetup;

[ServiceProviderModule]
[Singleton<IConfiguration>(Factory = nameof(GetConfiguration))]
[Singleton<ILoggerFactory>(Factory = nameof(GetLoggerFactory))]
[Transient(typeof(ILogger<>), typeof(Logger<>))]
public interface ILoggingModule
{
    public static IConfiguration GetConfiguration() => Host.CreateApplicationBuilder().Configuration;

    public static ILoggerFactory GetLoggerFactory(IConfiguration configuration) => LoggerFactoryProvider.GetFactory(configuration);
}
