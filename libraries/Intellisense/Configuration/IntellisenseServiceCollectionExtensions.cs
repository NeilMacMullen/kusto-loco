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


        // standard file system
        services.AddSingleton<IFileSystemReader, FileSystemReader>();
        services
            .AddSingleton<IFileSystemPathCompletionResultRetriever, ChildrenPathCompletionResultRetriever>()
            .AddSingleton<IFileSystemPathCompletionResultRetriever, SiblingPathCompletionResultRetriever>();

        // path processing
        services.AddSingleton<IPathFactory, PathFactory>();

        // shares
        services
            .AddSingleton<IShareReader, Win32ApiShareReader>()
            .AddSingleton<IShareClient, ShareClient>()
            .AddSingleton<IHostRepository, HostRepository>()
            .AddSingleton<IFileSystemPathCompletionResultRetriever, HostPathCompletionResultRetriever>()
            .AddSingleton<IFileSystemPathCompletionResultRetriever, SharePathCompletionResultRetriever>();


        // auxiliary services
        services.TryAddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>();


        return services;
    }
}
