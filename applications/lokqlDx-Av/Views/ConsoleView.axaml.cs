using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Xaml.Interactions.Custom;
using CommunityToolkit.Mvvm.ComponentModel;
using LokqlDx.ViewModels;
using System.Globalization;

namespace LokqlDx.Views;

public partial class ConsoleView : UserControl
{
    public ConsoleView()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UserControl.FontFamilyProperty || change.Property == UserControl.FontSizeProperty)
        {
            UpdateSize(Bounds.Width);
        }

    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (FontSize == 0)
            return;

        UpdateSize(e.NewSize.Width);
    }

    private void UpdateSize(double width)
    {
        if (DataContext is not ViewModels.ConsoleViewModel console)
            return;

        if (FontSize == 0)
            return;

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var n = 50;

        var formattedText = new FormattedText(
            string.Empty.PadRight(n, 'w'),
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            Brushes.Black);


        console.ConsoleWidth = (int)(width * n * 0.9 / formattedText.WidthIncludingTrailingWhitespace);
    }
}
