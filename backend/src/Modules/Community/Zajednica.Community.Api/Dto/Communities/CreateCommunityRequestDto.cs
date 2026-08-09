namespace Zajednica.Community.Api.Dto.Communities;

public record CreateCommunityRequestDto(
    string Name,
    AddressDto Address,
    string? RegistrationNumber,
    string? TaxId,
    string? BankAccountNumber);
