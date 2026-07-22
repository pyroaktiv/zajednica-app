using Shouldly;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class MembershipBanServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly MembershipBanService _service = new();

    [Fact]
    public void Ban_deactivates_the_membership_and_records_the_blacklist_entry()
    {
        var member = new Membership(Guid.NewGuid(), Guid.NewGuid(), Now);
        member.Confirm();
        member.Grant(CommunityRole.Issuer, null, Now);
        var intentId = Guid.NewGuid();

        var entry = _service.Ban(member, intentId, Now);

        member.State.ShouldBe(MembershipState.Banned);
        member.HasRole(CommunityRole.Issuer).ShouldBeTrue();
        entry.AccountId.ShouldBe(member.AccountId);
        entry.CommunityId.ShouldBe(member.CommunityId);
        entry.IntentId.ShouldBe(intentId);
    }
}
