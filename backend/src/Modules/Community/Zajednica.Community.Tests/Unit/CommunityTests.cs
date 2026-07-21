using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class CommunityTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Core.Domain.Community NewCommunity()
        => new("Zgrada 1", new Address("Bulevar", "12", new Coordinates(45.25m, 19.83m)), "qr-token", Now);

    private static Membership MemberOf(Core.Domain.Community community)
    {
        var member = new Membership(Guid.NewGuid(), community.Id, Now);
        member.Confirm();
        return member;
    }

    [Fact]
    public void UpdateDetails_is_allowed_for_the_manager()
    {
        var community = NewCommunity();
        var manager = MemberOf(community);
        manager.Grant(CommunityRole.Manager, null, Now);

        community.UpdateDetails(manager, "Zgrada 2", community.Address, null, null, "160-1234-56");

        community.Name.ShouldBe("Zgrada 2");
        community.BankAccountNumber.ShouldBe("160-1234-56");
    }

    [Fact]
    public void UpdateDetails_is_forbidden_for_the_creator_who_only_holds_the_issuer_role()
    {
        var community = NewCommunity();
        var creator = Membership.Creator(Guid.NewGuid(), community.Id, Now);

        Should.Throw<ForbiddenException>(() =>
            community.UpdateDetails(creator, "Zgrada 2", community.Address, null, null, null));
    }

    [Fact]
    public void UpdateDetails_is_forbidden_for_a_manager_of_another_community()
    {
        var community = NewCommunity();
        var foreignManager = new Membership(Guid.NewGuid(), Guid.NewGuid(), Now);
        foreignManager.Confirm();
        foreignManager.Grant(CommunityRole.Manager, null, Now);

        Should.Throw<ForbiddenException>(() =>
            community.UpdateDetails(foreignManager, "Zgrada 2", community.Address, null, null, null));
    }

    [Fact]
    public void UpdateDetails_is_forbidden_once_the_manager_leaves()
    {
        var community = NewCommunity();
        var manager = MemberOf(community);
        manager.Grant(CommunityRole.Manager, null, Now);
        manager.Leave(Now);

        Should.Throw<ForbiddenException>(() =>
            community.UpdateDetails(manager, "Zgrada 2", community.Address, null, null, null));
    }
}
