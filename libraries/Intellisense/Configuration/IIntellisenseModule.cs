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
[Singleton(typeof(IntellisenseClient))]
[Singleton(typeof(IIntellisenseService), typeof(IntellisenseService))]
[Singleton(typeof(IFileSystemReader), typeof(FileSystemReader))]
[Scoped(typeof(IFileSystemIntellisenseService), typeof(FileSystemIntellisenseService))]
[Scoped(typeof(IFileSystemPathCompletionResultRetriever), typeof(HostPathCompletionResultRetriever))]
[Scoped(typeof(IFileSystemPathCompletionResultRetriever), typeof(SharePathCompletionResultRetriever))]
[Scoped(typeof(IFileSystemPathCompletionResultRetriever), typeof(LocalFileSystemCompletionResultRetriever))]
[Scoped(typeof(CancellationContext))]
[Scoped(typeof(CancellationTokenSource), Factory = nameof(GetCancellationTokenSource))]
[Singleton(typeof(IPathFactory), typeof(PathFactory))]
[Singleton(typeof(IFileSystem), typeof(System.IO.Abstractions.FileSystem))]
[Singleton(typeof(ExclusiveRequestSession))]
[Singleton(typeof(ResourceManager))]
[Scoped(typeof(Win32ApiService))]
[Singleton(typeof(NullShareService))]
[Scoped(typeof(IShareService), Factory = nameof(GetShareService))]
public interface IIntellisenseModule
{
    public static CancellationTokenSource GetCancellationTokenSource(CancellationContext ctx) => ctx.TokenSource;

    public static IShareService GetShareService(Win32ApiService w, NullShareService n) =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? w : n;
}
