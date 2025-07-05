using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LokqlDx.Desktop;
using LokqlDx.ViewModels;
using LokqlDx.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LokqlDx;

public class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var mainView = _serviceProvider.GetRequiredService<MainView>();


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow(mainView);
            desktop.MainWindow = mainWindow;

            if (_serviceProvider is DiContainer di)
                di.SetTopLevel(desktop.MainWindow);

            var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            mainWindow.DataContext = viewModel;

            if (desktop.Args is not null)
                viewModel.SetInitWorkspacePath(desktop.Args?.FirstOrDefault() ?? "");
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = mainView;

            if (_serviceProvider is DiContainer di)
                di.SetTopLevel(TopLevel.GetTopLevel(singleViewPlatform.MainView));

            var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            mainView.DataContext = viewModel;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
