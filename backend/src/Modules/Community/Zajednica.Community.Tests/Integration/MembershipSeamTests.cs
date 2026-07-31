using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class MembershipSeamTests : BaseCommunityIntegrationTest
{
    public MembershipSeamTests(CommunityTestFactory factory) : base(factory) { }

    private static IInternalMembershipAccessService Access(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalMembershipAccessService>();

    private static IInternalMembershipAudienceService Audience(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalMembershipAudienceService>();

    private static IInternalIntentOutcomeService Outcome(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalIntentOutcomeService>();

    [Fact]
    public void An_unconfirmed_member_passes_only_the_membership_requirement()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        Join(scope, newcomerId, QrToken(scope, issuerId, community.Id));

        var access = Access(scope);

        access.RequireActiveMembershipId(newcomerId, community.Id).ShouldNotBe(Guid.Empty);
        access.RequireUnconfirmedMembershipId(newcomerId, community.Id).ShouldNotBe(Guid.Empty);
        Should.Throw<ForbiddenException>(() => access.RequireConfirmedMembershipId(newcomerId, community.Id));
        Should.Throw<ForbiddenException>(() => access.RequireUnconfirmedMembershipId(issuerId, community.Id));
    }

    [Fact]
    public void A_stranger_is_not_a_member()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var strangerId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);

        Should.Throw<ForbiddenException>(() =>
            Access(scope).RequireActiveMembershipId(strangerId, community.Id));
    }

    [Fact]
    public void Only_a_confirmed_member_holding_the_issuer_role_may_certify()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        var qrToken = QrToken(scope, issuerId, community.Id);
        Join(scope, memberId, qrToken);
        Join(scope, newcomerId, qrToken);
        var member = Certify(scope, issuerId, memberId, community.Id);

        var db = Db(scope);
        var issuer = db.Memberships.Single(m => m.AccountId == issuerId && m.CommunityId == community.Id);
        var newcomer = db.Memberships.Single(m => m.AccountId == newcomerId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();

        var access = Access(scope);

        access.CanIssueCertifications(community.Id, issuer.Id).ShouldBeTrue();
        access.CanIssueCertifications(community.Id, member.MembershipId).ShouldBeFalse();
        access.IsConfirmedMemberOf(community.Id, member.MembershipId).ShouldBeTrue();
        access.IsConfirmedMemberOf(community.Id, newcomer.Id).ShouldBeFalse();
        access.IsConfirmedMemberOf(Guid.NewGuid(), member.MembershipId).ShouldBeFalse();
    }

    [Fact]
    public void A_banned_membership_leaves_the_audience_and_every_requirement()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        Join(scope, memberId, QrToken(scope, issuerId, community.Id));
        var member = Certify(scope, issuerId, memberId, community.Id);

        var db = Db(scope);
        var issuer = db.Memberships.Single(m => m.AccountId == issuerId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();

        var audience = Audience(scope);
        audience.GetConfirmedAccountIds(community.Id, issuer.Id).ShouldBe([memberId]);
        audience.GetConfirmedCount(community.Id).ShouldBe(2);

        Outcome(scope).Ban(member.MembershipId, Guid.NewGuid());

        db.ChangeTracker.Clear();
        audience.GetConfirmedAccountIds(community.Id, null).ShouldBe([issuerId]);
        Should.Throw<ForbiddenException>(() => Access(scope).RequireActiveMembershipId(memberId, community.Id));
    }

    [Fact]
    public void A_muted_member_may_still_read_and_vote_but_fails_every_requirement_that_writes()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        Join(scope, memberId, QrToken(scope, issuerId, community.Id));
        var member = Certify(scope, issuerId, memberId, community.Id);

        Outcome(scope).Mute(member.MembershipId);
        Db(scope).ChangeTracker.Clear();

        var access = Access(scope);

        access.RequireConfirmedMembershipId(memberId, community.Id).ShouldBe(member.MembershipId);
        access.RequireActiveMembershipId(memberId, community.Id).ShouldBe(member.MembershipId);
        Should.Throw<ForbiddenException>(() => access.RequireUnmutedConfirmedMembershipId(memberId, community.Id));
        Should.Throw<ForbiddenException>(() => access.RequireUnmutedActiveMembershipId(memberId, community.Id));

        access.RequireUnmutedConfirmedMembershipId(issuerId, community.Id).ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void The_manager_is_found_by_role_and_moves_with_the_election()
    {
        using var scope = Factory.Services.CreateScope();
        var firstId = NewAccount(scope);
        var secondId = NewAccount(scope);
        var community = CreateCommunity(scope, firstId);
        Join(scope, secondId, QrToken(scope, firstId, community.Id));
        var second = Certify(scope, firstId, secondId, community.Id);

        var db = Db(scope);
        var first = db.Memberships.Single(m => m.AccountId == firstId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();

        var audience = Audience(scope);
        audience.GetManagerAccountId(community.Id).ShouldBeNull();

        Outcome(scope).ElectManager(first.Id);
        db.ChangeTracker.Clear();
        audience.GetManagerAccountId(community.Id).ShouldBe(firstId);

        Outcome(scope).ElectManager(second.MembershipId);
        db.ChangeTracker.Clear();
        audience.GetManagerAccountId(community.Id).ShouldBe(secondId);
    }
}
