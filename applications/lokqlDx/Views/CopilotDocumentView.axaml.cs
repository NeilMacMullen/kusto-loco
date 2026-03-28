using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public partial class CopilotDocumentView : UserControl
{
    private double _lastScrollOffset1;
    private double _lastScrollOffset2;

    public CopilotDocumentView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // Set up key handler for all TextBoxes in this view
        // Using AddHandler with tunneling to catch the event before the TextBox processes it
        AddHandler(KeyDownEvent, OnPreviewKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        // Subscribe to chat message changes for auto-scroll
        if (DataContext is CopilotDocumentViewModel vm)
        {
            vm.Messages.CollectionChanged += (_, __) => ScrollChatToBottomDelayed();
        }
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        // Restore scroll position when view regains focus
        RestoreScrollPositions();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // When the control becomes visible again, restore scroll position
        if (change.Property == IsVisibleProperty && change.NewValue is true)
        {
            RestoreScrollPositions();
        }
    }

    private void RestoreScrollPositions()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var scroll1 = this.FindControl<ScrollViewer>("MessagesScrollViewer");
            var scroll2 = this.FindControl<ScrollViewer>("MessagesScrollViewer2");

            if (scroll1 != null && _lastScrollOffset1 > 0)
            {
                scroll1.Offset = new Vector(0, _lastScrollOffset1);
            }
            if (scroll2 != null && _lastScrollOffset2 > 0)
            {
                scroll2.Offset = new Vector(0, _lastScrollOffset2);
            }
        }, DispatcherPriority.Loaded);
    }

    private void SaveScrollPositions()
    {
        var scroll1 = this.FindControl<ScrollViewer>("MessagesScrollViewer");
        var scroll2 = this.FindControl<ScrollViewer>("MessagesScrollViewer2");

        if (scroll1 != null)
        {
            _lastScrollOffset1 = scroll1.Offset.Y;
        }
        if (scroll2 != null)
        {
            _lastScrollOffset2 = scroll2.Offset.Y;
        }
    }

    private void ScrollChatToBottomDelayed()
    {
        // Use a slight delay to ensure layout is complete before scrolling
        Dispatcher.UIThread.Post(() =>
        {
            var scroll1 = this.FindControl<ScrollViewer>("MessagesScrollViewer");
            var scroll2 = this.FindControl<ScrollViewer>("MessagesScrollViewer2");

            scroll1?.ScrollToEnd();
            scroll2?.ScrollToEnd();

            // Save these positions as the new "last known good" positions
            SaveScrollPositions();
        }, DispatcherPriority.Loaded);
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        // Shift+Enter to send message (consistent with standard query panes)
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && DataContext is CopilotDocumentViewModel vm)
        {
            if (vm.SendMessageCommand.CanExecute(null))
            {
                vm.SendMessageCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Converter for message role to background color
    /// </summary>
    public static readonly IValueConverter RoleToBackgroundConverter = new RoleToBackgroundValueConverter();

    /// <summary>
    /// Converter for message role to horizontal alignment
    /// </summary>
    public static readonly IValueConverter RoleToAlignmentConverter = new RoleToAlignmentValueConverter();

    private class RoleToBackgroundValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var role = value as string ?? string.Empty;
            return role switch
            {
                "user" => new SolidColorBrush(Color.FromArgb(180, 0, 120, 212)), // Blue for user
                "assistant" => new SolidColorBrush(Color.FromArgb(100, 128, 128, 128)), // Gray for assistant
                "system" => new SolidColorBrush(Color.FromArgb(80, 100, 100, 100)), // Lighter gray for system
                "error" => new SolidColorBrush(Color.FromArgb(120, 255, 80, 80)), // Red for errors
                _ => new SolidColorBrush(Color.FromArgb(100, 128, 128, 128))
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    private class RoleToAlignmentValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var role = value as string ?? string.Empty;
            return role switch
            {
                "user" => HorizontalAlignment.Right,
                "system" => HorizontalAlignment.Center,
                _ => HorizontalAlignment.Left
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
