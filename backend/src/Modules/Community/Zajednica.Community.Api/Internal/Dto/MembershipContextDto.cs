namespace Zajednica.Community.Api.Internal.Dto;

public record MembershipContextDto(
    Guid MembershipId,
    Guid AccountId,
    Guid CommunityId,
    bool IsConfirmed,
    bool IsActive,
    bool IsMuted,
    IReadOnlyList<string> Roles);
