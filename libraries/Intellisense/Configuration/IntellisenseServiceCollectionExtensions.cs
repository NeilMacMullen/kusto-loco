using System.IO.Abstractions;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Intellisense.Configuration;

public static class IntellisenseServiceCollectionExtensions
{
    public static IServiceCollection AddIntellisense(this IServiceCollection services)
    {


        // main services
        services.AddSingleton<IFileSystemIntellisenseService, FileSystemIntellisenseService>();

        // completion result retrievers
        services
            .AddSingleton<IFileSystemPathCompletionResultRetriever, ChildrenPathCompletionResultRetriever>()
            .AddSingleton<IFileSystemPathCompletionResultRetriever, SiblingPathCompletionResultRetriever>();



        // file access
        services.AddSingleton<IFileSystemReader, FileSystemReader>();

        // path processing
        services.AddSingleton<IPathFactory, PathFactory>();

        // shares
        services
            .AddSingleton<IShareReader, Win32ApiShareReader>()
            .AddSingleton<ShareClient>();



        // auxiliary services
        services.TryAddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>();


        return services;
    }
}
