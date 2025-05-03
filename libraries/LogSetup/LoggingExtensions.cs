// ReSharper disable RedundantUsingDirective DO NOT REMOVE, COMPILER DIRECTIVES WILL FALSELY FLAG PACKAGES
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Formatting.Compact;
using NLog;
using NLog.Config;
using NLog.Targets;
using Serilog.Events;
using Serilog.Filters;
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

        builder.Services.AddOptions<LoggingOptions>().BindConfiguration(LoggingOptions.Logging);


        services
            .AddSerilog((sc, logBuilder) =>
                {
                    logBuilder
                        .ReadFrom.Configuration(builder.Configuration)
                        .ReadFrom.Services(sc)
                        .Enrich.FromLogContext();

                    using var bootstrapLogger = new LoggerConfiguration().WriteTo.Console(new CompactJsonFormatter()).CreateLogger();

                    var opts = sc.GetRequiredService<IOptions<LoggingOptions>>();

                    var logPath = opts.Value.LogPath;
                    if (string.IsNullOrWhiteSpace(logPath))
                    {
                        bootstrapLogger.Information("Log storage path not configured.");
                    }
                    else
                    {
                        bootstrapLogger.Information("Log storage path: {Path}", logPath);
                        logBuilder.WriteTo.File(new CompactJsonFormatter(), logPath, fileSizeLimitBytes: 5 * 1000 ^ 2);
                    }


#if DEBUG
                    var newLine = Environment.NewLine;
                    var template = new ExpressionTemplate(
                        "[{@t:HH:mm:ss.fff} {@l:u3}] {Concat('\e[32m',Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1))} | {@mt}{NewLine}  {@p}{NewLine}{@x}"
                            .Replace("{NewLine}", newLine),
                        theme: GetTheme(),
                        applyThemeWhenOutputIsRedirected: true
                    );
                    logBuilder.WriteTo.Console(template)
                        .WriteTo.Debug(template)
                        .WriteTo.File(new CompactJsonFormatter(), logPath, fileSizeLimitBytes: 5 * 1000 ^ 2)
                        .Filter.ByExcluding(Matching.WithProperty<string>("SourceContext",s => s.StartsWith("Microsoft.") || s.StartsWith("System.")));

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

public class LoggingOptions
{
    public const string Logging = "Logging";
    public string Directory { get; set; } = string.Empty;
    internal string LogPath => Path.Combine(Directory, "log.json");
}