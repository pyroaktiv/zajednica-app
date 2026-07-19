using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.BuildingBlocks.Core.IntegrationEvents;

public record MembershipConfirmed(Guid CommunityId, Guid MembershipId, Guid AccountId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
