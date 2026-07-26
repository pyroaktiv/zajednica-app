using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Membership : AggregateRoot
{
    private readonly List<RoleGrant> _roles = [];

    public Guid AccountId { get; private set; }
    public Guid CommunityId { get; private set; }
    public string? UnitNumber { get; private set; }
    public CertificationStatus CertificationStatus { get; private set; }
    public MembershipState State { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public int Stars { get; private set; }
    public DateTime DateJoined { get; private set; }
    public IReadOnlyList<RoleGrant> Roles => _roles;

    private Membership() { }

    public Membership(Guid accountId, Guid communityId, DateTime now)
    {
        if (accountId == Guid.Empty)
            throw new EntityValidationException("AccountId is required.");
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");

        AccountId = accountId;
        CommunityId = communityId;
        CertificationStatus = CertificationStatus.Unconfirmed;
        State = MembershipState.Active;
        Stars = 0;
        DateJoined = now;
    }

    public static Membership MakeCreator(Guid accountId, Guid communityId, DateTime now)
    {
        var membership = new Membership(accountId, communityId, now);
        membership.Confirm();
        membership.Grant(CommunityRole.Issuer, null, now);
        return membership;
    }

    public void SetUnitNumber(string? unitNumber)
        => UnitNumber = string.IsNullOrWhiteSpace(unitNumber) ? null : unitNumber.Trim();

    public void Confirm()
    {
        if (CertificationStatus == CertificationStatus.Confirmed)
            throw new EntityValidationException("Membership is already confirmed.");

        CertificationStatus = CertificationStatus.Confirmed;
    }

    public void Leave(DateTime now)
    {
        if (State != MembershipState.Active)
            throw new EntityValidationException("Only an active membership can be left.");

        State = MembershipState.Left;
        LeftAt = now;
    }

    public void Rejoin()
    {
        if (State != MembershipState.Left)
            throw new EntityValidationException("Only a membership that was left can be rejoined.");

        State = MembershipState.Active;
        LeftAt = null;
    }

    public void Ban(DateTime now)
    {
        if (State == MembershipState.Banned)
            throw new EntityValidationException("Membership is already banned.");

        State = MembershipState.Banned;
        LeftAt = now;
    }

    public void AddStars(int amount)
    {
        if (amount < 0)
            throw new EntityValidationException("Stars amount cannot be negative.");
        Stars += amount;
    }

    public void Grant(CommunityRole role, Guid? byMembershipId, DateTime now)
    {
        if (!IsActive())
            throw new EntityValidationException("An inactive member cannot hold a role.");
        if (!IsConfirmed())
            throw new EntityValidationException("Only a confirmed member can be granted a role.");
        if (HasRole(role))
            throw new EntityValidationException($"Membership already holds the {role} role.");

        _roles.Add(new RoleGrant(role, byMembershipId, now));
    }

    public void Revoke(CommunityRole role)
    {
        var granted = _roles.SingleOrDefault(r => r.Role == role);
        if (granted is null)
            throw new EntityValidationException($"Membership does not hold the {role} role.");

        _roles.Remove(granted);
    }

    public bool HasRole(CommunityRole role) => _roles.Any(r => r.Role == role);

    public bool IsActive() => State == MembershipState.Active;

    public bool IsConfirmed() => CertificationStatus == CertificationStatus.Confirmed;
}
