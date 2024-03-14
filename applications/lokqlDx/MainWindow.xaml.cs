using System.Data;
using System.Windows;
using KustoLoco.Core.Evaluation;
using KustoLoco.Rendering;
using KustoLoco.Core;

namespace lokqlDx;

public partial class MainWindow : Window
{
    private readonly WpfConsole _console;
    private readonly InteractiveTableExplorer explorer;

    public MainWindow()
    {
        InitializeComponent();

        var workin = @"C:\kusto";
        var context =
            new InteractiveTableExplorer.FolderContext(
                workin,
                workin,
                workin);
        _console = new WpfConsole(OutputText);
        explorer = new InteractiveTableExplorer(_console, context);
    }


    private async Task RunQuery(string query)
    {
        //start capturing console output from the engine
        _console.PrepareForOutput();
        //run the supplied lines of kusto/commands
        var result = await explorer.RunInput(query, false);
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

        var dt = new DataTable();

        foreach (var col in result.ColumnNames())
            dt.Columns.Add(col);

        foreach (var row in result.EnumerateRows().Take(maxDataGridRows))
            dt.Rows.Add(row);

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
        Editor.SetText(@"
# move the cursor over a block of lines and press CTRL-ENTER to run
# commands prefixed with '.' are special commands.  Use .help  to list

.help 

# loads a CSV file into a table called 'data'

.load c:\data\mydata.csv data

# gets the distribution of values in the 'Name' column

data 
| summarize count() by Name 
| render barchart


");
    }
}