using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public partial class QueryDocumentViewModel : Document, INotifyPropertyChanged
{
    private readonly bool _initialized;
    [ObservableProperty] public bool _editLocked = true;
    [ObservableProperty] private bool _isDeleted;

    [ObservableProperty] private bool _isVisible = true;

    public QueryDocumentViewModel(string title, QueryViewModel content)
    {
        Title = title;
        QueryViewModel = content;
        Messaging.RegisterForValue<TabChangedMessage, QueryDocumentViewModel>(this, ActiveDocumentChanged);
        QueryViewModel.Name = Title;
        _initialized = true;
    }

    public QueryViewModel QueryViewModel { get; }

    private void ActiveDocumentChanged(QueryDocumentViewModel model) => QueryViewModel.IsActive = model == this;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (_initialized && e.PropertyName == nameof(Title)) QueryViewModel.Name = Title;
        base.OnPropertyChanged(e);
    }
}
