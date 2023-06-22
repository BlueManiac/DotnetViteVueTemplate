using Web.Util.Modules;

namespace Web.Features.Examples.Errors;
public class ErrorModule : IModule
{
    public static void AddServices(IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public static void MapRoutes(WebApplication app)
    {
        app.MapGet("/error", () =>
        {
            throw new Exception("error");
        });
    }
}
