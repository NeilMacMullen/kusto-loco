using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace lokqlDx;

/// <summary>
///     Simple IConsole implementation that allows a RichTextBox to emulate a console for output text
/// </summary>
/// <remarks>
///     Rather than mess around with different dispatcher contexts, we just mandate that the client
///     app needs to render the buffered text at the end of the operation.
/// </remarks>
public class WpfConsole : IConsole
{
    private readonly RichTextBox _control;
    private readonly List<ColoredText> _lines = new();

    private ConsoleColor _currentColor = ConsoleColor.Red;

    public WpfConsole(RichTextBox control) => _control = control;

    //calculate the number of visible columns in the RichTextBox
    public int VisibleColumns
    {
        get
        {
            var typeface = new Typeface(_control.FontFamily, _control.FontStyle, _control.FontWeight,
                _control.FontStretch);
            var n = 50;
            var formattedText = new FormattedText("".PadRight(n, 'w'), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface, _control.FontSize, Brushes.Black,
                VisualTreeHelper.GetDpi(_control).PixelsPerDip);
            return (int)(_control.ActualWidth * n * 0.9 / formattedText.WidthIncludingTrailingWhitespace);
        }
    }

    public void Write(string s)
    {
        _lines.Add(new ColoredText(_currentColor, s));
    }

    public void SetForegroundColor(ConsoleColor color)
    {
        _currentColor = color;
    }

    public int WindowWidth { get; private set; }


    public string ReadLine() => string.Empty;

    /// <summary>
    ///     Prepare for new text output and calculate the width of the console window
    /// </summary>
    public void PrepareForOutput()
    {
        _lines.Clear();
        WindowWidth = Math.Max(10, VisibleColumns);
    }


    private static void AppendText(RichTextBox box, string text, Brush color)
    {
        text = text.Replace("\r", "");
        var tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
        {
            Text = text
        };
        try
        {
            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                color);
        }
        catch (FormatException)
        {
        }
    }

    /// <summary>
    ///     Renders queued text to the RichTextBox that backs up this console
    /// </summary>
    public void RenderTextBufferToWpfControl()
    {
        _control.Document.Blocks.Clear();
        _control.Document.LineHeight = 1;
        foreach (var line in _lines)
        {
            AppendText(_control, line.Text, GetColor(line.Color));
        }
    }

    private Brush GetColor(ConsoleColor lineColor)
    {
        switch (lineColor)
        {
            case ConsoleColor.Black:
                return Brushes.Black;
            case ConsoleColor.DarkBlue:
                return Brushes.DarkBlue;
            case ConsoleColor.DarkGreen:
                return Brushes.DarkGreen;
            case ConsoleColor.DarkCyan:
                return Brushes.DarkCyan;
            case ConsoleColor.DarkRed:
                return Brushes.DarkRed;
            case ConsoleColor.DarkMagenta:
                return Brushes.DarkMagenta;
            case ConsoleColor.DarkYellow:
                return Brushes.DarkGoldenrod;
            case ConsoleColor.Gray:
                return Brushes.Gray;
            case ConsoleColor.DarkGray:
                return Brushes.DarkGray;
            case ConsoleColor.Blue:
                return Brushes.Blue;
            case ConsoleColor.Green:
                return Brushes.Green;
            case ConsoleColor.Cyan:
                return Brushes.Cyan;
            case ConsoleColor.Red:
                return Brushes.Red;
            case ConsoleColor.Magenta:
                return Brushes.Magenta;
            case ConsoleColor.Yellow:
                return Brushes.Gold;
            case ConsoleColor.White:
                return Brushes.White;
        }

        return Brushes.Black;
    }

    private readonly record struct ColoredText(ConsoleColor Color, string Text);
}