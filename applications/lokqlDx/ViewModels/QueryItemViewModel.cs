using CommunityToolkit.Mvvm.ComponentModel;

namespace LokqlDx.ViewModels;

public partial class QueryItemViewModel : ObservableObject
{
    [ObservableProperty] private string _header;

    [ObservableProperty] private QueryViewModel _queryModel;

    public QueryItemViewModel(string header, QueryViewModel content)
    {
        _header = header;
        _queryModel = content;
    }
}
