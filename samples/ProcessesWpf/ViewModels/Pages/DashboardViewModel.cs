using System.Collections.ObjectModel;
using System.Windows.Data;
using Wpf.Ui.Abstractions.Controls;

namespace ProcessesWpf.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{
    public Task OnNavigatedFromAsync() => Task.CompletedTask;
    public Task OnNavigatedToAsync() => Task.CompletedTask;
}
