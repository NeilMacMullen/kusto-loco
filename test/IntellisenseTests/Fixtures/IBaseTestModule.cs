using Intellisense.Configuration;
using Jab;
using LogSetup;

namespace IntellisenseTests.Fixtures;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Import<ILoggingModule>]
public interface IBaseTestModule
{
}
