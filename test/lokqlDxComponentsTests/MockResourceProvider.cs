using Intellisense;
using lokqlDxComponents;
using lokqlDxComponents.Services;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponentsTests;

public class MockResourceProvider : ICompletionManagerServiceLocator
{
    public IntellisenseEntry[] InternalCommands { get; set; } = [];
    public IntellisenseEntry[] KqlFunctionEntries { get; set; } = [];
    public IntellisenseEntry[] SettingNames { get; set; } = [];
    public IntellisenseEntry[] KqlOperatorEntries { get; set; } = [];
    public IntellisenseEntry[] GetTables(string blockText) => throw new NotImplementedException();

    public IntellisenseEntry[] GetColumns(string blockText) => throw new NotImplementedException();

    public required IntellisenseClientAdapter _intellisenseClient { get; set; }
}
