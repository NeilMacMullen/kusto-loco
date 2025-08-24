using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using KustoLoco.Core.Console;

namespace LokqlDx.ViewModels;

public interface IDocumentShow
{
    void Show(QueryDocumentViewModel model);
}
public partial class QueryLibraryViewModel : Tool,INotifyPropertyChanged
{
    private  IDocumentShow? _show;
    [ObservableProperty] private ObservableCollection<QueryDocumentViewModel> _queries = [];

    public QueryLibraryViewModel(DisplayPreferencesViewModel displayPreferencesPreferences)
    {
        
        Title = "Queries";
        CanClose = false;
        MaxWidth = 200;
    }

    public void SetShower(IDocumentShow show) => _show = show;
    public void Sort()
    {
        var sorted = Queries.OrderBy(q => !q.Visible).ThenBy(q => q.Title);
        Queries = new(sorted);
    }
    public void Add(QueryDocumentViewModel model)
    {
        Queries.Add(model);
        Sort();
    }

    public void Clear()
    {
        Queries.Clear();
    }

    public bool IsDirty() => Queries.Any(q => q.QueryViewModel.IsDirty());

    public void ClearDirty()
    {
        foreach (var queryItemViewModel in Queries) queryItemViewModel.QueryViewModel.Clean();
    }

    public PersistedQuery[] Persist()
    {
        return Queries.Select(q => new PersistedQuery(q.Title, q.QueryViewModel.GetText()))
            .ToArray();
    }

    [ObservableProperty] private bool _editLocked=true;

    
    [RelayCommand]
    public void ToggleEdit()
    {
        EditLocked =!EditLocked;
    }
    [RelayCommand]
    public void Show(QueryDocumentViewModel query)
    {
        _show?.Show(query);
    }
    public void ChangeVisibilty(QueryDocumentViewModel query, bool b)
    {
        query.Visible = b;
        Sort();
    }
}


   
