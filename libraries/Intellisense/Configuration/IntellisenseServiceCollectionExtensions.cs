using System.IO.Abstractions;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Intellisense.Configuration;

public static class IntellisenseServiceCollectionExtensions
{
    public static IServiceCollection AddIntellisense(this IServiceCollection services)
    {
        // main services
        services.AddSingleton<IFileSystemIntellisenseService, FileSystemIntellisenseService>();


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
            .AddSingleton<IShareClient, ShareClient>()
            .AddSingleton<IHostRepository, HostRepository>()
            .AddSingleton<IShareResource, Win32ShareResource>();


        // timeouts
        services.AddCancellationContext();

        services.TryConfigure<IntellisenseTimeoutOptions>(x => x.IntellisenseTimeout = TimeSpan.FromMilliseconds(5000));


        // auxiliary services
        services.TryAddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>();
        services.TryAddSingleton(TimeProvider.System);


        return services;
    }

    public static IServiceCollection PostConfigureIntellisenseTimeouts(
        this IServiceCollection services,
        Action<IntellisenseTimeoutOptions> configure
    )
    {
        services.PostConfigure(configure);
        return services;
    }

    private static void AddCancellationContext(this IServiceCollection services)
    {
        services.TryAddScoped<CancellationContext>();
        services.TryAddScoped(x => x.GetRequiredService<CancellationContext>().TokenSource);
    }
}
