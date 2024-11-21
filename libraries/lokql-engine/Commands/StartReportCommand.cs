using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class StartReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var type = o.Type.ToLowerInvariant();
        if (type=="pptx")
        {
            exp.StartNewReport(PptReportTarget.Create(o.Template));
        }
        else
        if (type == "html")
        {
            var title = o.Title.OrWhenBlank("Report");
            var html = new HtmlReport(title);
            if (o.PaneHeight.IsNotBlank())
            {
                var style = new VegaDivStyle("squashed", $"width: 99vw; height: {o.PaneHeight};");
                html.Composer.AddStyle(style);
                ;
            }
            exp.StartNewReport(html);
           
        }
        else econtext.Explorer.Warn("Unrecognised report type 'type'");

        return Task.CompletedTask;
    }

    [Verb("startreport", HelpText = "start a report")]
    internal class Options
    {

        [Value(0, HelpText = "path including ext")]
        public string Type { get; set; } = string.Empty;
        [Value(1, HelpText = "Title")] public string Title { get; set; } = string.Empty;
        [Option("paneHeight", HelpText = "paneHeight for default style")]
        public string PaneHeight { get; set; } = string.Empty;

        [Option("template", HelpText = "paneHeight for default style")]
        public string Template { get; set; } = string.Empty;


    }
}
