using Zajednica.Community.Api.Dto.Certification;

namespace Zajednica.Community.Api.Public;

public interface ICertificationService
{
    CertificationChallengeDto CreateChallenge(Guid accountId, Guid communityId);
    void CancelChallenge(Guid accountId, Guid communityId, Guid challengeId);
    CertificationResultDto Confirm(Guid accountId, ConfirmCertificationRequestDto requestDto);
}
