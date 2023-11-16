using Web.Util.Modules;

namespace Web.Features.Examples.Hello;
public class HelloModule : IModule
{
    public static void AddServices(WebApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void MapRoutes(WebApplication app)
    {
        app.MapGet("/api/hello", () => new { Hello = "Hello World!" });
    }
}
