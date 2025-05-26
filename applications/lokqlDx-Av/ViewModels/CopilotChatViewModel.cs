using CommunityToolkit.Mvvm.ComponentModel;

namespace LokqlDx.ViewModels;

public partial class CopilotChatViewModel : ObservableObject
{
    [ObservableProperty] private double _fontSize = 20;

    internal void SetUiPreferences(UIPreferences uiPreferences) => FontSize = uiPreferences.FontSize;
}
