namespace Zajednica.Community.Api.Dto.Communities;

public record CommunityDetailsDto(
    Guid Id,
    string Name,
    AddressDto Address,
    string? RegistrationNumber,
    string? TaxId,
    string? BankAccountNumber,
    DateTime DateCreated);
