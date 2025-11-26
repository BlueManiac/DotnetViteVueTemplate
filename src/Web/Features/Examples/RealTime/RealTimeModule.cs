using Web.Util.Modules;

namespace Web.Features.Examples.RealTime;

public class RealTimeModule : IModule
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapHub<RealTimeHub>("/realTime");
    }
}
