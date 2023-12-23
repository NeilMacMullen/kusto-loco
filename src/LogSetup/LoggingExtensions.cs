using NLog;
using NLog.Config;
using NLog.Targets;

namespace LogSetup;

public static class LoggingExtensions
{
    public static void SetupLoggingForTest(LogLevel level)
        => NLogFactoryBuilder(level, LoggingTarget.Console, LoggingTarget.Debugger);


    public static void SetupLoggingForConsole(LogLevel level)
        => NLogFactoryBuilder(level, LoggingTarget.Console);

    internal static LogFactory NLogFactoryBuilder(LogLevel level, params LoggingTarget[] loggingTargets)
    {
        var factory = new LogFactory();
        var config = new LoggingConfiguration();

        // Create filter log. No need pass the Microsoft.* and System.* pattern into any target.
#if !DEBUG
        //var ignoreSystemLoggingRule = CreateIgnoreLoggingRule("System.*");
        //var ignoreMicrosoftLoggingRule = CreateIgnoreLoggingRule("Microsoft.*");
        //config.AddRule(ignoreSystemLoggingRule);
        //config.AddRule(ignoreMicrosoftLoggingRule);
#endif

        // Log to console.
        SinkToConsole();


        // Log to Debugger.
        SinkToDebugger();

        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();
        factory.Configuration = config;
        return factory;

        void SinkToConsole()
        {
            if (!loggingTargets.Contains(LoggingTarget.Console))
                return;

            var consoleTarget = new ColoredConsoleTarget("logconsole")
            {
                Layout = "${callsite:IncludeNamespace=false} ${message} ${exception}",
                UseDefaultRowHighlightingRules = true
            };
            config.LoggingRules.Add(new LoggingRule("*", level, consoleTarget));
        }


        void SinkToDebugger()
        {
            if (!loggingTargets.Contains(LoggingTarget.Debugger))
                return;

            var debuggerTarget = new DebuggerTarget("debug") { Layout = "${time}|${message} ${exception}" };
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, LogLevel.Fatal, debuggerTarget));
        }
    }
}