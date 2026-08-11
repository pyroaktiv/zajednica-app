namespace Zajednica.BuildingBlocks.Core.Domain;

// Marks the entity that guards an aggregate's invariants: the only entity a repository loads and saves.
public abstract class AggregateRoot : Entity
{
}
