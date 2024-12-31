using CommandLine;

namespace Lokql.Engine.Commands;

public static class FinishReportCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var report = exp.ActiveReport;
        report.SaveAs(o.File);
        exp.Info($"Saved report {o.File}");
        return Task.CompletedTask;
    }

    [Verb("finishreport", HelpText = @"finish a report by saving it out as a file")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file to save report to ", Required = true)]
        public string File { get; set; } = string.Empty;

    }
}
