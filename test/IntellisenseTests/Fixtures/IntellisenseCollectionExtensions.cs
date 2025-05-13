
using Intellisense.Configuration;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Shares;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntellisenseTests.Fixtures;

public static class IntellisenseCollectionExtensions
{
    public static IServiceCollection AddIntellisenseWithMockedIo(this IServiceCollection services) => services
        .AddIntellisense()
        .AddFakeLogging()
        .MockSingleton<IFileSystemReader>()
        .MockSingleton<IShareService>();

    public static IServiceCollection AddDefaultIntellisense(this IServiceCollection services) => services
        .AddIntellisense()
        .AddLogging();

    private static IServiceCollection MockSingleton<T>(this IServiceCollection services) where T : class => services
        .AddSingleton<T>(_ => Mock.Of<T>());
}
