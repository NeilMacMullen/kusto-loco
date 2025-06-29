using CommandLine;

namespace Lokql.Engine.Commands;

public static class FinishReportCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var report = exp.ActiveReport;
        var reportPath = Path.IsPathRooted(o.File)
            ? o.File
            : Path.Combine(exp.Settings.Get(StandardFormatAdaptor.Settings.KustoDataPath), o.File);
        report.SaveAs(reportPath);
        exp.Info($"Saved report {reportPath}");
        return Task.CompletedTask;
    }

    [Verb("finishreport", HelpText = "finish a report by saving it out as a file")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file to save report to ", Required = true)]
        [FileOptions(Extensions = [".html", ".ppt", ".pptx"])]
        public string File { get; set; } = string.Empty;
    }
}
