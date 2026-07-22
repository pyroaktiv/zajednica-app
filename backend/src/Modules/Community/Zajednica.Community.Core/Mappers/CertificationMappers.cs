using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Core.Mappers;

public static class CertificationMappers
{
    public static CertificationChallengeDto ToDto(this CertificationChallenge challenge) =>
        new(challenge.Id, challenge.Token, challenge.ExpiresAt);
}
