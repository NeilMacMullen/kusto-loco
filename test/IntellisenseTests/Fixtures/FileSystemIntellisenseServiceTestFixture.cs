using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Intellisense;
using Intellisense.FileSystem;
using Microsoft.Extensions.Logging.Testing;

namespace IntellisenseTests.Fixtures;

internal class FileSystemIntellisenseServiceTestFixture
{
    private readonly FileSystemIntellisenseService _fileSystemIntellisenseService;
    private readonly FakeLogger<IFileSystemIntellisenseService> _logger;

    public FileSystemIntellisenseServiceTestFixture(Dictionary<string, MockFileData> fileData, MockFileSystemOptions? options = null)
    {
        options ??= new MockFileSystemOptions { CreateDefaultTempDir = false };
        var fileSystem = new MockFileSystem(fileData, options);
        var reader = new FileSystemReader(fileSystem);
        _logger = new FakeLogger<IFileSystemIntellisenseService>();
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(reader,_logger);
    }

    public FileSystemIntellisenseServiceTestFixture(IFileSystemReader reader)
    {
        _logger = new FakeLogger<IFileSystemIntellisenseService>();
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(reader,_logger);
    }

    public IReadOnlyList<FakeLogRecord> GetLogs() => _logger.Collector.GetSnapshot();

    public CompletionResult GetPathIntellisenseOptions(string path) => _fileSystemIntellisenseService.GetPathIntellisenseOptions(path);
}
