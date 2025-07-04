using Intellisense.FileSystem;
using Jab;
using LogSetup;
using lokqlDxComponents.Configuration;
using Moq;

namespace lokqlDxComponentsTests.Fixtures;

[ServiceProvider]
[Import<IAutocompletionModule>]
[Import<ILoggingModule>]
[Singleton<IFileSystemReader>(Factory = nameof(GetFileSystemReader))]
public partial class CompletionManagerTestContainer
{
    public Func<IServiceProvider,IFileSystemReader> GetFileSystemReader { get; set; } = _ => Mock.Of<IFileSystemReader>();
}
