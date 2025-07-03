
using Intellisense.Configuration;
using Jab;

using lokqlDxComponents.Services;


namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageService, ImageService>]
[Import<IIntellisenseModule>]
public interface IAutocompletionModule;
