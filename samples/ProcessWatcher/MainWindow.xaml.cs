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
            var processes = Process.GetProcesses()
                .Select(p => new ProcessDetail(now, p.ProcessName, p.Threads.Count))
                .ToArray();
            ProcessList = ImmutableArray<ProcessDetail>.Empty.AddRange(processes);
        }

        private async void Go(object sender, RoutedEventArgs e)
        {
            await webview.EnsureCoreWebView2Async();
            var c = new KustoQueryContext();

            c.AddTableFromRecords("p", ProcessList);
            var result = await c.RunQuery(Query.Text);

            var html = KustoResultRenderer.RenderToHmtl("title", result);

            webview.NavigateToString(html);
            dataGrid.AutoGenerateColumns = true;
            var dt = new DataTable();
            dt.Columns.Add("test");
            dt.Rows.Add(1);
            dataGrid.ItemsSource = dt.Rows;
        }
    }

    public readonly record struct ProcessDetail(DateTime Time, string Name, int Threads);
}