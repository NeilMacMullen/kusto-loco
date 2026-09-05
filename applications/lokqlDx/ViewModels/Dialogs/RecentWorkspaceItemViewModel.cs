using CommunityToolkit.Mvvm.ComponentModel;
using NotNullStrings;

namespace LokqlDx.ViewModels.Dialogs;

/// <summary>
///     A single entry in the "search recent workspaces" dialog
/// </summary>
public partial class RecentWorkspaceItemViewModel : ObservableObject
{
    [ObservableProperty] private string _description;
    [ObservableProperty] private bool _isPinned;

    public RecentWorkspaceItemViewModel(MruEntry entry)
    {
        Path = entry.Path;
        Name = entry.Name.OrWhenBlank(System.IO.Path.GetFileNameWithoutExtension(entry.Path));
        LastAccessed = entry.LastAccessed;
        _description = entry.Description;
        _isPinned = entry.IsPinned;
    }

    public string Path { get; }
    public string Name { get; }
    public DateTime LastAccessed { get; }

    public string LastAccessedDisplay =>
        LastAccessed == DateTime.MinValue
            ? "never"
            : LastAccessed.ToString("g");

    public string PinGlyph => IsPinned ? "📌" : "📍";

    /// <summary>
    ///     True if _all_ the supplied tokens appear in the name or description
    /// </summary>
    public bool Matches(string[] tokens) =>
        tokens.All(t =>
            Name.Contains(t, StringComparison.OrdinalIgnoreCase) ||
            Description.Contains(t, StringComparison.OrdinalIgnoreCase));

    partial void OnIsPinnedChanged(bool value) => OnPropertyChanged(nameof(PinGlyph));
}
