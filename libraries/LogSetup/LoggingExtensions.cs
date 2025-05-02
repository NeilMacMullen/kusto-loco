using System.Reflection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using NLog;
using NLog.Config;
using NLog.Targets;
using Serilog.Templates;
using Serilog.Templates.Themes;
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

            var debuggerTarget = new DebuggerTarget("debug") { Layout = "${time}|${message} ${exception}", };
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, LogLevel.Fatal, debuggerTarget));
        }
    }

    public static IHostApplicationBuilder UseApplicationLogging(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        var entryAssembly = Assembly.GetEntryAssembly();
        var appName = entryAssembly?.GetName().Name ?? "Unknown";
        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        var logPath = Path.Combine(basePath, "logs.json");
        Log.Information("Application storage path: {Path}", basePath);

        services
            .AddSerilog((sc, logBuilder) =>
                {

                    logBuilder
                        .ReadFrom.Configuration(builder.Configuration)
                        .ReadFrom.Services(sc)
                        .Enrich.FromLogContext();
#if DEBUG


                    var newLine = Environment.NewLine;
                    var template = new ExpressionTemplate(
                        "[{@t:HH:mm:ss.fff} {@l:u3}] {Concat('\e[32m',Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1))} | {@mt}{NewLine}  {@p}{NewLine}{@x}"
                            .Replace("{NewLine}", newLine),
                        theme: GetTheme(),
                        applyThemeWhenOutputIsRedirected: true
                    );
                    logBuilder
                        .WriteTo.Console(template)
                        .WriteTo.File(new CompactJsonFormatter(), logPath, fileSizeLimitBytes: 5 * 1000 ^ 2);
#else
                    logBuilder.MinimumLevel.Information();
                    logBuilder.WriteTo.File(new CompactJsonFormatter(), logPath, fileSizeLimitBytes: 5 * 1000^2)
                        .WriteTo.Console(LogEventLevel.Error);
#endif
                }
            );

        return builder;
    }

    private static TemplateTheme GetTheme() => new(TemplateTheme.Code,
        new Dictionary<TemplateThemeStyle, string>
        {
            [TemplateThemeStyle.SecondaryText] = "\e[0;97m",
        }
    );
}
