using KustoLoco.Core.Settings;
using Lokql.Engine;

namespace lokqlDxComponents.Services;

public interface IIntellisenseResourceManager : IIntellisenseResourceProvider
{
    void AddSettingsForIntellisense(KustoSettingsProvider settings);
    void SetSchema(SchemaLine[] getSchema);
}
