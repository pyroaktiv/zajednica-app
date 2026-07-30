using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Dto.Memberships;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class CommunityLifecycleTests : BaseCommunityIntegrationTest
{
    public CommunityLifecycleTests(CommunityTestFactory factory) : base(factory) { }

    [Fact]
    public void Creating_a_community_makes_the_creator_a_confirmed_issuer()
    {
        using var scope = Factory.Services.CreateScope();
        var accountId = NewAccount(scope);

        var community = CreateCommunity(scope, accountId);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var membership = db.Memberships.Single(m => m.AccountId == accountId && m.CommunityId == community.Id);
        membership.IsConfirmed().ShouldBeTrue();
        membership.HasRole(CommunityRole.Issuer).ShouldBeTrue();
        membership.HasRole(CommunityRole.Manager).ShouldBeFalse();
    }

    [Fact]
    public void The_creator_cannot_change_community_details_without_being_the_manager()
    {
        using var scope = Factory.Services.CreateScope();
        var accountId = NewAccount(scope);
        var community = CreateCommunity(scope, accountId);

        var request = new UpdateCommunityRequest("Zgrada 2", community.Address, null, null, null);

        Should.Throw<ForbiddenException>(() =>
            Communities(scope, accountId).Update(community.Id, request));
    }

    [Fact]
    public void Joining_by_qr_code_leaves_the_newcomer_unconfirmed()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = CreateCommunity(scope, creatorId);
        var qrToken = QrToken(scope, creatorId, community.Id);

        var joined = Join(scope, newcomerId, qrToken);

        joined.CommunityId.ShouldBe(community.Id);
        joined.CommunityName.ShouldBe(community.Name);
        joined.IsConfirmed.ShouldBeFalse();

        var mine = Value<MemberProfileDto>(
            (Members(scope, newcomerId).GetMine(community.Id)).Result!);
        mine.Stars.ShouldBeNull();
        mine.Roles.ShouldBeEmpty();

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.Memberships.Single(m => m.Id == joined.MembershipId).State.ShouldBe(MembershipState.Active);
    }

    [Fact]
    public void Rejoining_keeps_the_confirmation_earned_before_leaving()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = CreateCommunity(scope, creatorId);
        var qrToken = QrToken(scope, creatorId, community.Id);
        Join(scope, memberId, qrToken);
        Certify(scope, creatorId, memberId, community.Id);

        Communities(scope, memberId).Leave(community.Id);
        var rejoined = Join(scope, memberId, qrToken);

        rejoined.IsConfirmed.ShouldBeTrue();

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.Memberships.Single(m => m.Id == rejoined.MembershipId).State.ShouldBe(MembershipState.Active);
    }

    [Fact]
    public void Leaving_keeps_every_role_the_member_held()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var community = CreateCommunity(scope, creatorId);

        Communities(scope, creatorId).Leave(community.Id);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var membership = db.Memberships.Single(m => m.AccountId == creatorId && m.CommunityId == community.Id);
        membership.HasRole(CommunityRole.Issuer).ShouldBeTrue();
        membership.State.ShouldBe(MembershipState.Left);
    }

    [Fact]
    public void A_banned_account_cannot_join_again()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var bannedId = NewAccount(scope);
        var community = CreateCommunity(scope, creatorId);
        var qrToken = QrToken(scope, creatorId, community.Id);
        var joined = Join(scope, bannedId, qrToken);

        var db = Db(scope);
        var membership = db.Memberships.Single(m => m.Id == joined.MembershipId);
        membership.Ban(null, DateTime.UtcNow);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        Should.Throw<ForbiddenException>(() => Join(scope, bannedId, qrToken));
    }
}
