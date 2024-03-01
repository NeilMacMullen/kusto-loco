using System.Collections.Immutable;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BabyKusto.Core.Evaluation;
using KustoSupport;

namespace ProcessWatcher;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ReportExplorer explorer;
    private readonly EmbeddedConsole _console;

    public MainWindow()
    {
        InitializeComponent();

        var workin = @"C:\kusto";
        var context =
            new ReportExplorer.FolderContext(
                workin,
                workin,
                workin);
        _console = new EmbeddedConsole(OutputText);
        explorer = new ReportExplorer(_console,context);
    }

    private async Task RunQuery()
    {
        await webview.EnsureCoreWebView2Async();
        var c = new KustoQueryContext();
        var i = Query.GetLineIndexFromCharacterIndex(Query.CaretIndex);
        var sb = new StringBuilder();
        while (i >= 1 && Query.GetLineText(i - 1).Trim().Length > 0)
            i--;
        while (i < Query.LineCount && Query.GetLineText(i).Trim().Length > 0)
        {
            sb.AppendLine(Query.GetLineText(i));
            i++;
        }

        //var result = c.RunTabularQueryWithoutDemandBasedTableLoading(sb.ToString());
        _console.Init();

        var result = await explorer.RunInput(sb.ToString(), false);
        _console.RenderToRichTextBox();

        if (result.Height > 0)
        {
            if (result.Visualization != VisualizationState.Empty)
            {
                var html = KustoResultRenderer.RenderToHtml(result);

                webview.NavigateToString(html);
            }

            FillInDataGrid(result);
        }
    }

    private void Go(object sender, RoutedEventArgs e)
    {
    }

    private void FillInDataGrid(KustoQueryResult result)
    {
        var maxDataGrid = 100;
        var dt = new DataTable();

        foreach (var col in result.ColumnNames())
            dt.Columns.Add(col);

        foreach (var row in result.EnumerateRows().Take(maxDataGrid))
            dt.Rows.Add(row);

        dataGrid.ItemsSource = dt.DefaultView;
    }

    private async void Query_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                e.Handled = true;
                await RunQuery();
            }
        }
    }
}