using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using NLog;
using NLog.Config;
using NLog.Targets;
using LogLevel = NLog.LogLevel;


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

    public static IServiceCollection AddApplicationLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        var logPath = Path.Combine(basePath, "logs.json");
        Log.Information("Application storage path: {Path}", basePath);

        services
            .AddLogging(logBuilder =>
                {
                    logBuilder.ClearProviders();
                    var loggerConfig = new LoggerConfiguration();

                    loggerConfig.Enrich.FromLogContext();


#if DEBUG
                    loggerConfig.MinimumLevel.Debug();
                    loggerConfig
                        .WriteTo.Console(LogEventLevel.Debug)
                        .WriteTo.File(new CompactJsonFormatter(), logPath, LogEventLevel.Debug);
#else
                    loggerConfig.MinimumLevel.Information();
                    loggerConfig.WriteTo.File(new CompactJsonFormatter(), logPath)
                        .WriteTo.Console(LogEventLevel.Error);
#endif
                    var logger = loggerConfig.CreateLogger();

                    logBuilder.AddSerilog(logger,true);
                }
            );


        return services;
    }
}
