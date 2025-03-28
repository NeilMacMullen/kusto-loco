﻿using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using KustoLoco.Core.Console;

namespace lokqlDx;

/// <summary>
///     Simple IKustoConsole implementation that allows a RichTextBox to emulate a console for output text
/// </summary>
public class WpfConsole : IKustoConsole
{
    private readonly RichTextBox _control;


    public WpfConsole(RichTextBox control) => _control = control;

    //calculate the number of visible columns in the RichTextBox
    public int VisibleColumns
    {
        get
        {
            var typeface = new Typeface(_control.FontFamily, _control.FontStyle, _control.FontWeight,
                                        _control.FontStretch);
            var n = 50;
            var formattedText = new FormattedText(string.Empty.PadRight(n, 'w'), CultureInfo.InvariantCulture,
                                                  FlowDirection.LeftToRight,
                                                  typeface, _control.FontSize, Brushes.Black,
                                                  VisualTreeHelper.GetDpi(_control).PixelsPerDip);
            return (int)(_control.ActualWidth * n * 0.9 / formattedText.WidthIncludingTrailingWhitespace);
        }
    }

    public void Write(string s)
    {
        var line = new ColoredText(ForegroundColor, s);

        Application.Current.Dispatcher.Invoke(() => { RenderTextBufferToWpfControl(line); });
    }

    public ConsoleColor ForegroundColor { get; set; }


    public int WindowWidth { get; private set; }


    public string ReadLine() => string.Empty;

    /// <summary>
    ///     Prepare for new text output and calculate the width of the console window
    /// </summary>
    public void PrepareForOutput()
    {
        Application.Current.Dispatcher.Invoke(() =>
                                              {
                                                  _control.Document.Blocks.Clear();
                                                  _control.Document.LineHeight = 1;
                                                  WindowWidth = Math.Max(10, VisibleColumns);
                                                  //TODO - works around some weirdness in the RichTextBox control
                                                  //that causes it to not render a newline after the first line of text
                                                  AppendText(_control, "\r\n", Brushes.Black);
                                              });
    }


    private static void AppendText(RichTextBox box, string text, Brush color)
    {
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

        box.ScrollToEnd();
    }

    /// <summary>
    ///     Renders queued text to the RichTextBox that backs up this console
    /// </summary>
    private void RenderTextBufferToWpfControl(ColoredText line)
    {
        AppendText(_control, line.Text, GetColor(line.Color));
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
