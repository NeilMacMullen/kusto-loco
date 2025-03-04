using KustoLoco.Core;
using ProcessesWpf.ViewModels.Pages;
using System.Collections.Immutable;
using System.Diagnostics;
using Wpf.Ui.Abstractions.Controls;

namespace ProcessesWpf.Views.Pages;
public partial class DashboardPage : INavigableView<DashboardViewModel>
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    private async void OnRunButtonClicked(object sender, RoutedEventArgs e)
    {
        //get data into context
        var context = new KustoQueryContext();

        var processes = Process.GetProcesses()
            .Select(p => new ProcessInfo(Pid: p.Id,
                                         Name: p.ProcessName,
                                         NumThreads: p.Threads.Count,
                                         WorkingSet: p.WorkingSet64))
            .ToImmutableArray();

        context.WrapDataIntoTable("processes", processes);

        //run query
        var query = QueryEntry.Text.Trim();
        var result = await context.RunQuery(query);

        //render results
        ResultDataGrid.ItemsSource = result.ToDataTableOrError().DefaultView;
    }
}
