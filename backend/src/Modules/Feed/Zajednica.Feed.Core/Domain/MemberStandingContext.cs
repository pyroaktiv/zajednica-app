using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain;

public sealed class MemberStandingContext : ValueObject
{
    public Guid MembershipId { get; }
    public MembershipStatus Status { get; }
    public MembershipRole Role { get; }

    public MemberStandingContext(Guid membershipId, MembershipStatus status, MembershipRole role)
    {
        if (membershipId == Guid.Empty)
            throw new EntityValidationException("A member standing must be identified by their membership.");

        MembershipId = membershipId;
        Status = status;
        Role = role;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MembershipId;
        yield return Status;
        yield return Role;
    }
}
