using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Layouts.ClefJsonLayout;
using NLog.Targets;
using NLogLevel = NLog.LogLevel;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LogSetup;

public class LoggerFactoryProvider
{
    private const string KustoLocoLog = "kusto-loco.log";
    private const string KustoLocoStartupLog = "kusto-loco-startup.log";

    public static void DirectLog(string str)
    {
        var path = Path.Combine(Path.GetTempPath(), KustoLocoStartupLog);
        File.AppendAllText(path,$"{DateTime.Now:yyyy-MMM-dd HH:mm:ss}:{str}{Environment.NewLine}");
    }

    public static ILoggerFactory GetFactory(IConfiguration configuration)
    {
        Console.WriteLine("Dumping configuration");
        DumpConfig(configuration);
        // callsite: does not work because of logging extension wrapper methods ("LoggingExtensions.Log")
        var firstLine = """
                        ${time}
                        ${level:uppercase=true}
                        [32m${logger:shortName=true}[0m
                        | ${message:raw=true:withException=true:exceptionSeparator=|}
                        """.ReplaceLineEndings(" ");


        var compoundLayout = new CompoundLayout
        {
            Layouts =
            {
                firstLine,
                new InternalJsonLayout()
            }
        };

        var defaultLevel = GetLogLevel(configuration, "Logging:LogLevel:Default", LogLevel.Information);

        var consoleLevel = GetLogLevel(configuration, "Console:LogLevel:Default", defaultLevel);
        var debugLevel = GetLogLevel(configuration, "Debug:LogLevel:Default", defaultLevel);
        var fileLevel = GetLogLevel(configuration, "File:LogLevel:Default", defaultLevel);
        var consoleNLogLevel = ToNLogLevel(consoleLevel);
        var debugNLogLevel = ToNLogLevel(debugLevel);
        var fileNLogLevel = ToNLogLevel(fileLevel);
        var logPath = configuration["File:Path"] ?? Path.Combine(Path.GetTempPath(), KustoLocoLog);
        Console.WriteLine($"Logging to {logPath}");

        var factory = LogManager
            .Setup()
            .LoadConfiguration(b =>
                {
                    b
                        .ForLogger()
                        .FilterMinLevel(consoleNLogLevel)
                        .WriteTo(new ColoredConsoleTarget
                            {
                                Layout = compoundLayout,
                                UseDefaultRowHighlightingRules = false,
                                Encoding = Encoding.UTF8,
                                EnableAnsiOutput = true,
                                WordHighlightingRules =
                                {
                                    new(NLogLevel.Trace.ToString().ToUpper(),
                                        ConsoleOutputColor.Gray,
                                        ConsoleOutputColor.NoChange
                                    ),
                                    new(NLogLevel.Debug.ToString().ToUpper(),
                                        ConsoleOutputColor.White,
                                        ConsoleOutputColor.NoChange
                                    ),
                                    new(NLogLevel.Info.ToString().ToUpper(),
                                        ConsoleOutputColor.Cyan,
                                        ConsoleOutputColor.NoChange
                                    ),
                                    new(NLogLevel.Warn.ToString().ToUpper(),
                                        ConsoleOutputColor.Yellow,
                                        ConsoleOutputColor.NoChange
                                    ),
                                    new(NLogLevel.Error.ToString().ToUpper(),
                                        ConsoleOutputColor.Red,
                                        ConsoleOutputColor.NoChange
                                    ),
                                    new(NLogLevel.Fatal.ToString().ToUpper(),
                                        ConsoleOutputColor.DarkRed,
                                        ConsoleOutputColor.White
                                    )
                                }
                            }
                        );
                    b
                        .ForLogger()
                        .FilterMinLevel(debugNLogLevel)
                        .WriteToDebugConditional(compoundLayout);

                    b
                        .ForLogger()
                        .FilterMinLevel(fileNLogLevel)
                        .WriteToFile(logPath,
                            new CompactJsonLayout
                            {
                                IncludeEventProperties = true,
                                IncludeScopeProperties = true,
                                RenderEmptyObject = false,
                                MaxRecursionLimit = 3
                            },
                            Encoding.UTF8,
                            maxArchiveDays: 30,
                            maxArchiveFiles: 3,
                            archiveAboveSize: 10 * (int)Math.Pow(1024, 2)
                        );
                }
            )
            .LogFactory;


        return new NLogLoggerFactory(new NLogLoggerProvider(
                new NLogProviderOptions
                {
                    IncludeScopes = true,
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true,
                    CaptureMessageParameters = true
                },
                factory
            )
        );
    }

    private static void DumpConfig(IConfiguration config)
    {
        DumpConfigSection(config, parentPath: string.Empty);
    }

    private static void DumpConfigSection(IConfiguration config, string parentPath)
    {
        foreach (var child in config.GetChildren())
        {
            // Build the full key path
            var key = string.IsNullOrEmpty(parentPath)
                ? child.Key
                : $"{parentPath}:{child.Key}";

            if (child.Value == null)
            {
                // This is a section with nested children
                DumpConfigSection(child, key);
            }
            else
            {
                // This is a leaf value
                Console.WriteLine($"{key} = {child.Value}");
            }
        }
    }

    private static LogLevel GetLogLevel(IConfiguration configuration, string configSection, LogLevel defaultLogLevel)
    {


        var logLevel = configuration[configSection] ?? "";
        var level = defaultLogLevel;
        if (Enum.TryParse<LogLevel>(logLevel, true, out var level2))
        {
            level = level2;
        }

        return level;
    }

    private static NLogLevel ToNLogLevel(LogLevel level)
    {
        var nlogLevel = level switch
        {
            LogLevel.None => NLogLevel.Off,
            LogLevel.Trace => NLogLevel.Trace,
            LogLevel.Debug => NLogLevel.Debug,
            LogLevel.Information => NLogLevel.Info,
            LogLevel.Warning => NLogLevel.Warn,
            LogLevel.Error => NLogLevel.Error,
            LogLevel.Critical => NLogLevel.Fatal,
            _ => NLogLevel.Off
        };
        return nlogLevel;
    }
}

/// <summary>
/// Renders properties on an indented new line if there are any.
/// </summary>
file class InternalJsonLayout : Layout
{
    private readonly JsonLayout _layout = new()
    {
        IncludeEventProperties = true,
        IncludeScopeProperties = true,
        RenderEmptyObject = false,
        MaxRecursionLimit = 1
    };

    protected override string GetFormattedMessage(LogEventInfo logEvent)
    {
        var res = _layout.Render(logEvent);
        return res is "" ? "" : $"{Environment.NewLine}  {res}";
    }
}
