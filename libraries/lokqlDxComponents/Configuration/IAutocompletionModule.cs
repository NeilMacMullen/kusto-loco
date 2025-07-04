
using Intellisense.Configuration;
using Intellisense.FileSystem;
using Jab;

using lokqlDxComponents.Services;
namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageProvider>(Factory = nameof(GetImageProvider))]
[Singleton<AvaloniaImageService>]
[Singleton<IImageSourceLocator>(Factory = nameof(GetImageSourceLocator))]
public interface IAutocompletionModule
{
    public static IImageSourceLocator GetImageSourceLocator(AvaloniaImageService avaloniaImageService) => avaloniaImageService;
    public static IImageProvider GetImageProvider(AvaloniaImageService avaloniaImageService) => avaloniaImageService;

}
