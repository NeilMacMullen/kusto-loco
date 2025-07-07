using Intellisense.Configuration;
using Intellisense.FileSystem;
using Jab;
using lokqlDxComponents.Services;

namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Singleton<IImageLoader, SvgLoader>]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageProvider>(Factory = nameof(GetAssetFolderSvgImageService))]
[Singleton<IImageSourceLocator>(Factory = nameof(GetAssetFolderSvgImageService))]
[Singleton<AssetFolderImageService>]
[Singleton<ImageServiceOptions>(Factory = nameof(GetImageServiceOptions))]
public interface IAutocompletionModule
{
    public static AssetFolderImageService GetAssetFolderSvgImageService(AssetFolderImageService service) => service;

    public static ImageServiceOptions GetImageServiceOptions()
    {
        var opts = new ImageServiceOptions
        {
            AssetsFolder = new("avares://lokqlDx/Assets/FileIcons/"),
            Extension = ".svg"
        };

        return opts;
    }
}
