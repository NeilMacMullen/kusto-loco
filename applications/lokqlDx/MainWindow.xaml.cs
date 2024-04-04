using System.ComponentModel;
using System.Data;
using System.Windows;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Rendering;

namespace lokqlDx;

public partial class MainWindow : Window
{
    private readonly WpfConsole _console;
    private InteractiveTableExplorer _explorer;
    private readonly PreferencesManager _preferenceManager = new();
    private readonly WorkspaceManager _workspaceManager = new();

    public MainWindow()
    {
        InitializeComponent();
        _preferenceManager.Load();
        _workspaceManager.Load(_preferenceManager.Preferences.LastWorkspacePath);

        _console = new WpfConsole(OutputText);
        _explorer = new InteractiveTableExplorer(_console, new InteractiveTableExplorer.FolderContext(
            string.Empty, string.Empty, string.Empty));
    }


    private async Task RunQuery(string query)
    {
        //start capturing console output from the engine
        _console.PrepareForOutput();
        //run the supplied lines of kusto/commands
        var result = await _explorer.RunInput(query, false);
        _console.RenderTextBufferToWpfControl();

        //if there are no results leave the previously rendered results in place
        if (result.Height == 0)
            return;

        if (result.Visualization != VisualizationState.Empty)
        {
            //annoying we have to do this, but it's the only way to get the webview to render
            await webview.EnsureCoreWebView2Async();
            //generate the html and display it
            var html = KustoResultRenderer.RenderToHtml(result);
            webview.NavigateToString(html);
        }

        FillInDataGrid(result);
    }

    private void FillInDataGrid(KustoQueryResult result)
    {
        var maxDataGridRows = int.TryParse(VisibleDataGridRows.Text, out var parsed) ? parsed : 100;
        var dt = result.ToDataTable(maxDataGridRows);
        dataGrid.ItemsSource = dt.DefaultView;
    }

    /// <summary>
    ///     Called when user presses CTRL-ENTER in the query editor
    /// </summary>
    private async void OnQueryEditorRunTextBlock(object? sender, QueryEditorRunEventArgs eventArgs)
    {
        await RunQuery(eventArgs.Query);
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var workspace = _workspaceManager.workspace;
        Editor.SetText(workspace.Text);
        var context =
            new InteractiveTableExplorer.FolderContext(
                workspace.WorkingDirectory,
                workspace.WorkingDirectory,
                workspace.WorkingDirectory);

        _explorer = new InteractiveTableExplorer(_console, context);
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        PreferencesManager.EnsureDefaultFolderExists();
        var workspace = _workspaceManager.workspace;
        workspace.Text = Editor.GetText();
        _workspaceManager.Save(workspace);
        _preferenceManager.Preferences.LastWorkspacePath = _workspaceManager._path;
        _preferenceManager.Save();
    }
}