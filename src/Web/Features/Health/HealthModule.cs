using Web.Util.Modules;

namespace Web.Features.Health;

public class HealthModule : IModule
{
    public static void AddServices(WebApplicationBuilder builder)
    {
    }

    public static void MapRoutes(WebApplication app)
    {
        var group = app.MapGroup("/health");

        group
            .MapGet("/ready", async context =>
            {
                context.Response.Headers.ContentType = "text/event-stream";
                context.Response.Headers.CacheControl = "no-cache";

                await context.Response.WriteAsync("data: true\n\n");
                await context.Response.Body.FlushAsync();
            })
            .AllowAnonymous();
    }
}
