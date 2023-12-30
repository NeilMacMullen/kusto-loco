using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using KustoSupport;

namespace ProcessWatcher
{
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
                    Time: now,
                    Name: p.ProcessName,
                    Threads: p.Threads.Count,
                    Id: p.Id,
                    WorkingSet: p.WorkingSet64,
                    Handles: p.HandleCount
                ))
                .ToImmutableArray();

            ProcessHistory =
                ProcessHistory.AddRange(ProcessList);

            var max = 100000;
            if (ProcessHistory.Length > max)
                ProcessHistory = ProcessHistory.TakeLast(max).ToImmutableArray();
        }

        private async void Go(object sender, RoutedEventArgs e)
        {
            await webview.EnsureCoreWebView2Async();
            var c = new KustoQueryContext();
            c.AddTableFromRecords("p", ProcessList);
            c.AddTableFromRecords("h", ProcessHistory);
            var result = c.RunTabularQuery(Query.Text);

            var html = KustoResultRenderer.RenderToHmtl("title", result);

            webview.NavigateToString(html);
            FillInDataGrid(result);
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
    }

    public readonly record struct ProcessDetail(
        DateTime Time,
        string Name,
        int Threads,
        int Id,
        long WorkingSet,
        int Handles);
}