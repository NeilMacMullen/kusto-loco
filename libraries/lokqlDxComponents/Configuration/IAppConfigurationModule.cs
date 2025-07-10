using Jab;

namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Singleton<AppOptions>(Factory = nameof(GetAppOptions))]
public interface IAppConfigurationModule
{


    public static AppOptions GetAppOptions()
    {
        var opts = new AppOptions
        {
            AssemblyName = "lokqlDx"
        };

        return opts;
    }
}
