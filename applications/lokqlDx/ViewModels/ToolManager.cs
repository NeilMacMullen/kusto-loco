using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class ToolManager : Tool
{
    private readonly ConsoleViewModel _console;
    public ConsoleDocumentViewModel Console;
    public readonly DisplayPreferencesViewModel _displayPreferences;
    public readonly PinnedResultsViewModel _pinnedResults;
    public QueryLibraryViewModel _libraryViewModel;
    public SchemaViewModel _schemaViewModel;

    public ToolManager(DisplayPreferencesViewModel displayPreferences, ConsoleViewModel console)
    {
        _displayPreferences = displayPreferences;
        _console = console;
        _libraryViewModel = new QueryLibraryViewModel();
        _schemaViewModel = new SchemaViewModel();
        Console = new ConsoleDocumentViewModel(_console);
        _pinnedResults=new PinnedResultsViewModel();
    }
}
