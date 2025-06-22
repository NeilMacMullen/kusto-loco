using CommunityToolkit.Mvvm.ComponentModel;
using LokqlDx.ViewModels;

namespace LokqlDx;

/// <summary>
///     A Workspace is the query text and settings for a user's session.
/// </summary>
public partial class Workspace : ObservableObject
{
    [ObservableProperty] string _text = "";
    [ObservableProperty] string _startupScript = "";
    [ObservableProperty] PersistedQuery [] _queries =[];
    [ObservableProperty] bool _isDirty = false;

    partial void OnStartupScriptChanged(string value) => IsDirty = true;
}
