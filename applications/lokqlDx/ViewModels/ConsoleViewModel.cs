using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core.Console;

namespace LokqlDx.ViewModels;

public partial class ConsoleViewModel : ObservableObject, IKustoConsole
{
    private readonly StringBuilder _buffer = new();

    [ObservableProperty] private ObservableCollection<ColoredText> _consoleContent = [];
    [ObservableProperty] private double _consoleWidth;
    [ObservableProperty] private DisplayPreferencesViewModel _displayPreferencesPreferences;
    [ObservableProperty] private ColoredText? _selectedItem;
    [ObservableProperty] private int _triggerScroll;

    public ConsoleViewModel(DisplayPreferencesViewModel displayPreferencesPreferences)
    {
        ConsoleContent = new ObservableCollection<ColoredText>();
        ForegroundColor = ConsoleColor.White;
        WindowWidth = 80; // Default width
        DisplayPreferencesPreferences = displayPreferencesPreferences;
        Messaging.RegisterForEvent<ClearConsoleMessage>(this, Clear);
    }

    public int WindowWidth { get; private set; } = 80;

    public ConsoleColor ForegroundColor { get; set; }

    public string ReadLine() => string.Empty;

    public void Write(string text)
    {
        var flush = false;
        if (text.EndsWith(Environment.NewLine))
        {
            text = text[..^Environment.NewLine.Length];
            flush = true;
        }

        _buffer.Append(text);

        if (flush)
        {
            var item = new ColoredText(GetColor(ForegroundColor), _buffer.ToString());
            ConsoleContent.Add(item);
            SelectedItem = item;
            TriggerScroll = TriggerScroll + 1;
            _buffer.Clear();
        }
    }

    [RelayCommand]
    private void Clear() => ConsoleContent.Clear();

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


    internal void PrepareForOutput()
    {
        //ConsoleContent.Clear();
    }

    public record ColoredText(IImmutableBrush Color, string Text);
}
