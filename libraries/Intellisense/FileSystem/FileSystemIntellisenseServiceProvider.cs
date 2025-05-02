using Intellisense.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Intellisense.FileSystem;

public static class FileSystemIntellisenseServiceProvider
{
    private static readonly IServiceProvider Provider = new ServiceCollection()
        .AddIntellisense()
        .AddLogging() // in place to resolve dependencies, does nothing for now
        .BuildServiceProvider();
    public static IFileSystemIntellisenseService GetFileSystemIntellisenseService() => Provider.GetRequiredService<IFileSystemIntellisenseService>();
}
