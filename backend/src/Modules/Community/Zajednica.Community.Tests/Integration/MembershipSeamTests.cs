using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class MembershipSeamTests : BaseCommunityIntegrationTest
{
    public MembershipSeamTests(CommunityTestFactory factory) : base(factory) { }

    private static IInternalMembershipFactsService Membership(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalMembershipFactsService>();

    private static IInternalMembershipAudienceService Audience(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalMembershipAudienceService>();

    private static IInternalMembershipCommandService Command(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalMembershipCommandService>();

    [Fact]
    public void An_unconfirmed_member_is_active_but_not_confirmed()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        Join(scope, newcomerId, QrToken(scope, issuerId, community.Id));

        var svc = Membership(scope);

        var newcomer = svc.FindWithAccountInCommunity(newcomerId, community.Id);
        newcomer.ShouldNotBeNull();
        newcomer.IsActive.ShouldBeTrue();
        newcomer.IsConfirmed.ShouldBeFalse();
        newcomer.MembershipId.ShouldNotBe(Guid.Empty);

        svc.FindWithAccountInCommunity(issuerId, community.Id)!.IsConfirmed.ShouldBeTrue();
    }

    [Fact]
    public void A_stranger_has_no_membership_facts()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var strangerId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);

        Membership(scope).FindWithAccountInCommunity(strangerId, community.Id).ShouldBeNull();
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

        var svc = Membership(scope);

        svc.FindByMembershipInCommunity(community.Id, issuer.Id)!.CanIssueCertifications.ShouldBeTrue();
        svc.FindByMembershipInCommunity(community.Id, member.MembershipId)!.CanIssueCertifications.ShouldBeFalse();
        (svc.FindByMembershipInCommunity(community.Id, member.MembershipId) is { IsActive: true, IsConfirmed: true }).ShouldBeTrue();
        (svc.FindByMembershipInCommunity(community.Id, newcomer.Id) is { IsActive: true, IsConfirmed: true }).ShouldBeFalse();
        svc.FindByMembershipInCommunity(Guid.NewGuid(), member.MembershipId).ShouldBeNull();
    }

    [Fact]
    public void A_banned_membership_leaves_the_audience_and_is_no_longer_active()
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

        Command(scope).Ban(member.MembershipId, Guid.NewGuid());

        db.ChangeTracker.Clear();
        audience.GetConfirmedAccountIds(community.Id, null).ShouldBe([issuerId]);
        (Membership(scope).FindWithAccountInCommunity(memberId, community.Id) is { IsActive: true }).ShouldBeFalse();
    }

    [Fact]
    public void A_muted_member_stays_confirmed_and_active_but_carries_a_future_mute()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        Join(scope, memberId, QrToken(scope, issuerId, community.Id));
        var member = Certify(scope, issuerId, memberId, community.Id);

        Command(scope).Mute(member.MembershipId);
        Db(scope).ChangeTracker.Clear();

        var svc = Membership(scope);
        var now = DateTime.UtcNow;

        var memberFacts = svc.FindWithAccountInCommunity(memberId, community.Id);
        memberFacts!.MembershipId.ShouldBe(member.MembershipId);
        memberFacts.IsActive.ShouldBeTrue();
        memberFacts.IsConfirmed.ShouldBeTrue();
        (memberFacts.MutedUntil is { } until && until > now).ShouldBeTrue();

        var issuerFacts = svc.FindWithAccountInCommunity(issuerId, community.Id);
        (issuerFacts!.MutedUntil is { } issuerUntil && issuerUntil > now).ShouldBeFalse();
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

        Command(scope).ElectManager(first.Id);
        db.ChangeTracker.Clear();
        audience.GetManagerAccountId(community.Id).ShouldBe(firstId);

        Command(scope).ElectManager(second.MembershipId);
        db.ChangeTracker.Clear();
        audience.GetManagerAccountId(community.Id).ShouldBe(secondId);
    }
}
