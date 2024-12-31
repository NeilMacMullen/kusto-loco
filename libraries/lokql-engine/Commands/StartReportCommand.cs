using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class StartReportCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var type = o.Type.ToLowerInvariant();
        if (type=="pptx")
        {
            exp.StartNewReport(PptReportTarget.Create(o.Template));
        }
        /*
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
        */
        else econtext.Explorer.Warn($"Unrecognised/unsupported report type '{type}'");

        return Task.CompletedTask;
    }

    [Verb("startreport", HelpText = @"starts building a report
Currently only the pptx format is supported
The template argument must point to a valid pptx file which will be used as a template
Examples:
  .startreport pptx c:\reports\weeklytemplate.pptx
  .addtoreport text *name-of-title-element* ""my report""
  .addtoreport image *name-of-image-element* storedresult1
  .finishreport c:\reports\thisweek.pptx
")]
    internal class Options
    {
        [Value(0, HelpText = "Type (must be 'pptx')",Required = true)]
        public string Type { get; set; } = string.Empty;
      
        [Value(1,HelpText = "Template file",Required = true)]
        public string Template { get; set; } = string.Empty;
    }
}
