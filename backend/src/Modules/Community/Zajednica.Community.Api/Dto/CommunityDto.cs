namespace Zajednica.Community.Api.Dto;

public record CommunityDto(
    Guid Id,
    string Name,
    AddressDto Address,
    string? RegistrationNumber,
    string? TaxId,
    string? BankAccountNumber,
    DateTime DateCreated);
