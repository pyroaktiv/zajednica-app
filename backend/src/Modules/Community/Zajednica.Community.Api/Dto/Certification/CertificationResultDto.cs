namespace Zajednica.Community.Api.Dto.Certification;

public record CertificationResultDto(
    Guid MembershipId,
    Guid CommunityId,
    DateTime CertifiedAt);
