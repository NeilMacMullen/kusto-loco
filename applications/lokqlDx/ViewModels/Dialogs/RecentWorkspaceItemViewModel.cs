using CommunityToolkit.Mvvm.ComponentModel;
using NotNullStrings;

namespace LokqlDx.ViewModels.Dialogs;

/// <summary>
///     A single entry in the "search recent workspaces" dialog
/// </summary>
public partial class RecentWorkspaceItemViewModel : ObservableObject
{
    [ObservableProperty] private string _description;

    [ObservableProperty] private bool _hasDescription;
    [ObservableProperty] private bool _isPinned;

    public RecentWorkspaceItemViewModel(MruEntry entry)
    {
        Path = entry.Path;
        Name = entry.Name.OrWhenBlank(System.IO.Path.GetFileNameWithoutExtension(entry.Path));
        LastAccessed = entry.LastAccessed;
        _description = entry.Description;
        _hasDescription = _description.IsNotBlank();
        _isPinned = entry.IsPinned;
    }

    public string Path { get; }
    public string Name { get; }

    public DateTime LastAccessed { get; }

    public string LastAccessedDisplay =>
        LastAccessed == DateTime.MinValue
            ? "never"
            : LastAccessed.ToString("ddd dd MMM yyyy HH:mm");

    public string PinGlyph => IsPinned ? "📌" : "📍";

    /// <summary>
    ///     True if _all_ the supplied tokens appear in the name or description
    /// </summary>
    public bool Matches(string[] tokens) =>
        tokens.All(t => (Name+Description+LastAccessed+Path).Contains(t, StringComparison.OrdinalIgnoreCase));

    partial void OnIsPinnedChanged(bool value) => OnPropertyChanged(nameof(PinGlyph));
}
