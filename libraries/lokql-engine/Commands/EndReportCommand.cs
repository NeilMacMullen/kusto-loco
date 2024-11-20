using System.Diagnostics;
using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class EndReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var fileName = Path.ChangeExtension(o.File.OrWhenBlank(Path.GetTempFileName()), "html");
        var exp = econtext.Explorer;
        var report = exp.ActiveReport;
        var text = report.Render();
        File.WriteAllText(fileName, text);
        exp.Info($"Saved chart as {fileName}");
        if (!o.SaveOnly)
            Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        return Task.CompletedTask;
    }

    [Verb("endreport", HelpText = "end a report")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file to save report to ")]
        public string File { get; set; } = string.Empty;

        [Option("saveOnly", HelpText = "just save the file without opening in the browser")]
        public bool SaveOnly { get; set; }
    }
}
