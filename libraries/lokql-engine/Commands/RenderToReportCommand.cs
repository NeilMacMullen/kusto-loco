using CommandLine;


namespace Lokql.Engine.Commands;


public static class AddToReportCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var result = exp._resultHistory.Fetch(o.ResultName);

        var type = o.Type.Trim().ToLowerInvariant();
        switch (type)
        {
            case "image":
                await exp.ActiveReport.UpdateOrAddImage(o.Element, exp, result);
                break;
            case "table":
                exp.ActiveReport.UpdateOrAddTable(o.Element, exp._resultHistory.Fetch(o.ResultName));
                break;
            case "text":
                var text = (result.RowCount > 0) ? result.Get(0, 0)?.ToString()??"<null>" : "no data";
                exp.ActiveReport.UpdateOrAddText(o.Element, text);
                break;
            case "literal":
                exp.ActiveReport.UpdateOrAddText(o.Element, o.ResultName);
                break;
            default :
                exp.Warn("Unrecognised type '{}'");
                break;
        }
    }

    [Verb("addtoreport", HelpText = @"add results to the active report
- Type must be one of image,table, text, or literal
- Element must be the name of an element in the template file.  If the element
  is not found, a new element may be added at the end of the report
- ResultName is the name of a stored result or the most recent result if left blank.
- For elements of type 'text', only cell (0,0) is used to generate the text.   
- Type 'literal' is the same as 'text' except that the ResultName is treated as a literal string.
Examples:
  .addtoreport image chart1 exceptionResult
  .addtoreport literal title ""this is the title of my report""
")]
    internal class Options
    {
        [Value(0,Required = true)] public string Type { get; set; } = string.Empty;
        [Value(1,Required = true)] public string Element { get; set; } = string.Empty;
        [Value(2)] public string ResultName { get; set; } = string.Empty;
    }
}
