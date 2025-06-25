using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
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



public class ScrollToEndBehavior : Behavior<ScrollViewer>
{
    public static readonly StyledProperty<int> ScrollToEndProperty =
        AvaloniaProperty.Register<ScrollToEndBehavior, int>(nameof(ScrollToEnd));

    public int ScrollToEnd
    {
        get => GetValue(ScrollToEndProperty);
        set => SetValue(ScrollToEndProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        this.GetObservable(ScrollToEndProperty).Subscribe(OnScrollToEndChanged);
    }

    private void OnScrollToEndChanged(int scrollToEnd)
    {
        if (scrollToEnd!=0 && AssociatedObject != null)
        {
            AssociatedObject.ScrollToEnd();
        }
    }
}

