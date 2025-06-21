using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using KustoLoco.Core;
using Wpf.Ui.Abstractions.Controls;

namespace ProcessesWpf.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private string _query = @"processes
| summarize TotalThreads=sum(NumThreads) by Name
| order by TotalThreads";

    [ObservableProperty] private ObservableCollection<object> _resultData = [];
    [ObservableProperty] private KustoQueryResult _result = KustoQueryResult.Empty;

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnNavigatedToAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        //get data into context
        var context = new KustoQueryContext();

        var processes = Process.GetProcesses()
            .Select(p => new ProcessInfo(p.Id,
                p.ProcessName,
                p.Threads.Count,
                p.WorkingSet64))
            .ToImmutableArray();

        context.WrapDataIntoTable("processes", processes);

        //run query
        var query = Query.Trim();
        Result = await context.RunQuery(query);

        //render results
        ResultData.Clear();
        foreach (var item in Result.ToDataTableOrError().DefaultView) ResultData.Add(item);
    }
}
