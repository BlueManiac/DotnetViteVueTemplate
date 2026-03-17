using Web.Util.Modules;

namespace Web.Features.Health;

public class HealthModule : IModule
{
    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/health");

        group
            .MapMethods("/ready", ["GET", "HEAD"], async context =>
            {
                context.Response.Headers.ContentType = "text/event-stream";
                context.Response.Headers.CacheControl = "no-cache";

                if (context.Request.Method == "GET")
                {
                    await context.Response.WriteAsync("data: true\n\n");
                    await context.Response.Body.FlushAsync();
                }
            })
            .AllowAnonymous();
    }
}
