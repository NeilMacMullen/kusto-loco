
using Intellisense.Configuration;
using Jab;

using lokqlDxComponents.Services;


namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Transient<IntellisenseClientAdapter>]
[Import<IIntellisenseModule>]
public interface IAutocompletionModule;
