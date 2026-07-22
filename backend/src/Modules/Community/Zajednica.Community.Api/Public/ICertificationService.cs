using Zajednica.Community.Api.Dto.Certification;

namespace Zajednica.Community.Api.Public;

public interface ICertificationService
{
    Task<CertificationChallengeDto> CreateChallengeAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task CancelChallengeAsync(Guid accountId, Guid communityId, Guid challengeId, CancellationToken ct = default);
    Task<CertificationResultDto> ConfirmAsync(Guid accountId, ConfirmCertificationRequest request, CancellationToken ct = default);
}
