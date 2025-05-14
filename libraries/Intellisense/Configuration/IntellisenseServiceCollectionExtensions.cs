using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.InteropServices;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Intellisense.Configuration;

public static class IntellisenseServiceCollectionExtensions
{
    public static IServiceCollection AddIntellisense(this IServiceCollection services)
    {
        // main services
        services.AddSingleton<IFileSystemIntellisenseService, FileSystemIntellisenseService>();

        services.Configure<IntellisenseOptions>(x =>
            {
                // default location
                var folder1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
                x.Directory = Path.Combine(folder1, appName);
            }
        );


        // file system
        services.AddSingleton<IFileSystemReader, FileSystemReader>();

        // completion results
        services
            .AddScoped<IFileSystemPathCompletionResultRetriever, ChildrenPathCompletionResultRetriever>()
            .AddScoped<IFileSystemPathCompletionResultRetriever, SiblingPathCompletionResultRetriever>()
            .AddScoped<IFileSystemPathCompletionResultRetriever, HostPathCompletionResultRetriever>()
            .AddScoped<IFileSystemPathCompletionResultRetriever, SharePathCompletionResultRetriever>();

        // path processing
        services.AddSingleton<IPathFactory, PathFactory>();

        // shares

        services
            .AddScoped<IShareReader, Win32ApiShareReader>()
            .AddScoped<IHostReader, Win32ApiHostReader>();

        services.AddScoped<IConnectionVerifier, ConnectionVerifier>();


        // timeouts
        services.AddCancellationContext();

        // auxiliary services
        services.TryAddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>();
        services.TryAddSingleton(TimeProvider.System);


        return services;
    }
}
