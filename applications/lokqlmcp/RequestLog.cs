using System.Text;

namespace LokqlMcp;

/// <summary>
///     Appends a human-readable record of each tool call to a text file.
/// </summary>
public sealed class RequestLog(string path)
{
    private readonly object _lock = new();

    public string Path { get; } = path;

    public void Write(string tool, string? input, string action, TimeSpan duration, string? detail = null)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {tool}");
            if (!string.IsNullOrWhiteSpace(input))
            {
                sb.AppendLine("  Input:");
                Indent(sb, input, "    ");
            }

            sb.AppendLine($"  Action: {action}");
            if (!string.IsNullOrWhiteSpace(detail))
            {
                sb.AppendLine("  Detail:");
                Indent(sb, detail, "    ");
            }

            sb.AppendLine($"  Duration: {duration.TotalMilliseconds:0} ms");
            sb.AppendLine();

            lock (_lock)
            {
                using var stream = new FileStream(Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to write request log: {ex.Message}");
        }
    }

    private static void Indent(StringBuilder sb, string text, string prefix)
    {
        foreach (var line in text.TrimEnd().Split('\n'))
            sb.AppendLine(prefix + line.TrimEnd('\r'));
    }
}
