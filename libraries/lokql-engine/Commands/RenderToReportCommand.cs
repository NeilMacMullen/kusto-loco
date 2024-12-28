﻿using CommandLine;


namespace Lokql.Engine.Commands;

public static class SleepCommand
{
    internal static async Task Run(CommandProcessorContext econtext, Options o)
    {
        econtext.Explorer.Info("Sleeping...");
        await Task.Delay(TimeSpan.FromSeconds(o.Seconds));
    }

    [Verb("sleep", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public double Seconds { get; set; } =0;
    }
}

public static class RenderToReportCommand
{
    internal static async Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
      
        await exp.ActiveReport.UpdateOrAddImage(o.Name,exp);
    }

    [Verb("renderToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public string Name { get; set; } = string.Empty;
    }
}


public static class RenderTableToReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
       
            exp.ActiveReport.UpdateOrAddTable(o.Name, exp.GetPreviousResult());
        

        return Task.CompletedTask;
    }

    [Verb("renderTableToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public string Name { get; set; } = string.Empty;
    }
}

public static class RenderTableToText
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
            var text = (exp.GetPreviousResult().RowCount > 0) ? exp.GetPreviousResult().Get(0, 0)?.ToString()??"<null>" : "no data";
                
            exp.ActiveReport.UpdateOrAddText(o.Name, text);

        return Task.CompletedTask;
    }

    [Verb("renderTextToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public string Name { get; set; } = string.Empty;
    }
}