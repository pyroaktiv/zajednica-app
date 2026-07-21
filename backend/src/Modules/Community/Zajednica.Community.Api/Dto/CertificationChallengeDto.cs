namespace Zajednica.Community.Api.Dto;

public record CertificationChallengeDto(Guid ChallengeId, string Token, DateTime ExpiresAt);
