namespace Zajednica.BuildingBlocks.Core.Domain;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
