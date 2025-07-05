using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LokqlDx.ViewModels;

public partial class DisplayPreferencesViewModel : ObservableObject
{
    [ObservableProperty] private FontFamily _fontFamily = new("Consolas");
    [ObservableProperty] private double _fontSize = 20;
    [ObservableProperty] private bool _showLineNumbers;
    [ObservableProperty] private bool _wordWrap;
}
