using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.BuildingBlocks.Infrastructure.DomainEvents;

public static class DomainEventsInstaller
{
    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, InMemoryDomainEventDispatcher>();
        services.AddScoped<DomainEventDispatchInterceptor>();
        return services;
    }

    /// <summary>
    /// Scans a module's assembly (its Core, where UseCases live) and registers every
    /// IDomainEventHandler&lt;T&gt; it finds. A module calls this from its AddXModule installer.
    /// </summary>
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IDomainEventHandler<>);

        var handlers =
            from type in assembly.GetTypes()
            where type is { IsAbstract: false, IsInterface: false }
            from iface in type.GetInterfaces()
            where iface.IsGenericType && iface.GetGenericTypeDefinition() == handlerInterface
            select (Service: iface, Implementation: type);

        foreach (var (service, implementation) in handlers)
            services.AddScoped(service, implementation);

        return services;
    }
    
    public static DbContextOptionsBuilder UseDomainEvents(this DbContextOptionsBuilder options, IServiceProvider serviceProvider)
    {
        options.AddInterceptors(serviceProvider.GetRequiredService<DomainEventDispatchInterceptor>());
        return options;
    }
}
