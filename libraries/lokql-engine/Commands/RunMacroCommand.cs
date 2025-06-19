using CommandLine;
using KustoLoco.Core.Settings;

namespace Lokql.Engine.Commands;

public static class RunMacroCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var macro =  exp.GetMacro(o.Name);
        if (macro.Name == string.Empty)
        {
            exp.Warn($"Macro '{o.Name}' not found");
            return ;
        }
        if(macro.ParameterNames.Count() != o.ParameterValues.Count())
        {
            exp.Warn($"Macro '{o.Name}' expects {macro.ParameterNames.Count()} parameters, but {o.ParameterValues.Count()} were provided");
            return ;
        }
        var parameters = new KustoSettingsProvider();
        foreach(var p in macro.ParameterNames.Zip(o.ParameterValues))
        {
            exp.Info($"Parameter {p.First} = {p.Second}");
            parameters.Set(p.First, p.Second);
        }
        exp.PushSettingLayer(parameters);
        var macroSequence = macro.Sequence();
        await exp.RunSequence(macroSequence);
        exp.PopSettingLayer();
    }

    [Verb("macro", aliases: ["m"],
        HelpText = "runs a macro")]
    internal class Options
    {
        [Value(0, HelpText = "macro name", Required = true)]
        public string Name { get; set; } = string.Empty;
        [Value(1, HelpText = "parameter Values ")]
        public IEnumerable<string> ParameterValues { get; set; } = [];
    }
}
