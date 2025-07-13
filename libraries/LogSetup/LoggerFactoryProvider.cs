using NLog;
using NLog.Extensions.Logging;

namespace LogSetup;

public class LoggerFactoryProvider
{
    public NLogLoggerFactory Get()
    {
        return new NLogLoggerFactory(new NLogLoggerProvider(new NLogProviderOptions(),
                LoggingExtensions.NLogFactoryBuilder(LogLevel.Trace, LoggingTarget.Console, LoggingTarget.Debugger)
            )
        );
    }
}
