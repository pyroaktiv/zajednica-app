namespace Zajednica.Community.Api.Dto.Communities;

public record UpdateCommunityRequestDto(
    string Name,
    AddressDto Address,
    string? RegistrationNumber,
    string? TaxId,
    string? BankAccountNumber);
