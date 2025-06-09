using System;
using System.Threading;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Shares;
using Jab;
using Moq;

namespace IntellisenseTests.Fixtures;

[ServiceProvider]
[Import<IBaseTestModule>]
[Singleton<IFileSystemReader>(Factory = nameof(GetFileSystemReader))]
[Scoped<IShareService>(Factory = nameof(GetShareService))]
public partial class MockedIoTestContainer
{
    public Func<CancellationTokenSource, IShareService> GetShareService = _ => Mock.Of<IShareService>();
    public Func<IFileSystemReader> GetFileSystemReader = Mock.Of<IFileSystemReader>;
}
