using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class CommunityLifecycleTests : BaseCommunityIntegrationTest
{
    public CommunityLifecycleTests(CommunityTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Creating_a_community_makes_the_creator_a_confirmed_issuer()
    {
        using var scope = Factory.Services.CreateScope();
        var accountId = NewAccount(scope);

        var community = await CreateCommunityAsync(scope, accountId);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var membership = db.Memberships.Single(m => m.AccountId == accountId && m.CommunityId == community.Id);
        membership.IsConfirmed().ShouldBeTrue();
        membership.HasRole(CommunityRole.Issuer).ShouldBeTrue();
        membership.HasRole(CommunityRole.Manager).ShouldBeFalse();
    }

    [Fact]
    public async Task The_creator_cannot_change_community_details_without_being_the_manager()
    {
        using var scope = Factory.Services.CreateScope();
        var accountId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, accountId);

        var request = new UpdateCommunityRequest("Zgrada 2", community.Address, null, null, null);

        await Should.ThrowAsync<ForbiddenException>(() =>
            Communities(scope, accountId).Update(community.Id, request, default));
    }

    [Fact]
    public async Task Joining_by_qr_code_leaves_the_newcomer_unconfirmed()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, creatorId);
        var qrToken = await QrTokenAsync(scope, creatorId, community.Id);

        var membership = await JoinAsync(scope, newcomerId, qrToken);

        membership.Member.IsConfirmed.ShouldBeFalse();
        membership.Member.Stars.ShouldBeNull();
        membership.Member.Roles.ShouldBeEmpty();
        membership.State.ShouldBe(MembershipState.Active.ToString());
    }

    [Fact]
    public async Task Rejoining_keeps_the_confirmation_earned_before_leaving()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, creatorId);
        var qrToken = await QrTokenAsync(scope, creatorId, community.Id);
        await JoinAsync(scope, memberId, qrToken);
        await CertifyAsync(scope, creatorId, memberId, community.Id);

        await Communities(scope, memberId).Leave(community.Id, default);
        var rejoined = await JoinAsync(scope, memberId, qrToken);

        rejoined.Member.IsConfirmed.ShouldBeTrue();
        rejoined.State.ShouldBe(MembershipState.Active.ToString());
    }

    [Fact]
    public async Task Leaving_strips_every_role_the_member_held()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, creatorId);

        await Communities(scope, creatorId).Leave(community.Id, default);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var membership = db.Memberships.Single(m => m.AccountId == creatorId && m.CommunityId == community.Id);
        membership.Roles.ShouldBeEmpty();
        membership.State.ShouldBe(MembershipState.Left);
    }

    [Fact]
    public async Task A_blacklisted_account_cannot_join_again()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var bannedId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, creatorId);
        var qrToken = await QrTokenAsync(scope, creatorId, community.Id);
        await JoinAsync(scope, bannedId, qrToken);

        var db = Db(scope);
        db.BlacklistEntries.Add(new BlacklistEntry(community.Id, bannedId, DateTime.UtcNow));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await Should.ThrowAsync<ForbiddenException>(() => JoinAsync(scope, bannedId, qrToken));
    }
}
