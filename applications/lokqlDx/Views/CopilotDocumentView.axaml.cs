using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public partial class CopilotDocumentView : UserControl
{
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
            vm.Messages.CollectionChanged += (_, __) => ScrollChatToBottom();
        }
    }

    private void ScrollChatToBottom()
    {
        // Try both scroll viewers (one for each pane)
        var scroll1 = this.FindControl<ScrollViewer>("MessagesScrollViewer");
        scroll1?.ScrollToEnd();

        // The second scroll viewer has no name, so find by type
        foreach (var sv in GetScrollViewers(this))
        {
            if (sv != scroll1 && sv.Parent is DockPanel)
            {
                sv.ScrollToEnd();
            }
        }
    }

    private static IEnumerable<ScrollViewer> GetScrollViewers(Control root)
    {
        if (root is ScrollViewer sv)
            yield return sv;
        foreach (var child in root.GetVisualChildren())
        {
            if (child is Control ctrl)
            {
                foreach (var descendant in GetScrollViewers(ctrl))
                    yield return descendant;
            }
        }
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
