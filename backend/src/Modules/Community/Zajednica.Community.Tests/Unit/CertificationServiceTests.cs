using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.IntegrationEvents;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class CertificationServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly CertificationService _service = new();

    private static Membership Issuer(Guid communityId) => Membership.Founder(Guid.NewGuid(), communityId, Now);
    private static Membership Candidate(Guid communityId) => new(Guid.NewGuid(), communityId, Now);

    [Fact]
    public void Certify_confirms_the_candidate_and_records_a_certificate()
    {
        var communityId = Guid.NewGuid();
        var issuer = Issuer(communityId);
        var candidate = Candidate(communityId);

        var certificate = _service.Certify(issuer, candidate, Now);

        candidate.Status.ShouldBe(MembershipStatus.Confirmed);
        candidate.DomainEvents.ShouldContain(e => e is MembershipConfirmed);
        certificate.CommunityId.ShouldBe(communityId);
        certificate.IssuerMembershipId.ShouldBe(issuer.Id);
        certificate.CandidateMembershipId.ShouldBe(candidate.Id);
    }

    [Fact]
    public void Certify_requires_the_issuer_to_hold_the_issuer_right()
    {
        var communityId = Guid.NewGuid();
        var notAnIssuer = Candidate(communityId); // unconfirmed, no right
        var candidate = Candidate(communityId);

        Should.Throw<EntityValidationException>(() => _service.Certify(notAnIssuer, candidate, Now));
        candidate.Status.ShouldBe(MembershipStatus.Unconfirmed); // left untouched
    }

    [Fact]
    public void Certify_rejects_a_candidate_from_another_community()
    {
        var issuer = Issuer(Guid.NewGuid());
        var candidate = Candidate(Guid.NewGuid()); // different community

        Should.Throw<EntityValidationException>(() => _service.Certify(issuer, candidate, Now));
    }
}
