using CommunityToolkit.Mvvm.ComponentModel;

namespace LokqlDx;

/// <summary>
///     A Workspace is the query text and settings for a user's session.
/// </summary>
public partial class Workspace : ObservableObject
{
    [ObservableProperty] string _text = "";
    [ObservableProperty] string _startupScript = "";
    [ObservableProperty] bool _isDirty = false;

    partial void OnTextChanged(string value) => IsDirty = true;
    partial void OnStartupScriptChanged(string value) => IsDirty = true;
}
