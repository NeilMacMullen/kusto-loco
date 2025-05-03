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
            .AddSingleton<IFileSystemPathCompletionResultRetriever, ChildrenPathCompletionResultRetriever>()
            .AddSingleton<IFileSystemPathCompletionResultRetriever, SiblingPathCompletionResultRetriever>()
            .AddSingleton<IFileSystemPathCompletionResultRetriever, HostPathCompletionResultRetriever>()
            .AddSingleton<IFileSystemPathCompletionResultRetriever, SharePathCompletionResultRetriever>();

        // path processing
        services.AddSingleton<IPathFactory, PathFactory>();

        // shares
        services
            .AddSingleton<IShareReader, Win32ApiShareReader>()
            .AddSingleton<IShareClient, ShareClient>()
            .AddSingleton<IHostRepository, HostRepository>();


        // auxiliary services
        services.TryAddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>();


        return services;
    }
}
