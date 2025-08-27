using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class ToolManager : Tool
{
    public readonly ConsoleViewModel _console;
    public readonly DisplayPreferencesViewModel _displayPreferences;

    public QueryLibraryViewModel _libraryViewModel;
    public SchemaViewModel _schemaViewModel;

    public ToolManager(DisplayPreferencesViewModel displayPreferences, ConsoleViewModel console)
    {
        _displayPreferences = displayPreferences;
        _console = console;
        _libraryViewModel = new QueryLibraryViewModel();
        _schemaViewModel = new SchemaViewModel();
    }
}
