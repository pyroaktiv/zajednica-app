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

    private static Membership ConfirmedMember()
    {
        var member = NewMember();
        member.Confirm();
        return member;
    }

    [Fact]
    public void Confirm_flips_status_to_confirmed()
    {
        var member = NewMember();

        member.Confirm();

        member.CertificationStatus.ShouldBe(CertificationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_is_rejected_when_already_confirmed()
    {
        var member = ConfirmedMember();

        Should.Throw<EntityValidationException>(() => member.Confirm());
    }

    [Fact]
    public void Creator_is_a_confirmed_issuer_and_nothing_more()
    {
        var creator = Membership.Creator(Account, CommunityId, Now);

        creator.CertificationStatus.ShouldBe(CertificationStatus.Confirmed);
        creator.HasRole(CommunityRole.Issuer).ShouldBeTrue();
        creator.HasRole(CommunityRole.Manager).ShouldBeFalse();
    }

    [Fact]
    public void Grant_requires_a_confirmed_member()
    {
        var member = NewMember();

        Should.Throw<EntityValidationException>(() => member.Grant(CommunityRole.Issuer, null, Now));

        member.Confirm();
        member.Grant(CommunityRole.Issuer, null, Now);
        member.HasRole(CommunityRole.Issuer).ShouldBeTrue();
    }

    [Fact]
    public void Grant_records_who_granted_the_role_and_when()
    {
        var grantedBy = Guid.NewGuid();
        var member = ConfirmedMember();

        member.Grant(CommunityRole.Issuer, grantedBy, Now);

        var role = member.Roles.Single();
        role.Role.ShouldBe(CommunityRole.Issuer);
        role.GrantedByMembershipId.ShouldBe(grantedBy);
        role.GrantedAt.ShouldBe(Now);
    }

    [Fact]
    public void Grant_is_rejected_when_the_role_is_already_held()
    {
        var member = ConfirmedMember();
        member.Grant(CommunityRole.Manager, null, Now);

        Should.Throw<EntityValidationException>(() => member.Grant(CommunityRole.Manager, null, Now));
    }

    [Fact]
    public void Revoke_is_rejected_when_the_role_is_not_held()
    {
        var member = ConfirmedMember();

        Should.Throw<EntityValidationException>(() => member.Revoke(CommunityRole.Manager));
    }

    [Fact]
    public void Leave_preserves_confirmation_and_stars_but_drops_all_roles()
    {
        var member = ConfirmedMember();
        member.Grant(CommunityRole.Manager, null, Now);
        member.AddStars(50);

        member.Leave(Now);

        member.State.ShouldBe(MembershipState.Left);
        member.LeftAt.ShouldBe(Now);
        member.Roles.ShouldBeEmpty();
        member.CertificationStatus.ShouldBe(CertificationStatus.Confirmed);
        member.Stars.ShouldBe(50);
    }

    [Fact]
    public void Ban_drops_all_roles()
    {
        var member = ConfirmedMember();
        member.Grant(CommunityRole.Manager, null, Now);

        member.Ban(Now);

        member.State.ShouldBe(MembershipState.Banned);
        member.Roles.ShouldBeEmpty();
    }

    [Fact]
    public void Rejoin_restores_an_active_membership_without_its_former_roles()
    {
        var member = ConfirmedMember();
        member.Grant(CommunityRole.Issuer, null, Now);
        member.Leave(Now);

        member.Rejoin();

        member.IsActive().ShouldBeTrue();
        member.LeftAt.ShouldBeNull();
        member.Roles.ShouldBeEmpty();
    }

    [Fact]
    public void Rejoin_is_rejected_for_a_banned_membership()
    {
        var member = ConfirmedMember();
        member.Ban(Now);

        Should.Throw<EntityValidationException>(() => member.Rejoin());
    }

    [Fact]
    public void Grant_is_rejected_for_an_inactive_membership()
    {
        var member = ConfirmedMember();
        member.Leave(Now);

        Should.Throw<EntityValidationException>(() => member.Grant(CommunityRole.Issuer, null, Now));
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
