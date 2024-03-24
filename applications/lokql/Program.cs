
using CommandLine;
using LogSetup;
using NLog;

LoggingExtensions.SetupLoggingForConsole(LogLevel.Info);
await StandardParsers.Default
    .ParseArguments(args,
        typeof(CmdExplore.Options))
    .WithParsedAsync<CmdExplore.Options>(CmdExplore.RunAsync);


    