using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KustoLoco.Core.Console;
using System.Collections.ObjectModel;
using System.Text;

namespace LokqlDx.ViewModels;

public partial class ConsoleViewModel : ObservableObject, IKustoConsole
{
    
    [ObservableProperty] private ObservableCollection<ColoredText> _consoleContent = [];
    [ObservableProperty] private double _consoleWidth;
    [ObservableProperty] private ColoredText? _selectedItem;
    [ObservableProperty] private DisplayPreferencesViewModel _displayPreferencesPreferences;
    [ObservableProperty] private int _triggerScroll;

    public ConsoleViewModel(DisplayPreferencesViewModel displayPreferencesPreferences)
    {
        ConsoleContent = new ObservableCollection<ColoredText>();
        ForegroundColor = ConsoleColor.White;
        WindowWidth = 80; // Default width
        DisplayPreferencesPreferences = displayPreferencesPreferences;
        WeakReferenceMessenger.Default.Register<ClearConsoleMessage>(this, (r, m) =>
        {
          Clear();
        });

    }

    [RelayCommand]
    private void Clear()
    {
        ConsoleContent.Clear();
    }

    public int WindowWidth { get; private set; } = 80;

    public ConsoleColor ForegroundColor { get; set; }

    public string ReadLine() => string.Empty;

    private StringBuilder _buffer = new StringBuilder();
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
