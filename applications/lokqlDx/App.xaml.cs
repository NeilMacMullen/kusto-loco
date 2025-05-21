using System.Windows;
using System.Windows.Threading;
using Intellisense.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace lokqlDx;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Window window = new MainWindow(e.Args);
        window.Show();
    }

    public static IServiceProvider Services => Instance.Services;

    public static T Resolve<T>() where T : notnull => Instance.Services.GetRequiredService<T>();

    private static IHost CreateHost()
    {

        var appBuilder = Host.CreateApplicationBuilder();




        appBuilder
            .Services
            .AddIntellisense()
            .AddSingleton<GlobalExceptionHandler>();

        return appBuilder.Build();
    }

    private static readonly IHost Instance = CreateHost();

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private void OnStartup(object sender, StartupEventArgs e) => Instance.Start();

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    private async void OnExit(object sender, ExitEventArgs e)
    {
        var logger = Resolve<ILogger<App>>();
        try
        {
            await Instance.StopAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unhandled exception while closing application.");
        }
        finally
        {
            Instance.Dispose();
        }

    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0

        var handler = Resolve<GlobalExceptionHandler>();

        e.Handled = handler.Handle(e.Exception);
    }
}

