using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace LokqlDx.ViewModels;

public partial class QueryItemViewModel : ObservableObject
{
    [ObservableProperty] private string _header;

    [ObservableProperty] private QueryViewModel _queryModel;

    public QueryItemViewModel(string header, QueryViewModel content)
    {
        _header = header;
        _queryModel = content;
        WeakReferenceMessenger.Default.Register<TabChangedMessage>(this, (r, m) =>
        {
            _queryModel.IsActive = m.Value == this;
        });
    }
}
