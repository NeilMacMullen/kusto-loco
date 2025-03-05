using KustoLoco.Core;
using ProcessesWpf.ViewModels.Pages;
using System.Diagnostics;
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
    }
}
