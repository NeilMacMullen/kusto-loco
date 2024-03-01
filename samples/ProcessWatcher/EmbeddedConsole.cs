using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ProcessWatcher;



public class EmbeddedConsoleWriter : TextWriter
{
    private readonly EmbeddedConsole _console;

    public EmbeddedConsoleWriter(EmbeddedConsole console)
    {
        _console = console;
    }

    public override Encoding Encoding {
        get { return Encoding.UTF8; }
    }

    public override void Write(char value)
    {
        _console.Write(value.ToString());
    }
}

public class EmbeddedConsole : IConsole
{
    private readonly RichTextBox _control;

    public EmbeddedConsole(RichTextBox control)
    {
        _control = control;
        _writer = new EmbeddedConsoleWriter(this);
    }
    private readonly record struct ColoredText (ConsoleColor Color,string Text);
    private readonly List<ColoredText> _lines = new();
    ConsoleColor _currentColor = ConsoleColor.Red;
    private readonly EmbeddedConsoleWriter _writer;

    public void Write(string s)
    {
        _lines.Add(new ColoredText(_currentColor, s));
    }

    public void SetForegroundColor(ConsoleColor color)
    {
       _currentColor= color;
    }

    public int WindowWidth
    {
        get;
        private set;
    }

    public void Init()
    {
        _lines.Clear();
       
        WindowWidth=Math.Max(10, VisibleColumns);

    }
    //calculate the number of visible columns in the RichTextBox
    public int VisibleColumns
    {
        get
        {
            var typeface = new Typeface(_control.FontFamily, _control.FontStyle, _control.FontWeight, _control.FontStretch);
            var n = 50;
            var formattedText = new FormattedText("".PadRight(n,'w'), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface, _control.FontSize, Brushes.Black,
                VisualTreeHelper.GetDpi(_control).PixelsPerDip);
            return (int)(_control.ActualWidth *n*0.9 / formattedText.WidthIncludingTrailingWhitespace);
        }
    }

    public TextWriter Writer => _writer;

    public string ReadLine()
    {
       return string.Empty;
    }


    public static void AppendText(RichTextBox box, string text, Brush color)
    {
        text  = text.Replace("\r", "");
        var tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
        {
            Text = text
        };
        try
        {
            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                color);
        }
        catch (FormatException) { }
    }
    public void RenderToRichTextBox()
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
}