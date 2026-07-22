using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipService
{
    Task<MembershipContextDto?> GetContextAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipContextDto>> GetContextsAsync(IReadOnlyCollection<Guid> membershipIds, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipContextDto>> GetConfirmedAsync(Guid communityId, CancellationToken ct = default);
    Task<int> GetConfirmedCountAsync(Guid communityId, CancellationToken ct = default);
    Task<bool> AreEligibleAsync(IReadOnlyCollection<Guid> membershipIds, CancellationToken ct = default);

    Task BanAsync(Guid membershipId, Guid intentId, CancellationToken ct = default);
    Task ElectManagerAsync(Guid membershipId, CancellationToken ct = default);
    Task AddStarsAsync(Guid membershipId, int stars, CancellationToken ct = default);
}
