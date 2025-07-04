using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Intellisense.Concurrency;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;
using Jab;

namespace Intellisense.Configuration;

[ServiceProviderModule]
[Singleton<IntellisenseClient>()]
[Singleton<IIntellisenseService, IntellisenseService>()]
[Singleton<IFileSystemReader, FileSystemReader>()]
[Scoped<IFileSystemIntellisenseService, FileSystemIntellisenseService>()]
[Scoped<IFileSystemPathCompletionResultRetriever, HostPathCompletionResultRetriever>()]
[Scoped<IFileSystemPathCompletionResultRetriever, SharePathCompletionResultRetriever>()]
[Scoped<IFileSystemPathCompletionResultRetriever, FileSystemCompletionResultRetriever>()]
[Scoped<CancellationContext>()]
[Scoped<CancellationTokenSource>(Factory = nameof(GetCancellationTokenSource))]
[Singleton<IPathFactory, PathFactory>()]
[Singleton<IFileSystem, System.IO.Abstractions.FileSystem>()]
[Singleton<ExclusiveRequestSession>()]
[Singleton<ResourceManager>()]
[Scoped<Win32ApiService>()]
[Singleton<NullShareService>()]
[Scoped<IShareService>(Factory = nameof(GetShareService))]
[Singleton<IImageSourceLocator,NullImageSourceLocator>]
public interface IIntellisenseModule
{
    public static CancellationTokenSource GetCancellationTokenSource(CancellationContext ctx) => ctx.TokenSource;

    public static IShareService GetShareService(Win32ApiService w, NullShareService n) =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? w : n;
}
