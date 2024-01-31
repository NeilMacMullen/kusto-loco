using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KustoSupport;

namespace ProcessWatcher;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherTimer timer = new();
    private ImmutableArray<ProcessDetail> ProcessHistory = ImmutableArray<ProcessDetail>.Empty;
    private ImmutableArray<ProcessDetail> ProcessList = ImmutableArray<ProcessDetail>.Empty;

    public MainWindow()
    {
        InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += TimerOnTick;
        timer.IsEnabled = true;
    }

    private void TimerOnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        ProcessList = Process.GetProcesses()
            .Select(p => new ProcessDetail(
                now,
                p.ProcessName,
                p.Threads.Count,
                p.Id,
                p.WorkingSet64,
                p.HandleCount
            ))
            .ToImmutableArray();

        ProcessHistory =
            ProcessHistory.AddRange(ProcessList);

        var max = 100000;
        if (ProcessHistory.Length > max)
            ProcessHistory = ProcessHistory.TakeLast(max).ToImmutableArray();
    }


    private async Task RunQuery()
    {
        await webview.EnsureCoreWebView2Async();
        var c = new KustoQueryContext();
        c.AddTableFromRecords("p", ProcessList);
        c.AddTableFromRecords("h", ProcessHistory);
        var i = Query.GetLineIndexFromCharacterIndex(Query.CaretIndex);
        var sb = new StringBuilder();
        while (i >= 1 && Query.GetLineText(i-1).Trim().Length > 0)
            i--;
        while (i < Query.LineCount && Query.GetLineText(i).Trim().Length > 0)
        {
            sb.AppendLine(Query.GetLineText(i));
            i++;
        }
            
        var result = c.RunTabularQueryWithoutDemandBasedTableLoading(sb.ToString());

        var html = KustoResultRenderer.RenderToHtml(result);

        webview.NavigateToString(html);
        FillInDataGrid(result);
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

public readonly record struct ProcessDetail(
    DateTime Time,
    string Name,
    int Threads,
    int Id,
    long WorkingSet,
    int Handles);