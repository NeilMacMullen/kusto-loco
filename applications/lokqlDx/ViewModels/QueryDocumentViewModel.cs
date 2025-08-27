using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using DocumentFormat.OpenXml.Linq;
using System.ComponentModel;

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
        Messaging.RegisterForValue<TabChangedMessage, QueryDocumentViewModel>(this, ActiveDocumentChanged);
        QueryViewModel.Name = Title;
        _initialized = true;
    }

    private void ActiveDocumentChanged(QueryDocumentViewModel model)
    {
        QueryViewModel.IsActive = model == this;
    }

    private bool _initialized;
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if ( _initialized && e.PropertyName == nameof(Title))
        {
            QueryViewModel.Name = Title;
        }
        base.OnPropertyChanged(e);
    }

}
