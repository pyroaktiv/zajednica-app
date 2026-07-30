using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Certificate : Entity
{
    public Guid IssuerMembershipId { get; private set; }
    public DateTime Date { get; private set; }

    private Certificate() { }

    internal Certificate(Guid issuerMembershipId, DateTime date)
    {
        if (issuerMembershipId == Guid.Empty)
            throw new EntityValidationException("IssuerMembershipId is required.");

        IssuerMembershipId = issuerMembershipId;
        Date = date;
    }
}
