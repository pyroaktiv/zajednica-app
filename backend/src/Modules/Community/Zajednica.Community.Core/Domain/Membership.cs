using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Membership : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public Guid CommunityId { get; private set; }
    public string? UnitNumber { get; private set; }
    public MembershipStatus Status { get; private set; }
    public bool IsActive { get; private set; }
    public bool CertificateIssuer { get; private set; }
    public DateTime? MutedUntil { get; private set; }
    public int Stars { get; private set; }
    public DateTime DateJoined { get; private set; }

    // EF
    private Membership() { }

    public Membership(Guid accountId, Guid communityId, DateTime now)
    {
        if (accountId == Guid.Empty)
            throw new EntityValidationException("AccountId is required.");
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");

        AccountId = accountId;
        CommunityId = communityId;
        Status = MembershipStatus.Unconfirmed;
        IsActive = true;
        CertificateIssuer = false;
        Stars = 0;
        DateJoined = now;
    }

    // The community creator joins already confirmed and with the right to issue certificates.
    public static Membership Founder(Guid accountId, Guid communityId, DateTime now)
    {
        var membership = new Membership(accountId, communityId, now)
        {
            Status = MembershipStatus.Confirmed,
            CertificateIssuer = true
        };
        return membership;
    }
    
    public void SetUnitNumber(string? unitNumber)
        => UnitNumber = string.IsNullOrWhiteSpace(unitNumber) ? null : unitNumber.Trim();

    public void Confirm()
    {
        if (Status == MembershipStatus.Confirmed)
            throw new EntityValidationException("Membership is already confirmed.");

        Status = MembershipStatus.Confirmed;
    }

    public void GrantIssuerRight()
    {
        if (Status != MembershipStatus.Confirmed)
            throw new EntityValidationException("Only a confirmed member can be granted the issuer right.");
        CertificateIssuer = true;
    }
    
    public void Leave()
    {
        if (!IsActive)
            throw new EntityValidationException("Membership is already inactive.");
        IsActive = false;
    }

    public void Mute(DateTime until) => MutedUntil = until;
    
    public void Ban() => IsActive = false;

    public void AddStars(int amount)
    {
        if (amount < 0)
            throw new EntityValidationException("Stars amount cannot be negative.");
        Stars += amount;
    }

    public bool IsMuted(DateTime now) => MutedUntil.HasValue && now < MutedUntil.Value;
}
