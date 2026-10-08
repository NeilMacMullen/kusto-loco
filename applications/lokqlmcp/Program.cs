using KustoLoco.Core.Settings;
using LokqlMcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

string? folder = null;
var logPath = Path.Combine(Directory.GetCurrentDirectory(), "lokqlmcp.log");
for (var i = 0; i < args.Length; i++)
{
    if (args[i] is "--folder" or "-f" && i + 1 < args.Length)
        folder = args[++i];
    else if (args[i] is "--log" or "-l" && i + 1 < args.Length)
        logPath = Path.GetFullPath(args[++i]);
    else if (!args[i].StartsWith('-'))
        folder ??= args[i];
}

if (string.IsNullOrWhiteSpace(folder))
{
    Console.Error.WriteLine("Usage: lokqlmcp [--folder] <folder-containing-parquet-files> [--log <file>]");
    return 1;
}

var store = new TableStore(new KustoSettingsProvider());
try
{
    await store.LoadFolderAsync(Path.GetFullPath(folder));
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Services.AddSingleton(store);
builder.Services.AddSingleton(new RequestLog(logPath));
Console.Error.WriteLine($"Logging requests to {logPath}");
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
return 0;
