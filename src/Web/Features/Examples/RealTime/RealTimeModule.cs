using Web.Util.Modules;

namespace Web.Features.Examples.RealTime;

public class RealTimeModule : IModule
{
    public static void AddServices(IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
    }

    public static void MapRoutes(WebApplication app)
    {
        app.MapHub<RealTimeHub>("/realTime");
    }
}
