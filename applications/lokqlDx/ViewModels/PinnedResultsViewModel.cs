using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using KustoLoco.Core;
using Lokql.Engine;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class PinnedResultsViewModel : Tool, INotifyPropertyChanged
{

    [ObservableProperty] private ObservableCollection<NamedKustoResult> _results = [];

    public PinnedResultsViewModel()
    {
        Title = "Results";
        CanClose = false;

        WeakReferenceMessenger.Default.Register<PinResultMessage>(this,
            Process);
    }

    private void Process(object? sender, PinResultMessage message)
    {
        Results.Add(new NamedKustoResult(message.Value));
    }

    [RelayCommand]
    public void ToggleEdit(NamedKustoResult result)
    {
        result.EditLocked = !result.EditLocked;
    }

    [RelayCommand]
    public void ToggleDelete(NamedKustoResult result)
    {
        Results.Remove(result);
    }

    [RelayCommand]
    public void Show(NamedKustoResult result)
    {
        WeakReferenceMessenger.Default.Send(new DisplayResultMessage(result.Result));
    }

}

public partial class NamedKustoResult : ObservableObject
{
    [ObservableProperty] private string _name=String.Empty;
    [ObservableProperty] private KustoQueryResult _result=KustoQueryResult.Empty;
    [ObservableProperty] private DateTime _created=DateTime.MinValue;
    [ObservableProperty] private bool _editLocked = true;
    public NamedKustoResult(KustoQueryResult result)
    {
        Result = result;
        Name = DateTime.Now.ToString("HH:mm:ss");
        Created = DateTime.Now;
    }
}
