using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        if (val.ImmediateDisplay) Messaging.Send(new DisplayResultMessage(named));
    }

    [RelayCommand]
    public void ToggleEdit(PinnedKustoResult result) => result.EditLocked = !result.EditLocked;

    [RelayCommand]
    public void ToggleDelete(PinnedKustoResult result) => Results.Remove(result);

    [RelayCommand]
    public void Show(PinnedKustoResult result) => Messaging.Send(new DisplayResultMessage(result));

    [RelayCommand]
    public void FilterEnter(PinnedKustoResult result) => result.EditLocked = true;
}
