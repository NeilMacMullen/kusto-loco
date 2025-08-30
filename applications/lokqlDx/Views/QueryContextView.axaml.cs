using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public partial class QueryContextView : UserControl
{
    public QueryContextView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is INotifyPropertyChanged npc) npc.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QueryContextViewModel.IsExpanded) && DataContext is QueryContextViewModel
            {
                IsExpanded: true
            })
            // Ensure focus is set on the UI thread
            Dispatcher.UIThread.Post(() => ParametersTextBox.Focus());
    }
}
