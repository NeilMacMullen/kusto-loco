using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class ToolManager : Tool
{
    public ConsoleDocumentViewModel Console;
    public readonly DisplayPreferencesViewModel DisplayPreferences;
    public readonly PinnedResultsViewModel PinnedResults;
    public QueryLibraryViewModel LibraryViewModel;
    public SchemaViewModel SchemaViewModel;

    public ToolManager(DisplayPreferencesViewModel displayPreferences, ConsoleViewModel console)
    {
        DisplayPreferences = displayPreferences;
        LibraryViewModel = new QueryLibraryViewModel();
        SchemaViewModel = new SchemaViewModel();
        Console = new ConsoleDocumentViewModel(console);
        PinnedResults=new PinnedResultsViewModel();
    }
}
