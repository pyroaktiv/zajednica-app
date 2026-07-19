using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class CertificationService
{
    public Certificate Certify(Membership issuer, Membership candidate, DateTime now)
    {
        if (!issuer.CertificateIssuer)
            throw new EntityValidationException("Issuer does not have the right to certify members.");
        if (!issuer.IsActive)
            throw new EntityValidationException("An inactive member cannot certify.");
        if (issuer.CommunityId != candidate.CommunityId)
            throw new EntityValidationException("Issuer and candidate belong to different communities.");

        var certificate = new Certificate(candidate.CommunityId, issuer.Id, candidate.Id, now);
        candidate.Confirm();
        return certificate;
    }
}
