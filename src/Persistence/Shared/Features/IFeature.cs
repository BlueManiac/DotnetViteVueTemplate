using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Features;

public interface IFeature
{
    IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration);
}
