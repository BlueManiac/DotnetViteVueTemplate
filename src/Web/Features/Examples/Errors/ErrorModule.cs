using Web.Util.Modules;

namespace Web.Features.Examples.Errors;

public class ErrorModule : IModule
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/error", () =>
        {
            throw new Exception("Server error");
        });
    }
}
