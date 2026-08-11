using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Dto.Memberships;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class RoleGrantTests : BaseCommunityIntegrationTest
{
    public RoleGrantTests(CommunityTestFactory factory) : base(factory) { }

    private static IInternalMembershipCommandService Outcome(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalMembershipCommandService>();

    [Fact]
    public void An_issuer_can_share_the_right_to_certify()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        var qrToken = QrToken(scope, issuerId, community.Id);
        Join(scope, memberId, qrToken);
        var member = Certify(scope, issuerId, memberId, community.Id);

        Members(scope, issuerId).GrantIssuer(community.Id, member.MembershipId);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.Memberships.Single(m => m.Id == member.MembershipId)
            .HasRole(CommunityRole.Issuer).ShouldBeTrue();
    }

    [Fact]
    public void A_member_without_the_issuer_role_cannot_share_it()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        var qrToken = QrToken(scope, issuerId, community.Id);
        Join(scope, memberId, qrToken);
        var member = Certify(scope, issuerId, memberId, community.Id);

        Should.Throw<ForbiddenException>(() =>
            Members(scope, memberId).GrantIssuer(community.Id, member.MembershipId));
    }

    [Fact]
    public void Electing_a_manager_moves_the_role_off_the_previous_one()
    {
        using var scope = Factory.Services.CreateScope();
        var firstId = NewAccount(scope);
        var secondId = NewAccount(scope);
        var community = CreateCommunity(scope, firstId);
        var qrToken = QrToken(scope, firstId, community.Id);
        Join(scope, secondId, qrToken);
        var second = Certify(scope, firstId, secondId, community.Id);

        var db = Db(scope);
        var first = db.Memberships.Single(m => m.AccountId == firstId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();

        Outcome(scope).ElectManager(first.Id);
        Outcome(scope).ElectManager(second.MembershipId);

        db.ChangeTracker.Clear();
        db.Memberships.Single(m => m.Id == first.Id).HasRole(CommunityRole.Manager).ShouldBeFalse();
        db.Memberships.Single(m => m.Id == second.MembershipId).HasRole(CommunityRole.Manager).ShouldBeTrue();
    }

    [Fact]
    public void The_elected_manager_may_change_the_community_details()
    {
        using var scope = Factory.Services.CreateScope();
        var accountId = NewAccount(scope);
        var community = CreateCommunity(scope, accountId);

        var db = Db(scope);
        var membership = db.Memberships.Single(m => m.AccountId == accountId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();
        Outcome(scope).ElectManager(membership.Id);

        var updated = Value<CommunityDetailsDto>((Communities(scope, accountId)
            .Update(community.Id, new UpdateCommunityRequestDto("Zgrada 2", community.Address, "12345678", "123456789", "160-1"))).Result!);

        updated.Name.ShouldBe("Zgrada 2");
        updated.RegistrationNumber.ShouldBe("12345678");
        updated.TaxId.ShouldBe("123456789");
    }

    [Fact]
    public void Member_lists_are_scoped_by_the_caller_role()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        var qrToken = QrToken(scope, issuerId, community.Id);
        Join(scope, newcomerId, qrToken);

        var unconfirmed = Value<IReadOnlyList<MemberSummaryDto>>(
            (Members(scope, issuerId).GetUnconfirmed(community.Id)).Result!);
        unconfirmed.Select(m => m.AccountId).ShouldContain(newcomerId);

        var issuers = Value<IReadOnlyList<MemberSummaryDto>>(
            (Members(scope, newcomerId).GetIssuers(community.Id)).Result!);
        issuers.Select(m => m.AccountId).ShouldBe(new[] { issuerId });

        Should.Throw<ForbiddenException>(() =>
            Members(scope, newcomerId).GetConfirmed(community.Id));
    }
}
