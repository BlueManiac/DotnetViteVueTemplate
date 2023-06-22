using Web.Util.Modules;

namespace Web.Features.Examples.Home;
public class HomeModule : IModule
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
