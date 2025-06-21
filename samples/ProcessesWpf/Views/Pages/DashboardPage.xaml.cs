using System.ComponentModel;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using ProcessesWpf.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ProcessesWpf.Views.Pages;
public partial class DashboardPage : INavigableView<DashboardViewModel>
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        PropertyChangedEventHandler? onPropertyChanged = OnPropertyChanged;
        ViewModel.PropertyChanged += onPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Result))
        {
            var result = ViewModel.Result;
            if (result.IsChart)
            {
                WpfPlot1.Reset();
                WpfPlot1.Plot.Clear();
                ScottPlotKustoResultRenderer.RenderToPlot(WpfPlot1.Plot,result,new KustoSettingsProvider());
                WpfPlot1.Refresh();
            }
        }
    }
}
