namespace Zajednica.Community.Api.Internal.Dto;

public record MembershipContextDto(
    Guid MembershipId,
    Guid AccountId,
    Guid CommunityId,
    bool IsConfirmed,
    bool IsActive,
    IReadOnlyList<string> Roles);
