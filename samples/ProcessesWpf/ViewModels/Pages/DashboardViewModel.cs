using KustoLoco.Core;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Data;
using Wpf.Ui.Abstractions.Controls;

namespace ProcessesWpf.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private string _query = string.Empty;
    [ObservableProperty]
    private ObservableCollection<Object> _resultData = [];

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
    public Task OnNavigatedToAsync() => Task.CompletedTask;

    [RelayCommand]
    private async Task RunAsync()
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
        var query = Query.Trim();
        var result = await context.RunQuery(query);

        //render results
        foreach(var item in result.ToDataTableOrError().DefaultView)
        {
            ResultData.Add(item);
        }
    }
}
