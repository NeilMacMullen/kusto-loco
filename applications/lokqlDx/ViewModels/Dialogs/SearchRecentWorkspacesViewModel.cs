using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotNullStrings;

namespace LokqlDx.ViewModels.Dialogs;

public partial class SearchRecentWorkspacesViewModel : ObservableObject, IDialogViewModel
{
    private readonly List<RecentWorkspaceItemViewModel> _allItems;
    private readonly TaskCompletionSource _completionSource;
    private readonly PreferencesManager _preferencesManager;
    [ObservableProperty] private string _filterText = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private RecentWorkspaceItemViewModel? _selectedItem;

    public SearchRecentWorkspacesViewModel(PreferencesManager preferencesManager)
    {
        _preferencesManager = preferencesManager;
        _allItems = preferencesManager.GetMruEntries()
            .Select(e => new RecentWorkspaceItemViewModel(e))
            .ToList();
        Items = [];
        ApplyFilter();
        _completionSource = new TaskCompletionSource();
        Result = _completionSource.Task;
    }

    public ObservableCollection<RecentWorkspaceItemViewModel> Items { get; }

    /// <summary>
    ///     The path of the workspace the user chose, or empty if they cancelled
    /// </summary>
    public string SelectedPath { get; private set; } = string.Empty;

    public Task Result { get; }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var tokens = FilterText.Tokenize();
        var selected = SelectedItem;
        Items.Clear();
        foreach (var item in Sorted(_allItems.Where(i => i.Matches(tokens))))
            Items.Add(item);
        SelectedItem = selected is not null && Items.Contains(selected)
            ? selected
            : Items.FirstOrDefault();
    }

    private static IEnumerable<RecentWorkspaceItemViewModel> Sorted(
        IEnumerable<RecentWorkspaceItemViewModel> items) =>
        items.OrderByDescending(i => i.IsPinned)
            .ThenByDescending(i => i.LastAccessed);

    private bool CanLoad() => SelectedItem is not null;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private void Load()
    {
        if (SelectedItem is null)
            return;
        SelectedPath = SelectedItem.Path;
        _completionSource.TrySetResult();
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedPath = string.Empty;
        _completionSource.TrySetResult();
    }

    [RelayCommand]
    private void Remove(RecentWorkspaceItemViewModel item)
    {
        _preferencesManager.RemoveFromMruList(item.Path);
        _allItems.Remove(item);
        Items.Remove(item);
        SelectedItem ??= Items.FirstOrDefault();
    }

    [RelayCommand]
    private void TogglePin(RecentWorkspaceItemViewModel item)
    {
        item.IsPinned = !item.IsPinned;
        _preferencesManager.SetPinned(item.Path, item.IsPinned);
        ApplyFilter();
    }
}
