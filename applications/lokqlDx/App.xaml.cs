using System.Windows;

using System.Windows.Threading;
using Intellisense.Configuration;
using LogSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

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

        appBuilder.UseApplicationLogging();


        appBuilder
            .Services
            .AddIntellisense();

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
        try
        {
            await Instance.StopAsync();

            Instance.Dispose();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }

    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0

        var logger = Resolve<ILogger<App>>();
        if (e.Exception is OperationCanceledException exc)
        {
            logger.LogDebug(exc,"Operation was cancelled.");
        }
        else
        {
            logger.LogError(e.Exception, "Application exception occurred.");
        }


        // top level exception handler
        // keep app open with unhandled exception unless there is risk of data corruption, then add handling for it here
        e.Handled = true;
    }
}

