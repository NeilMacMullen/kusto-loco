using System.Reflection;

namespace LokqlDx.Services;

public static class CrashLogger
{
    public static string CrashLogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "lokqlDx",
        "crash.log");

    private static readonly object Lock = new();

    public static void Log(string source, Exception? ex)
    {
        try
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
            var text = $"""
                        ==================================================
                        {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{source}] lokqlDx v{version}
                        {ex?.ToString() ?? "(no exception object)"}

                        """;
            lock (Lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CrashLogPath)!);
                File.AppendAllText(CrashLogPath, text);
            }
        }
        catch
        {
            // never allow crash logging to cause a further failure
        }
    }
}
