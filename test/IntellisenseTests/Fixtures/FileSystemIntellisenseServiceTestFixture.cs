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
        var provider = new MockFileSystemTestContainer();
        var reader = (ProxyReader)provider.GetRequiredService<IFileSystemReader>();
        reader.FileSystem = new MockFileSystem(fileData, new MockFileSystemOptions { CreateDefaultTempDir = false });
        _fileSystemIntellisenseService = provider.GetRequiredService<IFileSystemIntellisenseService>();
    }

    public Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path) =>
        _fileSystemIntellisenseService.GetPathIntellisenseOptionsAsync(path);
}
