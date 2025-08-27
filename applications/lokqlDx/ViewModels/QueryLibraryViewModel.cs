using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using NotNullStrings;

namespace LokqlDx.ViewModels;


public partial class QueryLibraryViewModel : LokqlTool
{
    [ObservableProperty] private string _filter = string.Empty;
    [ObservableProperty] private ObservableCollection<QueryDocumentViewModel> _filteredQueries = [];
    [ObservableProperty] private ObservableCollection<QueryDocumentViewModel> _queries = [];
    [ObservableProperty] private bool _searchQueryBody;
  
    public QueryLibraryViewModel()
    {
        Title = "Queries";
    }

    public bool DeletedItemsShown => FilteredQueries.Any(q => q.IsDeleted);

    [RelayCommand]
    public void FilterChanged() => Sort();

    public void Sort()
    {
        var sorted = Queries
            .Where(ApplyFilter)
            .OrderBy(q => q.IsDeleted)
            .ThenBy(q => !q.IsVisible)
            .ThenBy(q => q.Title);
        FilteredQueries = new ObservableCollection<QueryDocumentViewModel>(sorted);
        OnPropertyChanged(nameof(DeletedItemsShown));
    }

    private bool ApplyFilter(QueryDocumentViewModel arg)
    {
        var filterTokens = Filter.Tokenize(" ");
        if (!filterTokens.Any())
            return true;

        var argStr = $"{arg.Title}";
        if (SearchQueryBody)
            argStr += arg.QueryViewModel.GetText();
        return filterTokens.All(t => argStr.Contains(t, StringComparison.InvariantCultureIgnoreCase));
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

    public PersistedQuery[] Persist() =>
        Queries
            .Where(q => !q.IsDeleted)
            .Select(q => new PersistedQuery(q.Title, q.QueryViewModel.GetText(), !q.IsVisible))
            .ToArray();

    partial void OnSearchQueryBodyChanged(bool value) => Sort();

    [RelayCommand]
    public void ToggleEdit(QueryDocumentViewModel query) => query.EditLocked = !query.EditLocked;

    [RelayCommand]
    public void Show(QueryDocumentViewModel query) => Messaging.Send(new ShowQueryRequestMessage(query));

    public void ChangeVisibility(QueryDocumentViewModel query, bool b)
    {
        query.IsVisible = b;
        Sort();
    }

    [RelayCommand]
    public void FilterEnter(QueryDocumentViewModel query) => query.EditLocked = true;

    [RelayCommand]
    public void ToggleDelete(QueryDocumentViewModel query)
    {
        query.IsDeleted = !query.IsDeleted;
        Sort();
    }

    [RelayCommand]
    public void EmptyTrash()
    {
        Queries = new ObservableCollection<QueryDocumentViewModel>(Queries.Where(q => !q.IsDeleted));
        Sort();
    }
}
