using Intellisense.Configuration;
using Jab;
using lokqlDxComponents.Services;

namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Singleton<IImageLoader, SvgLoader>]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageProvider>(Factory = nameof(GetAssetFolderSvgImageService))]
[Singleton<AssetFolderImageProvider>]
[Singleton<ImageServiceOptions>(Factory = nameof(GetImageServiceOptions))]
public interface IAutocompletionModule
{
    public static AssetFolderImageProvider GetAssetFolderSvgImageService(AssetFolderImageProvider provider) => provider;

    public static ImageServiceOptions GetImageServiceOptions()
    {
        var opts = new ImageServiceOptions
        {
            AssetsFolder = new("avares://lokqlDx/Assets/CompletionIcons/"),
            Extension = ".svg"
        };

        return opts;
    }
}
