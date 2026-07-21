using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class ManagerElectionServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly ManagerElectionService _service = new();

    private static Membership Confirmed(Guid communityId)
    {
        var member = new Membership(Guid.NewGuid(), communityId, Now);
        member.Confirm();
        return member;
    }

    [Fact]
    public void Elect_grants_the_role_when_the_community_has_no_manager()
    {
        var newManager = Confirmed(Guid.NewGuid());

        _service.Elect(null, newManager, Now);

        newManager.HasRole(CommunityRole.Manager).ShouldBeTrue();
    }

    [Fact]
    public void Elect_moves_the_role_from_the_current_manager()
    {
        var communityId = Guid.NewGuid();
        var currentManager = Confirmed(communityId);
        var newManager = Confirmed(communityId);
        _service.Elect(null, currentManager, Now);

        _service.Elect(currentManager, newManager, Now);

        currentManager.HasRole(CommunityRole.Manager).ShouldBeFalse();
        newManager.HasRole(CommunityRole.Manager).ShouldBeTrue();
    }

    [Fact]
    public void Elect_leaves_the_current_manager_untouched_when_the_candidate_is_ineligible()
    {
        var communityId = Guid.NewGuid();
        var currentManager = Confirmed(communityId);
        var unconfirmed = new Membership(Guid.NewGuid(), communityId, Now);
        _service.Elect(null, currentManager, Now);

        Should.Throw<EntityValidationException>(() => _service.Elect(currentManager, unconfirmed, Now));
    }

    [Fact]
    public void Elect_rejects_a_candidate_from_another_community()
    {
        var currentManager = Confirmed(Guid.NewGuid());
        var newManager = Confirmed(Guid.NewGuid());
        _service.Elect(null, currentManager, Now);

        Should.Throw<EntityValidationException>(() => _service.Elect(currentManager, newManager, Now));
    }

    [Fact]
    public void Elect_rejects_re_electing_the_same_membership()
    {
        var currentManager = Confirmed(Guid.NewGuid());
        _service.Elect(null, currentManager, Now);

        Should.Throw<EntityValidationException>(() => _service.Elect(currentManager, currentManager, Now));
    }
}
