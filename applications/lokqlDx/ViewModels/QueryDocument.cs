using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace LokqlDx.ViewModels;

public partial class QueryDocument : ObservableObject
{
    [ObservableProperty] private string _header;

    [ObservableProperty] private QueryViewModel _queryViewModel;

    public QueryDocument(string header, QueryViewModel content)
    {
        _header = header;
        _queryViewModel = content;
        WeakReferenceMessenger.Default.Register<TabChangedMessage>(this, (r, m) =>
        {
            _queryViewModel.IsActive = m.Value == this;
        });
    }
}
