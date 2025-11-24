using Web.Util.Modules;

namespace Web.Features.Examples.RealTime;

public class RealTimeModule : IModule
{
    public static void MapRoutes(WebApplication app)
    {
        app.MapHub<RealTimeHub>("/api/realTime");
    }
}
