using Intellisense;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using lokqlDxComponents;
using lokqlDxComponents.Services;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponentsTests;

public class MockResourceProvider : ICompletionManagerServiceLocator, IIntellisenseResourceManager
{
    public IntellisenseEntry[] InternalCommands { get; set; } = [];
    public IntellisenseEntry[] KqlFunctionEntries { get; set; } = [];
    public IntellisenseEntry[] SettingNames { get; set; } = [];
    public IntellisenseEntry[] KqlOperatorEntries { get; set; } = [];
    public IntellisenseEntry[] GetTables(string blockText) => _schemaIntellisenseProvider.GetTables(blockText);
    public IntellisenseEntry[] GetColumns(string blockText) => _schemaIntellisenseProvider.GetColumns(blockText);

    public required IntellisenseClientAdapter _intellisenseClient { get; set; }
    public required SchemaIntellisenseProvider _schemaIntellisenseProvider { get; set; }
    public void AddSettingsForIntellisense(KustoSettingsProvider settings) => throw new NotImplementedException();

    public void SetSchema(SchemaLine[] getSchema) => _schemaIntellisenseProvider.SetSchema(getSchema);
}
