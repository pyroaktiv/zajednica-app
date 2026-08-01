using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class VoterContext : ValueObject
{
    public Guid VoterMembershipId { get; }
    public DateTime CertificationDate { get; }

    public VoterContext(Guid voterMembershipId, DateTime certificationDate)
    {
        if (voterMembershipId == Guid.Empty)
            throw new EntityValidationException("A voter must be identified by their membership.");

        VoterMembershipId = voterMembershipId;
        CertificationDate = certificationDate;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return VoterMembershipId;
        yield return CertificationDate;
    }
}
