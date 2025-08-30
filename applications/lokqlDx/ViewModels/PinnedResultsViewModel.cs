using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using KustoLoco.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace LokqlDx.ViewModels;

public partial class PinnedResultsViewModel : LokqlTool
{
    [ObservableProperty] private ObservableCollection<PinnedKustoResult> _results = [];

    public PinnedResultsViewModel()
    {
        Title = "Results";
     
        Messaging.RegisterForValue<PinResultMessage, QueryResultWithSender>(this, ReceivePinMesssage);
    }

    private void ReceivePinMesssage(QueryResultWithSender val)
    {
        var named = new PinnedKustoResult(val);
        Results.Add(named);
        if (val.ImmediateDisplay)
        {
           Messaging.Send(new DisplayResultMessage(named));
        }
    }

    [RelayCommand]
    public void ToggleEdit(PinnedKustoResult result) => result.EditLocked = !result.EditLocked;

    [RelayCommand]
    public void ToggleDelete(PinnedKustoResult result) => Results.Remove(result);

    [RelayCommand]
    public void Show(PinnedKustoResult result) =>Messaging.Send(new DisplayResultMessage(result));

    [RelayCommand]
    public void FilterEnter(PinnedKustoResult result) => result.EditLocked = true;
}

public partial class PinnedKustoResult : ObservableObject
{
    [ObservableProperty] private DateTime _created = DateTime.MinValue;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private bool _editLocked = true;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private KustoQueryResult _result = KustoQueryResult.Empty;

    public PinnedKustoResult(QueryResultWithSender result)
    {
        Result = result.Result;
        Name = result.Sender + $" - {DateTime.Now:HH:mm:ss}";
        Created = DateTime.Now;
        Description = $"Rows:{Result.RowCount} Columns:{Result.ColumnCount}";
    }
}
