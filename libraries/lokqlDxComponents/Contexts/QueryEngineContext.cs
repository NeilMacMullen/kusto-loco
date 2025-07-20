using KustoLoco.Core.Settings;
using Lokql.Engine;

namespace lokqlDxComponents.Contexts;

public interface IQueryEngineContext
{
    IEnumerable<SchemaLine> GetSchema();
    IEnumerable<RawKustoSetting> GetSettings();
    IEnumerable<VerbEntry> GetVerbs();
}


public class QueryEngineContext(InteractiveTableExplorer tableExplorer) : IQueryEngineContext
{
    public IEnumerable<SchemaLine> GetSchema() => tableExplorer.GetSchema();

    public IEnumerable<RawKustoSetting> GetSettings() => tableExplorer.Settings.Enumerate();

    public IEnumerable<VerbEntry> GetVerbs() => tableExplorer._commandProcessor.GetVerbs(tableExplorer._loader);
}
