using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class CertificationTests : BaseCommunityIntegrationTest
{
    public CertificationTests(CommunityTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Confirming_a_challenge_certifies_the_candidate_and_records_the_certificate()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var candidateId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, issuerId);
        var qrToken = await QrTokenAsync(scope, issuerId, community.Id);
        await JoinAsync(scope, candidateId, qrToken);

        var confirmed = await CertifyAsync(scope, issuerId, candidateId, community.Id);

        confirmed.Member.IsConfirmed.ShouldBeTrue();

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var issuerMembership = db.Memberships.Single(m => m.AccountId == issuerId && m.CommunityId == community.Id);
        var certificate = db.Certificates.Single(c => c.CandidateMembershipId == confirmed.Member.MembershipId);
        certificate.IssuerMembershipId.ShouldBe(issuerMembership.Id);
        certificate.CommunityId.ShouldBe(community.Id);

        db.CertificationChallenges.Count(c => c.CommunityId == community.Id).ShouldBe(0);
    }

    [Fact]
    public async Task An_expired_challenge_is_rejected_and_discarded()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var candidateId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, issuerId);
        var qrToken = await QrTokenAsync(scope, issuerId, community.Id);
        await JoinAsync(scope, candidateId, qrToken);

        var db = Db(scope);
        var issuerMembership = db.Memberships.Single(m => m.AccountId == issuerId && m.CommunityId == community.Id);
        var expired = new CertificationChallenge(
            community.Id, issuerMembership.Id, $"expired-{Guid.NewGuid():N}", DateTime.UtcNow.AddMinutes(-1));
        db.CertificationChallenges.Add(expired);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Certification(scope, candidateId).Confirm(new ConfirmCertificationRequest(expired.Token), default));

        db.ChangeTracker.Clear();
        db.CertificationChallenges.Count(c => c.Token == expired.Token).ShouldBe(0);
    }

    [Fact]
    public async Task A_plain_confirmed_member_cannot_open_a_certification_challenge()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, issuerId);
        var qrToken = await QrTokenAsync(scope, issuerId, community.Id);
        await JoinAsync(scope, memberId, qrToken);
        await CertifyAsync(scope, issuerId, memberId, community.Id);

        await Should.ThrowAsync<ForbiddenException>(() =>
            Certification(scope, memberId).CreateChallenge(community.Id, default));
    }

    [Fact]
    public async Task The_trust_graph_exposes_who_certified_whom()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var candidateId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, issuerId);
        var qrToken = await QrTokenAsync(scope, issuerId, community.Id);
        await JoinAsync(scope, candidateId, qrToken);
        var candidate = await CertifyAsync(scope, issuerId, candidateId, community.Id);

        var graph = Value<TrustGraphDto>(
            (await Certification(scope, issuerId).GetTrustGraph(community.Id, default)).Result!);

        graph.Vertices.Count.ShouldBe(2);
        graph.Vertices.ShouldAllBe(v => v.Username != "");
        var edge = graph.Edges.Single(e => e.CandidateMembershipId == candidate.Member.MembershipId);
        graph.Vertices.ShouldContain(v => v.MembershipId == edge.IssuerMembershipId);
    }
}
