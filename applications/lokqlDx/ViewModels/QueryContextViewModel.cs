using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vanara.PInvoke;

namespace LokqlDx.ViewModels;

public partial class QueryContextViewModel : ObservableObject
{
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private string _text = "";

    [ObservableProperty] private bool _isDirty;


    [RelayCommand]
    public void ToggleExpand() => IsExpanded = !IsExpanded;

    partial void OnTextChanged(string value)
    {
        IsDirty = true;
    }
}
