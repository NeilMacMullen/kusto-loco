using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Intellisense.Concurrency;
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
        services.AddSingleton<IntellisenseClient>();
        services.AddSingleton<IIntellisenseService, IntellisenseService>();

        services.AddSingleton<IFileSystemReader, FileSystemReader>();
        services.AddScoped<IFileSystemIntellisenseService, FileSystemIntellisenseService>();

        services.AddScoped<IFileSystemPathCompletionResultRetriever, HostPathCompletionResultRetriever>();
        services.AddScoped<IFileSystemPathCompletionResultRetriever, SharePathCompletionResultRetriever>();
        services.AddScoped<IFileSystemPathCompletionResultRetriever, LocalFileSystemCompletionResultRetriever>();

        services.AddSingleton<IPathFactory, PathFactory>();
        AddShareService(services);


        services.TryAddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>();
        services.AddCancellationContext();
        services.AddSingleton<ExclusiveRequestSession>();


        return services;
    }

    private static void AddShareService(IServiceCollection services)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddScoped<IShareService, NullShareService>();
            return;
        }

        services.AddScoped<IShareService, Win32ApiService>();
    }
}
