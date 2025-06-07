using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using KustoLoco.Core.Console;

namespace LokqlDx.ViewModels;

public partial class ConsoleViewModel : ObservableObject, IKustoConsole
{
    [ObservableProperty] private ObservableCollection<ColoredText> _consoleContent = [];
    [ObservableProperty] private double _consoleWidth;
    [ObservableProperty] private FontFamily? _fontFamily;
    [ObservableProperty] private double _fontSize = 20;
    [ObservableProperty] private ColoredText? _selectedItem;

    public int WindowWidth { get; private set; } = 80;

    public ConsoleColor ForegroundColor { get; set; }

    public string ReadLine() => string.Empty;

    public void Write(string text)
    {
        if (text.EndsWith(Environment.NewLine))
            text = text[..^Environment.NewLine.Length];
        
        var item = new ColoredText(GetColor(ForegroundColor), text);
        ConsoleContent.Add(item);
        SelectedItem = item;
    }

    partial void OnConsoleWidthChanged(double value) => WindowWidth = (int)value;

    private IImmutableBrush GetColor(ConsoleColor lineColor) =>
        lineColor switch
        {
            ConsoleColor.Black => Brushes.Black,
            ConsoleColor.DarkBlue => Brushes.DarkBlue,
            ConsoleColor.DarkGreen => Brushes.DarkGreen,
            ConsoleColor.DarkCyan => Brushes.DarkCyan,
            ConsoleColor.DarkRed => Brushes.DarkRed,
            ConsoleColor.DarkMagenta => Brushes.DarkMagenta,
            ConsoleColor.DarkYellow => Brushes.DarkGoldenrod,
            ConsoleColor.Gray => Brushes.Gray,
            ConsoleColor.DarkGray => Brushes.DarkGray,
            ConsoleColor.Blue => Brushes.Blue,
            ConsoleColor.Green => Brushes.Green,
            ConsoleColor.Cyan => Brushes.Cyan,
            ConsoleColor.Red => Brushes.Red,
            ConsoleColor.Magenta => Brushes.Magenta,
            ConsoleColor.Yellow => Brushes.Gold,
            ConsoleColor.White => Brushes.White,
            _ => Brushes.Black
        };

    internal void SetUiPreferences(UIPreferences uiPreferences)
    {
        FontFamily = new FontFamily(uiPreferences.FontFamily);
        FontSize = uiPreferences.FontSize;
    }

    internal void PrepareForOutput() => ConsoleContent.Clear();

    public record ColoredText(IImmutableBrush Color, string Text);
}
