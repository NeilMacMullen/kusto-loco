using Intellisense;
using Lokql.Engine;
using lokqlDxComponents.Contexts;
namespace lokqlDxComponents.Services;

public class SchemaIntellisenseProvider(IQueryEditorContext ctx)
{
    private IEnumerable<SchemaLine> AllSchemaLines() => ctx.QueryEngineContext.GetSchema();


    private string[] TablesForCommand(string command) =>
        AllSchemaLines().Where(s => s.Command == command)
            .Select(s => s.Table)
            .Distinct()
            .ToArray();

    private string[] AllCommands() =>
        //it's important to order by length so that the longest commands
        //are matched first since the "dynamic" command have an empty string
        //as the command
        AllSchemaLines().Select(s => s.Command).Distinct()
            .OrderByDescending(s => s.Length)
            .ToArray();

    public IntellisenseEntry[] GetTables(string blockText)
    {
        foreach (var command in AllCommands())
            if (blockText.Contains(command))
                return TablesForCommand(command)
                    .Select(t => new IntellisenseEntry(t, $"{command} table","",IntellisenseHint.Table))
                    .ToArray();

        //no command found so return empty
        return [];
    }

    public IntellisenseEntry[] GetColumns(string blockText)
    {
        foreach (var command in AllCommands())
            if (blockText.Contains(command))
            {
                var tables = TablesForCommand(command);
                //TODO this is a bit fuzzy since short table names could be substrings
                //of longer ones or even keywords.  We should probably use a regex
                var matchingTables = tables.Where(blockText.Contains).ToArray();
                var columns = AllSchemaLines().Where(s => s.Command == command)
                    .Where(s => matchingTables.Contains(s.Table))
                    .Select(c => new IntellisenseEntry(c.Column, $"{c.Command} column for {c.Table}","", IntellisenseHint.Column))
                    .ToArray();
                return columns;
            }

        //no command found so return empty
        return [];
    }
}
