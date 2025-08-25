using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public partial class QueryDocumentViewModel : Document,INotifyPropertyChanged
{

    public QueryViewModel QueryViewModel { get; private set; }
    [ObservableProperty] public bool _editLocked = true;

    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isDeleted = false;
    public QueryDocumentViewModel(string title, QueryViewModel content)
    {
        Title = title;
        QueryViewModel = content;
        WeakReferenceMessenger.Default.Register<TabChangedMessage>(this, (r, m) =>
        {
            QueryViewModel.IsActive = m.Value == this;
        });
    }
}
