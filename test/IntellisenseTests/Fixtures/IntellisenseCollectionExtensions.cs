using Intellisense.Configuration;
using Intellisense.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace IntellisenseTests.Fixtures;

public static class IntellisenseCollectionExtensions
{
    public static IServiceCollection AddMockedIo(this IServiceCollection services) => services
        .AddIntellisense()
        .AddFakeLogging()
        .MockSingleton<IFileSystemReader>();

    public static IServiceCollection AddDefault(this IServiceCollection services) => services
        .AddIntellisense()
        .AddLogging(x => x.AddConsole());

    private static IServiceCollection MockSingleton<T>(this IServiceCollection services) where T : class => services
        .AddSingleton<T>(_ => Mock.Of<T>());
}
