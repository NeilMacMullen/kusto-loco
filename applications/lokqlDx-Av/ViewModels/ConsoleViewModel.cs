using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using KustoLoco.Core.Console;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokqlDx.ViewModels;
public partial class ConsoleViewModel : ObservableObject, IKustoConsole
{
    [ObservableProperty] ObservableCollection<ColoredText> _consoleContent = [];
    [ObservableProperty] ColoredText? _selectedItem;
    [ObservableProperty] double _consoleWidth;
    [ObservableProperty] FontFamily? _fontFamily;
    [ObservableProperty] double _fontSize = 20;

    partial void OnConsoleWidthChanged(double value)
    {
        WindowWidth = (int)value;
    }

    public int WindowWidth { get; private set; } = 80;

    public ConsoleColor ForegroundColor { get; set; }

    public string ReadLine() => string.Empty;
    public void Write(string text)
    {
        var item = new ColoredText(GetColor(ForegroundColor), text);
        ConsoleContent.Add(item);
        SelectedItem = item;
    }

    private IImmutableBrush GetColor(ConsoleColor lineColor)
    {
        return lineColor switch
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
            _ => Brushes.Black,
        };
    }

    internal void SetUiPreferences(UIPreferences uiPreferences)
    {
        FontFamily = new FontFamily(uiPreferences.FontFamily);
        FontSize = uiPreferences.FontSize;
    }

    internal void PrepareForOutput()
    {
        ConsoleContent.Clear();
    }

    public record ColoredText(IImmutableBrush Color, string Text);
}
