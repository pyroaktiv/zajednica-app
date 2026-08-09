namespace Zajednica.Community.Api.Internal.Dto;

public record InternalMembershipFactsDto(
    Guid MembershipId,
    Guid AccountId,
    Guid CommunityId,
    bool IsActive,
    bool IsConfirmed,
    bool IsBanned,
    DateTime? MutedUntil,
    bool CanIssueCertifications,
    bool IsManager,
    DateTime CertifiedAt);
