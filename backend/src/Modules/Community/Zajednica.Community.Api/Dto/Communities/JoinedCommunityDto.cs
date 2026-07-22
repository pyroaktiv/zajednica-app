namespace Zajednica.Community.Api.Dto.Communities;

public record JoinedCommunityDto(
    Guid MembershipId,
    Guid CommunityId,
    string CommunityName,
    bool IsConfirmed);
