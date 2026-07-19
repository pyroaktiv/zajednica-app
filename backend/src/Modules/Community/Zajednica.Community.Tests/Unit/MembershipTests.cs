using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class MembershipTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Account = Guid.NewGuid();
    private static readonly Guid CommunityId = Guid.NewGuid();

    private static Membership NewMember() => new(Account, CommunityId, Now);

    [Fact]
    public void Confirm_flips_status_to_confirmed()
    {
        var member = NewMember();

        member.Confirm();

        member.Status.ShouldBe(MembershipStatus.Confirmed);
    }

    [Fact]
    public void Confirm_is_rejected_when_already_confirmed()
    {
        var member = NewMember();
        member.Confirm();

        Should.Throw<EntityValidationException>(() => member.Confirm());
    }

    [Fact]
    public void Founder_is_a_confirmed_issuer()
    {
        var founder = Membership.Founder(Account, CommunityId, Now);

        founder.Status.ShouldBe(MembershipStatus.Confirmed);
        founder.CertificateIssuer.ShouldBeTrue();
    }

    [Fact]
    public void GrantIssuerRight_requires_a_confirmed_member()
    {
        var member = NewMember(); // unconfirmed

        Should.Throw<EntityValidationException>(() => member.GrantIssuerRight());

        member.Confirm();
        member.GrantIssuerRight();
        member.CertificateIssuer.ShouldBeTrue();
    }

    [Fact]
    public void Leave_preserves_confirmation_and_stars_but_deactivates()
    {
        var member = NewMember();
        member.Confirm();
        member.AddStars(50);

        member.Leave();

        member.IsActive.ShouldBeFalse();
        member.Status.ShouldBe(MembershipStatus.Confirmed); // confirmation is kept on leaving
        member.Stars.ShouldBe(50);
    }

    [Fact]
    public void AddStars_accumulates_and_rejects_negative()
    {
        var member = NewMember();
        member.AddStars(50);
        member.AddStars(200);
        member.Stars.ShouldBe(250);

        Should.Throw<EntityValidationException>(() => member.AddStars(-1));
    }

    [Fact]
    public void IsMuted_is_true_only_before_the_deadline()
    {
        var member = NewMember();
        member.Mute(Now.AddDays(7));

        member.IsMuted(Now.AddDays(1)).ShouldBeTrue();
        member.IsMuted(Now.AddDays(8)).ShouldBeFalse();
    }
}
