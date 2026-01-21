using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Features;

public static class FeatureExtensions
{
    public static IServiceCollection AddFeature(this IServiceCollection services, IConfiguration configuration, IFeature feature)
    {
        var featureType = feature.GetType();

        if (IsFeatureRegistered(services, featureType))
            return services;

        services.AddSingleton(feature);

        return feature.AddServices(services, configuration);
    }

    public static IServiceCollection AddFeature<TFeature>(this IServiceCollection services, IConfiguration configuration)
        where TFeature : IFeature, new()
    {
        if (IsFeatureRegistered(services, typeof(TFeature)))
            return services;

        var feature = new TFeature();

        services.AddSingleton<IFeature>(feature);

        return feature.AddServices(services, configuration);
    }

    private static bool IsFeatureRegistered(IServiceCollection services, Type featureType)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType != typeof(IFeature))
                continue;

            var implementationType = descriptor.ImplementationType ?? descriptor.ImplementationInstance?.GetType();
            if (implementationType != featureType)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
