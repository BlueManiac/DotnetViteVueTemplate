using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Features;

public static class FeatureExtensions
{
    public static IServiceCollection AddFeature(this IServiceCollection services, IFeature feature, IConfiguration configuration)
    {
        return feature.AddServices(services, configuration);
    }

    public static IServiceCollection AddFeature<TFeature>(this IServiceCollection services, IConfiguration configuration)
        where TFeature : IFeature, new()
    {
        var feature = new TFeature();
        return feature.AddServices(services, configuration);
    }
}
