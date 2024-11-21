using System.Diagnostics;
using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class EndReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
       
        var exp = econtext.Explorer;
        var report = exp.ActiveReport;
         report.SaveAs(o.File);
         exp.Info($"Saved chart as {o.File}");
        // if (!o.SaveOnly)
        //     Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        return Task.CompletedTask;
    }

    [Verb("endreport", HelpText = "end a report")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file to save report to ",Required = true)]
        public string File { get; set; } = string.Empty;

        [Option("saveOnly", HelpText = "just save the file without opening in the browser")]
        public bool SaveOnly { get; set; }
    }
}
