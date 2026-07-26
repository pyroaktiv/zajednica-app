using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class CertificationServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly CertificationService _service = new();

    private static Membership Issuer(Guid communityId) => Membership.MakeCreator(Guid.NewGuid(), communityId, Now);
    private static Membership Candidate(Guid communityId) => new(Guid.NewGuid(), communityId, Now);

    [Fact]
    public void Certify_confirms_the_candidate_and_records_a_certificate()
    {
        var communityId = Guid.NewGuid();
        var issuer = Issuer(communityId);
        var candidate = Candidate(communityId);

        var certificate = _service.Certify(issuer, candidate, Now);

        candidate.CertificationStatus.ShouldBe(CertificationStatus.Confirmed);
        certificate.CommunityId.ShouldBe(communityId);
        certificate.IssuerMembershipId.ShouldBe(issuer.Id);
        certificate.CandidateMembershipId.ShouldBe(candidate.Id);
    }

    [Fact]
    public void Certify_requires_the_issuer_to_hold_the_issuer_role()
    {
        var communityId = Guid.NewGuid();
        var notAnIssuer = Candidate(communityId);
        var candidate = Candidate(communityId);

        Should.Throw<EntityValidationException>(() => _service.Certify(notAnIssuer, candidate, Now));
        candidate.CertificationStatus.ShouldBe(CertificationStatus.Unconfirmed);
    }

    [Fact]
    public void Certify_is_rejected_after_the_issuer_leaves_the_community()
    {
        var communityId = Guid.NewGuid();
        var issuer = Issuer(communityId);
        var candidate = Candidate(communityId);
        issuer.Leave(Now);

        Should.Throw<EntityValidationException>(() => _service.Certify(issuer, candidate, Now));
    }

    [Fact]
    public void Certify_rejects_a_candidate_from_another_community()
    {
        var issuer = Issuer(Guid.NewGuid());
        var candidate = Candidate(Guid.NewGuid());

        Should.Throw<EntityValidationException>(() => _service.Certify(issuer, candidate, Now));
    }
}
