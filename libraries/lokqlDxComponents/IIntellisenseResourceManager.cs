using KustoLoco.Core.Settings;
using Lokql.Engine;

namespace lokqlDxComponents;

public interface IIntellisenseResourceManager : IIntellisenseResourceProvider
{
    void AddSettingsForIntellisense(KustoSettingsProvider settings);
    void AddInternalCommands(IEnumerable<VerbEntry> verbEntries);
    void SetSchema(SchemaLine[] getSchema);
}
