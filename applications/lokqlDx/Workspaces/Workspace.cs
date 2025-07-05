using CommunityToolkit.Mvvm.ComponentModel;
using LokqlDx.ViewModels;

namespace LokqlDx;

/// <summary>
///     A Workspace is the query text and settings for a user's session.
/// </summary>
public partial class Workspace : ObservableObject
{
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private PersistedQuery[] _queries = [];
    [ObservableProperty] private string _startupScript = "";
    [ObservableProperty] private string _text = "";

    partial void OnStartupScriptChanged(string value) => IsDirty = true;
}
