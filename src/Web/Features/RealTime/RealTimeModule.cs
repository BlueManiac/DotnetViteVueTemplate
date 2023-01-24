using Web.Features.Realtime;
using Web.Util.Modules;

namespace Web.Features.RealTime;

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
