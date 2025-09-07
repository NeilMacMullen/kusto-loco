using System.Text;
using CommandLine;

namespace Lokql.Engine.Commands;

public static class DefineMacroCommand
{
    internal static async Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var blocks = context.Sequence;
        var description = MakeDescription(blocks);
        var macroBlocks = new List<string>();
        var valid = false;
        while (!blocks.Complete)
        {
            var nextBlock = blocks.Next();
            if (nextBlock == ".end")
            {
                valid = true;
                break;
            }

            macroBlocks.Add(nextBlock);
        }

        if (!valid)
        {
            exp.Warn("Macro definition incomplete - must finish with .end");
            return;
        }

        //let's see if we can get a description from previous blocks
       
        exp.AddMacro(new MacroDefinition(o.Name, o.ParameterNames,
            macroBlocks.ToArray(),description));

        exp.Info($"Macro '{o.Name}' defined");
        await Task.CompletedTask;
    }

    [Verb("define", aliases: ["def"],
        HelpText = "Defines a Macro")]
    internal class Options
    {
        [Value(0, HelpText = "Macro name", Required = true)]
        public string Name { get; set; } = string.Empty;

        [Value(1, HelpText = "parameter names ")]
        public IEnumerable<string> ParameterNames { get; set; } = [];
    }

    private static string MakeDescription(BlockSequence blocks)
    {
        var descriptionLines = new List<string>();
        var prevIndex = 1;
        while (true)
        {
            var block = blocks.PreviousBlock(prevIndex);
            if (block.StartsWith("#"))
                descriptionLines.Add(block[1..]);
            else break;
            prevIndex++;
        }

        var sb = new StringBuilder();

        foreach (var descriptionLine in descriptionLines.ToArray().Reverse())
            sb.AppendLine(descriptionLine);
        return sb.ToString();
    }
}
