using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using KustoLoco.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace LokqlDx.ViewModels;

public partial class PinnedResultsViewModel : Tool, INotifyPropertyChanged
{
    [ObservableProperty] private ObservableCollection<NamedKustoResult> _results = [];

    public PinnedResultsViewModel()
    {
        Title = "Results";
        CanClose = false;

        WeakReferenceMessenger.Default.Register<PinResultMessage>(this,
            ReceivePinMesssage);
    }

    private void ReceivePinMesssage(object? sender, PinResultMessage message)
    {
        var named = new NamedKustoResult(message.Value);
        Results.Add(named);
        if (message.Value.ImmediateDisplay)
        {
            WeakReferenceMessenger.Default.Send(new DisplayResultMessage(named));
        }
    }

    [RelayCommand]
    public void ToggleEdit(NamedKustoResult result) => result.EditLocked = !result.EditLocked;

    [RelayCommand]
    public void ToggleDelete(NamedKustoResult result) => Results.Remove(result);

    [RelayCommand]
    public void Show(NamedKustoResult result) => WeakReferenceMessenger.Default.Send(new DisplayResultMessage(result));

    [RelayCommand]
    public void FilterEnter(NamedKustoResult result) => result.EditLocked = true;
}

public partial class NamedKustoResult : ObservableObject
{
    [ObservableProperty] private DateTime _created = DateTime.MinValue;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private bool _editLocked = true;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private KustoQueryResult _result = KustoQueryResult.Empty;

    public NamedKustoResult(QueryResultWithSender result)
    {
        Result = result.Result;
        Name = result.Sender + $" - {DateTime.Now:HH:mm:ss}";
        Created = DateTime.Now;
        Description = $"Rows:{Result.RowCount} Columns:{Result.ColumnCount}";
    }
}
