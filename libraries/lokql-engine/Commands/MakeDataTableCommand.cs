using System.Text;
using System.Text.Json.Nodes;
using Clowd.Clipboard;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.Core.Util;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class MakeDataTableCommand
{
    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var result = context.History.Fetch(o.ResultName);
        var text = DataTableBuilder.Build(result);
        console.Info(text);

        if (!string.IsNullOrWhiteSpace(o.File))
        {
            try
            {
                var path = Path.GetFullPath(o.File);
                await File.WriteAllTextAsync(path, text);
                console.Info($"Saved datatable expression to {path}");
            }
            catch (Exception e)
            {
                console.Warn($"Unable to save to file: {e.Message}");
            }
        }

        if (o.Clip)
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                    throw new InvalidOperationException("clipboard not supported on this platform");
                await ClipboardAvalonia.SetTextAsync(text);
                console.Info("Copied datatable expression to clipboard");
            }
            catch (Exception)
            {
                console.Warn("Unable to copy to clipboard");
            }
        }
    }

    [Verb("makedatatable", aliases: ["mkdt"],
        HelpText = @"creates a KQL datatable expression from a query result
The expression is written to the output window and can optionally be saved or copied.
Examples:
  .makedatatable                     # datatable from last result
  .makedatatable myResult            # datatable from named result
  .makedatatable --file dt.kql       # also save to a file
  .makedatatable --clip              # also copy to clipboard
  ")]
    internal class Options
    {
        [Value(0, HelpText = "Name of result (or most recent result if left blank)")]
        public string ResultName { get; set; } = string.Empty;

        [Option('f', "file", HelpText = "File to save the expression to")]
        public string File { get; set; } = string.Empty;

        [Option('c', "clip", HelpText = "Copy the expression to the clipboard")]
        public bool Clip { get; set; }
    }

    internal static class DataTableBuilder
    {
        public static string Build(KustoQueryResult result)
        {
            var columns = result.ColumnDefinitions();
            var sb = new StringBuilder();
            sb.Append("datatable(");
            sb.Append(string.Join(", ",
                columns.Select(c => $"{NameEscaper.EscapeIfNecessary(c.Name)}:{KqlTypeName(c.UnderlyingType)}")));
            sb.AppendLine(")");
            sb.AppendLine("[");
            var rows = result.EnumerateRows().ToArray();
            for (var r = 0; r < rows.Length; r++)
            {
                var row = rows[r];
                var cells = columns.Select(c => Literal(row[c.Index], c.UnderlyingType));
                sb.Append("    ").Append(string.Join(", ", cells));
                if (r < rows.Length - 1)
                    sb.Append(',');
                sb.AppendLine();
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static string KqlTypeName(Type type)
        {
            return TypeMapping.SymbolForType(type).Name;
        }

        private static string Literal(object? value, Type type)
        {
            var kql = KqlTypeName(type);
            if (value is null || value == DBNull.Value)
                return kql switch
                {
                    "string" => "\"\"",
                    "dynamic" => "dynamic(null)",
                    _ => $"{kql}(null)"
                };

            var text = value is JsonNode node
                ? node.ToJsonString()
                : KustoFormatter.ObjectToKustoString(value);
            return kql switch
            {
                "string" => Quote(text),
                "bool" => text.ToLowerInvariant(),
                "dynamic" => $"dynamic({text})",
                _ => $"{kql}({text})"
            };
        }

        private static string Quote(string s)
            => "\"" + s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t") + "\"";
    }
}
