using Zajednica.Community.Api.Dto;
using Zajednica.Community.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Community.Core.Mappers;

public static class CertificationMappers
{
    public static CertificationChallengeDto ToDto(this CertificationChallenge challenge) =>
        new(challenge.Id, challenge.Token, challenge.ExpiresAt);

    public static TrustGraphDto ToTrustGraph(
        IEnumerable<Membership> confirmedMembers,
        IEnumerable<Certificate> certificates,
        IReadOnlyDictionary<Guid, AccountProfileDto> profiles)
    {
        var vertices = confirmedMembers
            .Select(m => new TrustVertexDto(
                m.Id,
                profiles.GetValueOrDefault(m.AccountId)?.Username ?? string.Empty,
                profiles.GetValueOrDefault(m.AccountId)?.ImageUrl,
                m.Stars))
            .ToList();

        var known = vertices.Select(v => v.MembershipId).ToHashSet();
        var edges = certificates
            .Where(c => known.Contains(c.IssuerMembershipId) && known.Contains(c.CandidateMembershipId))
            .Select(c => new TrustEdgeDto(c.IssuerMembershipId, c.CandidateMembershipId, c.Date))
            .ToList();

        return new TrustGraphDto(vertices, edges);
    }
}
