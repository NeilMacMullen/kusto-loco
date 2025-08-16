using CommandLine;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
/// Loads all sheets an an excel workbook
/// </summary>
public static class LoadExcel
{
    internal static async Task RunAsync(CommandContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var ser = new ExcelSerializer(exp.Settings, exp._outputConsole);
        var prefix = o.As.OrWhenBlank(Path.GetFileNameWithoutExtension(o.File));
        var dt = await ser.LoadAllTables(o.File, prefix);

        foreach (var res in dt)
        {
            if (res.Error.IsNotBlank())
            {
                exp.Warn($"{res.Error}");
                continue;
            }

            var tableName = res.Table.Name;
            //remove table if it already exists
            if (exp.GetCurrentContext().HasTable(tableName))
            {
                if (o.Force)
                {
                    exp.GetCurrentContext().RemoveTable(tableName);
                }
                else
                {
                    exp.Warn($"Table '{tableName}' already exists.  Use '.load -f' to force reload");
                    continue;
                }
            }
            exp.GetCurrentContext().AddTable(res.Table);
           
            exp.Info($"Table {NameEscaper.EscapeIfNecessary(tableName)} now available");
        }
    }

    [Verb("loadexcel",
        HelpText = @"loads multiple tables from an excel spreadsheet.
Tables are named after the worksheet that contains them, with an optional prefix.
")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file", Required = true)]
        [FileOptions(Extensions = [".xlsx", ".xls", ".xlsb"])]
        public string File { get; set; } = string.Empty;

        [Value(1, HelpText = "Prefix for loaded tables (defaults to name of file)")]
        public string As { get; set; } = string.Empty;

        [Option('f', "force", HelpText = "Force reload")]
        public bool Force { get; set; }
    }
}
