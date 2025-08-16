using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class AddToReportCommand
{
    internal static Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var resultName = o.ResultName.IsBlank()
                             ? o.Element
                             : o.ResultName;

        var result = exp._resultHistory.Fetch(resultName);

        var type = o.Type.Trim().ToLowerInvariant();

        switch (type)
        {
            case "image":
                exp.ActiveReport.UpdateOrAddImage(o.Element, exp, result);
                break;
            case "table":
                exp.ActiveReport.UpdateOrAddTable(o.Element, result);
                break;
            case "text":
                var text = (result.RowCount > 0)
                               ? result.Get(0, 0)?.ToString() ?? "<null>"
                               : "no data";
                exp.ActiveReport.UpdateOrAddText(o.Element, text);
                break;
            case "literal":
                exp.ActiveReport.UpdateOrAddText(o.Element, o.ResultName);
                break;
            default:
                exp.Warn($"Unrecognised type '{type}'");
                break;
        }

        return Task.CompletedTask;
    }

    [Verb("addtoreport", HelpText = @"add results to the active report
- Type must be one of image,table, text, or literal
- Element must be the name of an element in the template file.  If the element
  is not found, a new element may be added at the end of the report
- ResultName is the name of a stored result, '_' to indicate the most recent result, or
  left blank to use a result of the same name as the template element.
- For elements of type 'text', only cell (0,0) is used to generate the text.   
- Type 'literal' is the same as 'text' except that the ResultName is treated as a literal string.
Examples:
  .addtoreport image chart1 exceptionResult
  .addtoreport literal title ""this is the title of my report""
")]
    internal class Options
    {
        [Value(0, Required = true)] public string Type { get; set; } = string.Empty;
        [Value(1, Required = true)] public string Element { get; set; } = string.Empty;
        [Value(2)] public string ResultName { get; set; } = string.Empty;
    }
}
