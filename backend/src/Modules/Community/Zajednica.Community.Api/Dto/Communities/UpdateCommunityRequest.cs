namespace Zajednica.Community.Api.Dto.Communities;

public record UpdateCommunityRequest(
    string Name,
    AddressDto Address,
    string? RegistrationNumber,
    string? TaxId,
    string? BankAccountNumber);
