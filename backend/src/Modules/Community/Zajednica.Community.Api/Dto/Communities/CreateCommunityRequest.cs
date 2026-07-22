namespace Zajednica.Community.Api.Dto.Communities;

public record CreateCommunityRequest(
    string Name,
    AddressDto Address,
    string? RegistrationNumber,
    string? TaxId,
    string? BankAccountNumber);
