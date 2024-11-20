using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class StartReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var title =o.Title.OrWhenBlank("Report");
        exp.StartNewReport(title);
        if (o.PaneHeight.IsNotBlank())
        {
            var style = new VegaDivStyle("squashed", $"width: 99vw; height: {o.PaneHeight};");
            exp.ActiveReport.Composer.AddStyle(style);;
        }

        return Task.CompletedTask;
    }

    [Verb("startreport", HelpText = "start a report")]
    internal class Options
    {
        [Value(0, HelpText = "Title")] public string Title { get; set; } = string.Empty;
        [Option("paneHeight", HelpText = "paneHeight for default style")]
        public string PaneHeight { get; set; } = string.Empty;
    }
}
