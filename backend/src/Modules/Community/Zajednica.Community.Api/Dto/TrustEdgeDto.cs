namespace Zajednica.Community.Api.Dto;

public record TrustEdgeDto(Guid IssuerMembershipId, Guid CandidateMembershipId, DateTime Date);
