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
            assemblies = Enumerable.Append(assemblies, Assembly.GetAssembly(typeof(CqrsServiceCollectionExtensions))!);
        }

        var cqrsTypes = new HashSet<Type>
        {
            typeof(ICommand<>),
            typeof(ICommand<,>),
            typeof(IQuery<,>)
        };

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                foreach (var i in type.GetInterfaces())
                {
                    if (!i.IsGenericType || !cqrsTypes.Contains(i.GetGenericTypeDefinition()))
                        continue;

                    services.AddScoped(i, type);
                }
            }
        }

        return services;
    }
}
