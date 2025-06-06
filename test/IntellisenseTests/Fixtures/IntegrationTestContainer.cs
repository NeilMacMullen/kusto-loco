using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AwesomeAssertions.Extensions;
using Intellisense;
using Intellisense.Concurrency;
using Intellisense.Configuration;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Shares;
using Jab;
using LogSetup;
using Moq;

namespace IntellisenseTests.Fixtures;

[ServiceProvider]
[Import(typeof(IIntellisenseModule))]
[Import(typeof(ILoggingModule))]
public partial class IntegrationTestContainer
{
}

[ServiceProvider]
[Import(typeof(IIntellisenseModule))]
[Import(typeof(ILoggingModule))]
[Singleton(typeof(IShareService), Factory = nameof(ShareService))]
[Singleton(typeof(IFileSystemReader), Factory = nameof(FileSystemReader))]
[Scoped(typeof(IFileSystemIntellisenseService), typeof(MockDelayFileSystemIntellisense))]
public partial class MockDelayTestContainer
{
    public IShareService ShareService() => Mock.Of<IShareService>();
    public IFileSystemReader FileSystemReader() => Mock.Of<FileSystemReader>();
}

[ServiceProvider]
[Import(typeof(IIntellisenseModule))]
[Import(typeof(ILoggingModule))]
[Singleton(typeof(IShareService), Factory = nameof(ShareService))]
[Singleton(typeof(IFileSystemReader), Factory = nameof(FileSystemReader))]
[Scoped(typeof(IFileSystemIntellisenseService), Factory = nameof(FileSystemIntellisenseService))]
public partial class MockExceptionContainer
{
    public IShareService ShareService() => Mock.Of<IShareService>();
    public IFileSystemReader FileSystemReader() => Mock.Of<FileSystemReader>();

    public IFileSystemIntellisenseService FileSystemIntellisenseService()
    {
        var mock = new Mock<IFileSystemIntellisenseService>();
        mock.Setup(x => x.GetPathIntellisenseOptionsAsync(It.IsAny<string>())).Throws<Exception>();
        return mock.Object;
    }
}

[ServiceProvider]
[Import(typeof(IIntellisenseModule))]
[Import(typeof(ILoggingModule))]
[Singleton(typeof(IShareService), Factory = nameof(ShareService))]
[Singleton(typeof(IFileSystemReader), Factory = nameof(FileSystemReader))]
[Scoped(typeof(IFileSystemIntellisenseService), Factory = nameof(FileSystemIntellisenseService))]
public partial class MockExceptionContainer2
{
    public IShareService ShareService() => Mock.Of<IShareService>();
    public IFileSystemReader FileSystemReader() => Mock.Of<FileSystemReader>();

    public IFileSystemIntellisenseService FileSystemIntellisenseService()
    {
        var mock = new Mock<IFileSystemIntellisenseService>();
        mock.Setup(x => x.GetPathIntellisenseOptionsAsync(It.IsAny<string>())).Throws<OperationCanceledException>();
        return mock.Object;
    }
}

public class MockDelayFileSystemIntellisense(CancellationContext context) : IFileSystemIntellisenseService
{
    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(string delayMs)
    {
        var delay = int.Parse(delayMs);
        await Task.Delay(delay.Milliseconds(), context.TokenSource.Token);

        return new CompletionResult()
        {
            Entries = [new IntellisenseEntry() { Name = delayMs }]
        };
    }
}

[ServiceProviderModule]
[Import(typeof(IIntellisenseModule))]
[Import(typeof(ILoggingModule))]
[Singleton(typeof(IShareService), Factory = nameof(ShareService))]
[Singleton(typeof(IFileSystemReader), Factory = nameof(FileSystemReader))]
public interface IMockTestModule
{
    public static IShareService ShareService() => Mock.Of<IShareService>();
    public static IFileSystemReader FileSystemReader() => Mock.Of<FileSystemReader>();
}

[ServiceProvider]
[Import<IMockTestModule>]
[Singleton<IFileSystemReader,ProxyReader>]
public partial class MockFileSystemTestContainer
{
}

public class ProxyReader : IFileSystemReader
{
    public IFileSystem? FileSystem { get; set; }

    public IEnumerable<IFileSystemInfo> GetChildren(string path)
    {
        var reader = new FileSystemReader(FileSystem!);
        return reader.GetChildren(path);
    }
}