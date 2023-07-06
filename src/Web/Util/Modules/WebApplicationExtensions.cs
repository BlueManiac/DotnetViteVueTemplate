using System.Reflection;

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

    public static WebApplicationBuilder AddModules(this WebApplicationBuilder builder)
    {
        var modules = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IModule)));

        foreach (var module in modules)
        {
            module?.GetMethod(nameof(IModule.AddServices), BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[] { builder.Services, builder.Environment, builder.Configuration });
        }

        return builder;
    }

    public static WebApplication MapModules(this WebApplication app)
    {
        var modules = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IModule)));

        foreach (var module in modules)
        {
            module?.GetMethod(nameof(IModule.MapRoutes), BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[] { app });
        }

        return app;
    }
}
