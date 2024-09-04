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

public static class RenderToReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;

        var result = exp._prevResult;
        var renderer = new KustoResultRenderer(exp.Settings);
        renderer.RenderToComposer(result, exp.ActiveReport.Composer);
        return Task.CompletedTask;
    }

    [Verb("renderToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
    }
}

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
