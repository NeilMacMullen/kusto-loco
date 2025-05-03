using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Intellisense;
using Intellisense.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;

namespace IntellisenseTests.Fixtures;

internal class FileSystemIntellisenseServiceTestFixture
{
    private readonly IFileSystemIntellisenseService _fileSystemIntellisenseService;
    private readonly IServiceProvider _provider;

    public FileSystemIntellisenseServiceTestFixture(
        Dictionary<string, MockFileData> fileData
    )
        : this(
            new FileSystemReader(
                new MockFileSystem(fileData, new MockFileSystemOptions { CreateDefaultTempDir = false })
            )
        )
    {
    }

    public FileSystemIntellisenseServiceTestFixture(IFileSystemReader reader)
    {
        var provider = new ServiceCollection()
            .AddMockedIo()
            .AddSingleton(reader)
            .BuildServiceProvider();
        _fileSystemIntellisenseService = provider.GetRequiredService<IFileSystemIntellisenseService>();
        _provider = provider;
    }

    public IReadOnlyList<FakeLogRecord> GetLogs() => _provider.GetRequiredService<FakeLogCollector>().GetSnapshot();

    public Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path) =>
        _fileSystemIntellisenseService.GetPathIntellisenseOptionsAsync(path);
}
