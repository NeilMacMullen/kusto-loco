using System;

using Avalonia;
using HotAvalonia;
using Jab;
using Lokql.Engine.Commands;
using LokqlDx.ViewModels;
using LokqlDx.Views;
using LokqlDx.Services;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace LokqlDx.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var container = new DiContainer();
        BuildAvaloniaApp(container).StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure<App>(() => serviceProvider.GetRequiredService<App>())
            .UsePlatformDetect()
            .WithInterFont()
            .UseHotReload()
            .LogToTrace();

}

[ServiceProvider]
[Transient<App>]
[Transient<MainViewModel>]
[Transient<MainView>]
[Transient<PreferencesManager>]
[Transient<DialogService>]
[Transient<BrowserServices>]
[Transient<WorkspaceManager>]
[Transient<RegistryOperations>]
[Transient<CommandProcessorFactory>]
[Transient<IStorageProvider>(Factory = nameof(GetStorageProvider))]
[Transient<TopLevel>(Factory = nameof(GetTopLevel))]
internal partial class DiContainer
{
    private TopLevel? _topLevel;

    internal void SetTopLevel(TopLevel? topLevel) => _topLevel = topLevel;
    internal TopLevel GetTopLevel() => _topLevel ?? throw new InvalidOperationException();
    internal IStorageProvider GetStorageProvider() => _topLevel?.StorageProvider ?? throw new InvalidOperationException();
}

public class CommandProcessorFactory
{
    public CommandProcessor GetCommandProcessor() => CommandProcessor.Default();
}
