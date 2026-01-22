using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection AddEntityConfigurations(this IServiceCollection services, params IEnumerable<Assembly> assemblies)
    {
        if (!assemblies.Any())
        {
            assemblies = Enumerable.Append(assemblies, Assembly.GetExecutingAssembly());
        }

        var interfaceType = typeof(IEntityConfiguration);

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract || !interfaceType.IsAssignableFrom(type))
                    continue;

                services.AddSingleton(interfaceType, type);
            }
        }

        return services;
    }

    public static IServiceCollection AddDataSeeders(this IServiceCollection services, params IEnumerable<Assembly> assemblies)
    {
        if (!assemblies.Any())
        {
            assemblies = Enumerable.Append(assemblies, Assembly.GetExecutingAssembly());
        }

        var interfaceType = typeof(IDatabaseSeeder);

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract || !interfaceType.IsAssignableFrom(type))
                    continue;

                services.AddTransient(interfaceType, type);
            }
        }

        return services;
    }
}
