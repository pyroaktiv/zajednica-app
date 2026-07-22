using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Dto.Memberships;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class MembershipRoleTests : BaseCommunityIntegrationTest
{
    public MembershipRoleTests(CommunityTestFactory factory) : base(factory) { }

    private static IInternalMembershipService Internal(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IInternalMembershipService>();

    [Fact]
    public async Task An_issuer_can_share_the_right_to_certify()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, issuerId);
        var qrToken = await QrTokenAsync(scope, issuerId, community.Id);
        await JoinAsync(scope, memberId, qrToken);
        var member = await CertifyAsync(scope, issuerId, memberId, community.Id);

        await Members(scope, issuerId).GrantIssuer(community.Id, member.MembershipId, default);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.Memberships.Single(m => m.Id == member.MembershipId)
            .HasRole(CommunityRole.Issuer).ShouldBeTrue();
    }

    [Fact]
    public async Task A_member_without_the_issuer_role_cannot_share_it()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, issuerId);
        var qrToken = await QrTokenAsync(scope, issuerId, community.Id);
        await JoinAsync(scope, memberId, qrToken);
        var member = await CertifyAsync(scope, issuerId, memberId, community.Id);

        await Should.ThrowAsync<ForbiddenException>(() =>
            Members(scope, memberId).GrantIssuer(community.Id, member.MembershipId, default));
    }

    [Fact]
    public async Task Electing_a_manager_moves_the_role_off_the_previous_one()
    {
        using var scope = Factory.Services.CreateScope();
        var firstId = NewAccount(scope);
        var secondId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, firstId);
        var qrToken = await QrTokenAsync(scope, firstId, community.Id);
        await JoinAsync(scope, secondId, qrToken);
        var second = await CertifyAsync(scope, firstId, secondId, community.Id);

        var db = Db(scope);
        var first = db.Memberships.Single(m => m.AccountId == firstId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();

        await Internal(scope).ElectManagerAsync(first.Id);
        await Internal(scope).ElectManagerAsync(second.MembershipId);

        db.ChangeTracker.Clear();
        db.Memberships.Single(m => m.Id == first.Id).HasRole(CommunityRole.Manager).ShouldBeFalse();
        db.Memberships.Single(m => m.Id == second.MembershipId).HasRole(CommunityRole.Manager).ShouldBeTrue();
    }

    [Fact]
    public async Task The_elected_manager_may_change_the_community_details()
    {
        using var scope = Factory.Services.CreateScope();
        var accountId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, accountId);

        var db = Db(scope);
        var membership = db.Memberships.Single(m => m.AccountId == accountId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();
        await Internal(scope).ElectManagerAsync(membership.Id);

        var updated = Value<CommunityDetailsDto>((await Communities(scope, accountId)
            .Update(community.Id, new UpdateCommunityRequest("Zgrada 2", community.Address, "12345678", "123456789", "160-1"), default)).Result!);

        updated.Name.ShouldBe("Zgrada 2");
        updated.RegistrationNumber.ShouldBe("12345678");
        updated.TaxId.ShouldBe("123456789");
    }

    [Fact]
    public async Task Member_lists_are_scoped_by_the_caller_role()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, issuerId);
        var qrToken = await QrTokenAsync(scope, issuerId, community.Id);
        await JoinAsync(scope, newcomerId, qrToken);

        var unconfirmed = Value<IReadOnlyList<MemberSummaryDto>>(
            (await Members(scope, issuerId).GetUnconfirmed(community.Id, default)).Result!);
        unconfirmed.Select(m => m.AccountId).ShouldContain(newcomerId);

        var issuers = Value<IReadOnlyList<MemberSummaryDto>>(
            (await Members(scope, newcomerId).GetIssuers(community.Id, default)).Result!);
        issuers.Select(m => m.AccountId).ShouldBe(new[] { issuerId });

        await Should.ThrowAsync<ForbiddenException>(() =>
            Members(scope, newcomerId).GetConfirmed(community.Id, default));
    }
}
