using Microsoft.Extensions.DependencyInjection;

namespace Intellisense;

public static class ServiceCollectionExtensions
{
    public static void TryConfigure<T>(this IServiceCollection services, Action<T> configure) where T : class
    {
        var type = typeof(T);
        if (services.Any(x => x.ServiceType == type))
        {
            return;
        }
        services.Configure(configure);
    }
}
