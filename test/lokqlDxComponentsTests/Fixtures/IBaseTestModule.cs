using Jab;
using LogSetup;
using lokqlDxComponents.Configuration;

namespace lokqlDxComponentsTests.Fixtures;

[ServiceProviderModule]
[Import<ILoggingModule>]
[Import<IAutocompletionModule>]
public interface IBaseTestModule
{

}
