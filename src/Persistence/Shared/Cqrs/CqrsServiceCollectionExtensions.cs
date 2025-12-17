using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Cqrs;

public static class CqrsServiceCollectionExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services, params IEnumerable<Assembly> assemblies)
    {
        services.AddScoped<CommandExecutor>();
        services.AddScoped<QueryExecutor>();

        if (!assemblies.Any())
        {
            assemblies = [Assembly.GetAssembly(typeof(CqrsServiceCollectionExtensions))!];
        }

        foreach (var assembly in assemblies)
        {
            RegisterCommandsAndQueries(services, assembly);
        }

        return services;
    }

    private static void RegisterCommandsAndQueries(IServiceCollection services, Assembly assembly)
    {
        var cqrsTypes = new[]
        {
            typeof(ICommand<>),
            typeof(ICommand<,>),
            typeof(IQuery<,>)
        };

        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && cqrsTypes.Contains(i.GetGenericTypeDefinition()));

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, type);
            }
        }
    }
}
