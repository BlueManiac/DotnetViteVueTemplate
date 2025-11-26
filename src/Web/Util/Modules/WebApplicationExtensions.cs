using System.Reflection;

namespace Web.Util.Modules;

public static class WebApplicationExtensions
{
    public static WebApplicationBuilder AddModule<TModule>(this WebApplicationBuilder builder) where TModule : IModule
    {
        TModule.AddServices(builder);

        return builder;
    }

    public static WebApplication MapModule<TModule>(this WebApplication app, IEndpointRouteBuilder? routes = null) where TModule : IModule
    {
        TModule.MapRoutes(routes ?? app);

        return app;
    }

    public static WebApplicationBuilder AddModules(this WebApplicationBuilder builder, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !typeof(IModule).IsAssignableFrom(type))
                continue;

            var method = type.GetMethod(nameof(IModule.AddServices), BindingFlags.Static | BindingFlags.Public);

            method?.Invoke(null, [builder]);
        }

        return builder;
    }

    public static WebApplication MapModules(this WebApplication app, IEndpointRouteBuilder routes, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !typeof(IModule).IsAssignableFrom(type))
                continue;

            var method = type?.GetMethod(nameof(IModule.MapRoutes), BindingFlags.Static | BindingFlags.Public);

            method?.Invoke(null, [routes]);
        }

        return app;
    }
}
