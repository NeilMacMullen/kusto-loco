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
    {
        var fs = new MockFileSystem(fileData, new MockFileSystemOptions { CreateDefaultTempDir = false });
        var reader = new FileSystemReader(fs);
        var provider = new MockedIoTestContainer();
        provider.GetFileSystemReader = () => reader;

        _fileSystemIntellisenseService = provider.GetRequiredService<IFileSystemIntellisenseService>();
    }

    public Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path) =>
        _fileSystemIntellisenseService.GetPathIntellisenseOptionsAsync(path);
}
