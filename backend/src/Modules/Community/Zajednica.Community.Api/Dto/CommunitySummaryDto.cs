namespace Zajednica.Community.Api.Dto;

public record CommunitySummaryDto(
    Guid Id,
    string Name,
    AddressDto Address,
    bool IsConfirmed,
    IReadOnlyList<string> Roles);
