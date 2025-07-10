using Intellisense.Configuration;
using Jab;
using lokqlDxComponents.Services;

namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Import<IAppConfigurationModule>]
[Singleton<IImageLoader, SvgLoader>]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageProvider, AssetFolderImageProvider>]
[Singleton<AssetFolderImageProvider>]
public interface IAutocompletionModule;
