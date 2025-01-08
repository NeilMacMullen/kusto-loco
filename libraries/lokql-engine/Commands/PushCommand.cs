using CommandLine;

namespace Lokql.Engine.Commands;

/// <summary>
///     Saves last result as named result
/// </summary>
public static class PushCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        exp._resultHistory.Save(o.Name);
        exp.Info($"result stored as '{o.Name}'");
        await Task.CompletedTask;
    }

    [Verb("push", HelpText =
        @"names and stores the previous result so that it can be used later without rerunning the query
Examples:
  .push result1
  ... run some other queries....
  .save data.csv result1  #save the earlier result to a file
")]
    internal class Options
    {
        [Value(0, HelpText = "Name", Required = true)]
        public string Name { get; set; } = string.Empty;
    }
}


/// <summary>
///     Saves last result as named result
/// </summary>
public static class SetScalarCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var result =exp._resultHistory.Fetch(o.Result);
        var text = (result.RowCount > 0) ? result.Get(0, 0)?.ToString() ?? "<null>" : "no data";
        exp.Settings.Set(o.Name, text);
        exp.Info($"Set {o.Name} to {text}");
        await Task.CompletedTask;
    }

    [Verb("setscalar", HelpText =
        @"Sets the value in cell 0,0 of the named result as a value in the settings table
Examples:
print format_datetime(now(),'yyyy-MM-dd')
.setscalar file
.save $(file).csv earlier_result
")]
    internal class Options
    {
        [Value(0, HelpText = "Name", Required = true)]
        public string Name { get; set; } = string.Empty;
        [Value(1, HelpText = "Result")]
        public string Result { get; set; } = string.Empty;
    }
}

