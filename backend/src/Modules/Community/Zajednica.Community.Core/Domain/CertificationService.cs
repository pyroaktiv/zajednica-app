using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class CertificationService
{
    public void Certify(Membership issuer, Membership candidate, DateTime now)
    {
        if (!issuer.CanIssueCertifications())
            throw new EntityValidationException("Issuer does not have the right to certify members.");
        if (issuer.CommunityId != candidate.CommunityId)
            throw new EntityValidationException("Issuer and candidate belong to different communities.");

        candidate.CertifyBy(issuer.Id, now);
    }
}
