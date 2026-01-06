using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Features;

public static class FeatureExtensions
{
    public static IServiceCollection AddFeature<TFeature>(this IServiceCollection services, IConfiguration configuration)
        where TFeature : IFeature
    {
        return TFeature.AddServices(services, configuration);
    }
}
