namespace Web.Util.Modules;
public static class WebApplicationExtensions
{
    public static WebApplicationBuilder AddModule<TModule>(this WebApplicationBuilder builder) where TModule : IModule
    {
        TModule.AddServices(builder.Services, builder.Environment, builder.Configuration);

        return builder;
    }

    public static WebApplication MapModule<TModule>(this WebApplication app) where TModule : IModule
    {
        TModule.MapRoutes(app);

        return app;
    }
}
