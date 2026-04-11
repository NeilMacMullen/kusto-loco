using System.Diagnostics;
using CommandLine;
using KustoLoco.Rendering;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class RenderCommand
{
    internal static Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var fileName = Path.ChangeExtension(o.File.OrWhenBlank(Path.GetTempFileName()), "html");
        var result = exp.GetPreviousResult();
        var renderer = new KustoResultRenderer(exp.Settings);
        var text = renderer.RenderToHtml(result);
        File.WriteAllText(fileName, text);
        exp.Info($"Saved chart as {fileName}");
        if (!o.SaveOnly)
            Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        return Task.CompletedTask;
    }

    [Verb("render", aliases: ["ren"], HelpText = @"renders the last query result as HTML and opens in browser
Generates an interactive HTML visualization of your data.
Examples:
  .render                        # Render to temp file and open in browser
  .render output.html            # Render to specific file and open
  .render report.html --saveOnly # Save without opening browser")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file")]
        [FileOptions(Extensions = [".html"])]
        public string File { get; set; } = string.Empty;

        [Option("saveOnly", HelpText = "just save the file without opening in the browser")]
        public bool SaveOnly { get; set; }
    }
}
