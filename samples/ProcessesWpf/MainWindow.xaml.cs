using System.Collections.Immutable;
using System.Diagnostics;
using System.Windows;
using KustoLoco.Core;

namespace ProcessesWpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnRunButtonClicked(object sender, RoutedEventArgs e)
        {
            //get data into context
            var context = new KustoQueryContext();

            var processes = Process.GetProcesses()
                .Select(p => new ProcessInfo(Pid:p.Id,
                                             Name:p.ProcessName,
                                             NumThreads:p.Threads.Count,
                                             WorkingSet:p.WorkingSet64))
                .ToImmutableArray();

            context.WrapDataIntoTable("processes", processes);

            //run query
            var query = QueryEntry.Text.Trim();
            var result = await context.RunQuery(query);

            //render results
            ResultDataGrid.ItemsSource = result.ToDataTableOrError().DefaultView;
        }
    }
}

public readonly record struct ProcessInfo(int Pid, string Name, int NumThreads, long WorkingSet);
