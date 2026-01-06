using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Features;

public interface IFeature
{
    static abstract IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration);
}
