using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LokqlDx.ViewModels;

public partial class DisplayPreferencesViewModel : ObservableObject
{
    [ObservableProperty] private FontFamily _fontFamily=new FontFamily("Consolas");
    [ObservableProperty] private double _fontSize = 20;
    [ObservableProperty] private bool _wordWrap = false;
    [ObservableProperty] private bool _showLineNumbers = false;

}
