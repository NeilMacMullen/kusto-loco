using System.Diagnostics;
using CommandLine;
using KustoLoco.Rendering;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class RenderCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var fileName = Path.ChangeExtension(o.File.OrWhenBlank(Path.GetTempFileName()), "html");
        var result = exp._prevResult;
        var renderer = new KustoResultRenderer(exp.Settings);
        var text = renderer.RenderToHtml(result);
        File.WriteAllText(fileName, text);
        exp.Info($"Saved chart as {fileName}");
        if (!o.SaveOnly)
            Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        return Task.CompletedTask;
    }

    [Verb("render", aliases: ["ren"], HelpText = "render last results as html and opens with browser")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file")] public string File { get; set; } = string.Empty;

        [Option("saveOnly", HelpText = "just save the file without opening in the browser")]
        public bool SaveOnly { get; set; }
    }
}
