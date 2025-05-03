
using Intellisense.Configuration;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Shares;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntellisenseTests.Fixtures;

public static class IntellisenseCollectionExtensions
{
    public static IServiceCollection AddMockedIo(this IServiceCollection services) => services
        .AddIntellisense()
        .AddFakeLogging()
        .MockSingleton<IFileSystemReader>()
        .MockSingleton<IShareReader>()
        .MockSingleton<IHostReader>();

    public static IServiceCollection AddDefault(this IServiceCollection services) => services
        .AddIntellisense()
        .AddFakeLogging();

    private static IServiceCollection MockSingleton<T>(this IServiceCollection services) where T : class => services
        .AddSingleton<T>(_ => Mock.Of<T>());
}
