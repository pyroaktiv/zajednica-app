using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Community : AggregateRoot
{
    public Guid CreatorAccountId { get; private set; }
    public Guid? ManagerMembershipId { get; private set; }
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public RegistrationNumber? MB { get; private set; }
    public TaxId? PIB { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string QrToken { get; private set; } = null!;
    public DateTime DateCreated { get; private set; }

    // EF
    private Community() { }

    public Community(Guid creatorAccountId, string name, Address address, string qrToken, DateTime now,
        RegistrationNumber? mb = null, TaxId? pib = null, string? bankAccountNumber = null)
    {
        if (creatorAccountId == Guid.Empty)
            throw new EntityValidationException("CreatorAccountId is required.");

        CreatorAccountId = creatorAccountId;
        Name = RequireName(name);
        Address = address ?? throw new EntityValidationException("Address is required.");
        QrToken = RequireToken(qrToken);
        MB = mb;
        PIB = pib;
        BankAccountNumber = Clean(bankAccountNumber);
        DateCreated = now;
    }

    // Profile-completeness of the candidate manager is checked in the UseCase via Identity.Api.Internal
    // (cross-module fact), then this only records the structural change.
    public void SetManager(Guid membershipId)
    {
        if (membershipId == Guid.Empty)
            throw new EntityValidationException("ManagerMembershipId is required.");
        ManagerMembershipId = membershipId;
    }

    public void RemoveManager() => ManagerMembershipId = null;

    public bool HasManager() => ManagerMembershipId.HasValue;

    private static string RequireName(string name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new EntityValidationException("Community name is required.");
        return trimmed;
    }

    private static string RequireToken(string qrToken)
    {
        if (string.IsNullOrWhiteSpace(qrToken))
            throw new EntityValidationException("QR token is required.");
        return qrToken;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
