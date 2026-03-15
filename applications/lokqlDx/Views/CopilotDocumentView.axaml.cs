using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
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

        // Set up Enter key handler for the input text box
        var inputTextBox = this.FindControl<TextBox>("InputTextBox");
        if (inputTextBox != null)
        {
            inputTextBox.KeyDown += InputTextBox_KeyDown;
        }
    }

    private void InputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+Enter to send message (since Enter now creates a new line)
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control) && DataContext is CopilotDocumentViewModel vm)
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
