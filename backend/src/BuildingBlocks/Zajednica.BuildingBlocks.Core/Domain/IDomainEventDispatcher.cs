namespace Zajednica.BuildingBlocks.Core.Domain;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
