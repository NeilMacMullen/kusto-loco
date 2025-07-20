using Intellisense.FileSystem;
using Jab;
using lokqlDxComponents.Services;
using Moq;

namespace lokqlDxComponentsTests.Fixtures;

[ServiceProvider]
[Import<IBaseTestModule>]
[Singleton<IFileSystemReader>(Factory = nameof(GetFileSystemReader))]
[Singleton<IAssetService>(Factory = nameof(GetAssetLoader))]
public partial class CompletionManagerTestContainer
{
    public Func<IServiceProvider,IFileSystemReader> GetFileSystemReader { get; set; } = _ => Mock.Of<IFileSystemReader>();
    public Func<IServiceProvider, IAssetService> GetAssetLoader { get; set; } = _ => Mock.Of<IAssetService>();
}
