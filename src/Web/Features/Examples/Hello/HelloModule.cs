using Web.Util.Modules;

namespace Web.Features.Examples.Hello;

public class HelloModule : IModule
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/hello", () => new { Hello = "Hello World!" });
    }
}
