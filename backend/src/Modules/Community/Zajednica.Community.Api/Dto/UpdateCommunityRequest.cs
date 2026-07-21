namespace Zajednica.Community.Api.Dto;

public record UpdateCommunityRequest(
    string Name,
    AddressDto Address,
    string? RegistrationNumber,
    string? TaxId,
    string? BankAccountNumber);
