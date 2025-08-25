using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using KustoLoco.Core.Console;
using Lokql.Engine;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Avalonia.Input;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public interface IDocumentShow
{
    void Show(QueryDocumentViewModel model);
}
public partial class QueryLibraryViewModel : Tool,INotifyPropertyChanged
{
    private  IDocumentShow? _show;

    public bool DeletedItemsShown => FilteredQueries.Any(q => q.IsDeleted);

    [ObservableProperty] private ObservableCollection<QueryDocumentViewModel> _queries = [];
    [ObservableProperty] private ObservableCollection<QueryDocumentViewModel> _filteredQueries = [];
    public QueryLibraryViewModel(DisplayPreferencesViewModel displayPreferencesPreferences)
    {
        
        Title = "Queries";
        CanClose = false;
    }
    [RelayCommand]
    public void FilterChanged()
    {
      Sort();
    }

    [ObservableProperty] private string _filter = string.Empty;
    public void SetShower(IDocumentShow show) => _show = show;
    public void Sort()
    {
        var sorted = Queries
            .Where(ApplyFilter)
            .OrderBy(q => q.IsDeleted).ThenBy(q=>!q.IsVisible).ThenBy(q => q.Title);
        FilteredQueries = new(sorted);
        OnPropertyChanged(nameof(DeletedItemsShown));
    }

    private bool ApplyFilter(QueryDocumentViewModel arg)
    {
        var argStr = $"{arg.Title}";
        var toks = Filter.Tokenize(" ");
        if (!toks.Any())
            return true;
        return toks.All(t => argStr.Contains(t, StringComparison.InvariantCultureIgnoreCase));
    }

    public void Add(QueryDocumentViewModel model)
    {
        Queries.Add(model);
        Sort();
    }

    public void Clear()
    {
        Queries.Clear();
        Sort();
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

    
    [RelayCommand]
    public void ToggleEdit(QueryDocumentViewModel query)
    {
        query.EditLocked =!query.EditLocked;
    }
    [RelayCommand]
    public void Show(QueryDocumentViewModel query)
    {
        _show?.Show(query);
    }
    public void ChangeVisibility(QueryDocumentViewModel query, bool b)
    {
        query.IsVisible = b;
        Sort();
    }

    [RelayCommand]
    public void FilterEnter(QueryDocumentViewModel query)
    {
        query.EditLocked = true;
    }

    [RelayCommand]
    public void ToggleDelete(QueryDocumentViewModel query)
    {
        query.IsDeleted = !query.IsDeleted;
        Sort();
    }

    [RelayCommand]
    public void EmptyTrash()
    {
        Queries= new ObservableCollection<QueryDocumentViewModel>(Queries.Where(q=>!q.IsDeleted));
        Sort();
    }
}


   
