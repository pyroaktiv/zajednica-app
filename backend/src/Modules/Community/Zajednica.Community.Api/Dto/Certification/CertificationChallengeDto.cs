namespace Zajednica.Community.Api.Dto.Certification;

public record CertificationChallengeDto(Guid ChallengeId, string Token, DateTime ExpiresAt);
