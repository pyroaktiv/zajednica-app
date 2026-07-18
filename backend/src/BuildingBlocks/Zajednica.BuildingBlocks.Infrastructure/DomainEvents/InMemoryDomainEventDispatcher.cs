using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.BuildingBlocks.Infrastructure.DomainEvents;

public sealed class InMemoryDomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

        foreach (var handler in serviceProvider.GetServices(handlerType))
        {
            if (handler is null) continue;
            await (Task)handleMethod.Invoke(handler, [domainEvent, ct])!;
        }
    }
}
