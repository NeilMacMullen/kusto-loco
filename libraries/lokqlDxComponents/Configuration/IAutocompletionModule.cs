using Intellisense.Configuration;
using Jab;
using lokqlDxComponents.Services;

namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Import<IAppConfigurationModule>]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageProvider, AssetFolderImageProvider>]
[Singleton<IInternalAssetLoader, InternalAssetLoader>]
public interface IAutocompletionModule;