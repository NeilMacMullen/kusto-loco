using KustoLoco.Core.Settings;
using LokqlMcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string Usage = "Usage: lokqlmcp <run|updateConfig> --folder <folder-containing-parquet-files> [--config <file.md>] [--log <file>]";
if (args.Length == 0 || args[0].StartsWith('-'))
{
    Console.Error.WriteLine(Usage);
    return 1;
}

var verb = args[0];
if (!verb.Equals("run", StringComparison.OrdinalIgnoreCase) &&
    !verb.Equals("updateConfig", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine($"Unknown verb '{verb}'.");
    Console.Error.WriteLine(Usage);
    return 1;
}

args = args[1..];
string? folder = null;
string? configPath = null;
var logPath = Path.Combine(Directory.GetCurrentDirectory(), "lokqlmcp.log");
for (var i = 0; i < args.Length; i++)
{
    if (args[i] is "--folder" or "-f" && i + 1 < args.Length)
        folder = args[++i];
    else if (args[i] is "--log" or "-l" && i + 1 < args.Length)
        logPath = Path.GetFullPath(args[++i]);
    else if (args[i] is "--config" or "-c" && i + 1 < args.Length)
        configPath = Path.GetFullPath(args[++i]);
    else if (!args[i].StartsWith('-'))
        folder ??= args[i];
}

if (string.IsNullOrWhiteSpace(folder))
{
    Console.Error.WriteLine(Usage);
    return 1;
}

if (verb.Equals("updateConfig", StringComparison.OrdinalIgnoreCase))
{
    if (configPath is null)
    {
        Console.Error.WriteLine("updateConfig requires --config <file>");
        return 1;
    }

    var updateStore = new TableStore(new KustoSettingsProvider());
    try
    {
        await updateStore.LoadFolderAsync(Path.GetFullPath(folder));
        var existingText = File.Exists(configPath) ? File.ReadAllText(configPath) : null;
        var newText = ServerConfig.AddMissingTables(existingText,
            updateStore.Tables().Select(t => t.Name), out var added);
        if (added.Count > 0 || existingText is null)
            File.WriteAllText(configPath, newText);
        Console.Error.WriteLine(existingText is null
            ? $"Created {configPath} with {added.Count} table(s)"
            : $"Updated {configPath}: added {added.Count} table(s)");
        foreach (var name in added)
            Console.Error.WriteLine($"  {name}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

var config = ServerConfig.Empty;
if (configPath is not null)
{
    try
    {
        config = ServerConfig.Load(configPath);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Could not read config file '{configPath}': {ex.Message}");
        return 1;
    }
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

foreach (var name in config.TableDescriptions.Keys)
    if (store.FindTable(name) is null)
        Console.Error.WriteLine($"Warning: config describes table '{name}' which is not loaded");

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton(config);
builder.Services.Configure<ModelContextProtocol.Server.McpServerOptions>(o =>
    o.ServerInstructions = config.Context is null
        ? KqlTools.Boilerplate
        : KqlTools.Boilerplate + "\n\n" + config.Context);
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
