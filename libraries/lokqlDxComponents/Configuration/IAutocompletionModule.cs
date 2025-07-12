using Intellisense.Configuration;
using Jab;
using lokqlDxComponents.Services;

namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Import<IAppConfigurationModule>]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageProvider>(Factory = nameof(GetAssetFolderImageProvider))]
[Singleton<AssetFolderImageProvider>]
[Singleton<IInternalAssetLoader, InternalAssetLoader>]
public interface IAutocompletionModule
{
    public static IImageProvider GetAssetFolderImageProvider(AssetFolderImageProvider assetFolderImageProvider) => assetFolderImageProvider;
}