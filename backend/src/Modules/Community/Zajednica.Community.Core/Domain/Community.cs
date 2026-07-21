using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Community : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public RegistrationNumber? RegistrationNumber { get; private set; }
    public TaxId? TaxId { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string QrToken { get; private set; } = null!;
    public DateTime DateCreated { get; private set; }
    
    private Community() { }

    public Community(string name, Address address, string qrToken, DateTime now,
        RegistrationNumber? registrationNumber = null, TaxId? taxId = null, string? bankAccountNumber = null)
    {
        Name = RequireName(name);
        Address = address ?? throw new EntityValidationException("Address is required.");
        QrToken = RequireToken(qrToken);
        RegistrationNumber = registrationNumber;
        TaxId = taxId;
        BankAccountNumber = Clean(bankAccountNumber);
        DateCreated = now;
    }
    
    public void UpdateDetails(Membership actor, string name, Address address,
        RegistrationNumber? registrationNumber, TaxId? taxId, string? bankAccountNumber)
    {
        RequireManager(actor);

        Name = RequireName(name);
        Address = address ?? throw new EntityValidationException("Address is required.");
        RegistrationNumber = registrationNumber;
        TaxId = taxId;
        BankAccountNumber = Clean(bankAccountNumber);
    }

    private void RequireManager(Membership actor)
    {
        if (actor.CommunityId != Id)
            throw new ForbiddenException("Membership belongs to another community.");
        if (!actor.IsActive() || !actor.HasRole(CommunityRole.Manager))
            throw new ForbiddenException("Only the community manager can change community details.");
    }

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
