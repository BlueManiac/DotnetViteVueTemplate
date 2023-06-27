using Web.Util.Modules;

namespace Web.Features.Examples.Hello;
public class HelloModule : IModule
{
    public static void AddServices(IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public static void MapRoutes(WebApplication app)
    {
        app.MapGet("/hello", () => new { Hello = "Hello World!" });
    }
}
