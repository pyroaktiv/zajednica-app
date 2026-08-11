namespace Zajednica.Community.Api.Dto.Communities;

public record MyCommunityDto(
    Guid Id,
    string Name,
    AddressDto Address,
    bool IsConfirmed,
    IReadOnlyList<string> Roles);
