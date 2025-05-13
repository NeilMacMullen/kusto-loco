using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Intellisense;
using Intellisense.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace IntellisenseTests.Fixtures;

internal class FileSystemIntellisenseServiceTestFixture
{
    private readonly IFileSystemIntellisenseService _fileSystemIntellisenseService;

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
            .AddIntellisenseWithMockedIo()
            .AddSingleton(reader)
            .BuildServiceProvider();
        _fileSystemIntellisenseService = provider.GetRequiredService<IFileSystemIntellisenseService>();
    }

    public Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path) =>
        _fileSystemIntellisenseService.GetPathIntellisenseOptionsAsync(path);
}
