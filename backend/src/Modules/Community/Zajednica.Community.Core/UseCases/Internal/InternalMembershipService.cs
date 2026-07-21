using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class InternalMembershipService(
    IMembershipRepository memberships,
    IBlacklistRepository blacklist,
    IRealtimePusher realtime,
    ManagerElectionService election,
    MembershipBanService ban) : IInternalMembershipService
{
    public async Task<MembershipContextDto?> GetContextAsync(Guid accountId, Guid communityId, CancellationToken ct = default) =>
        (await memberships.GetAsync(accountId, communityId, ct))?.ToContextDto(DateTime.UtcNow);

    public async Task<IReadOnlyList<MembershipContextDto>> GetContextsAsync(IReadOnlyCollection<Guid> membershipIds, CancellationToken ct = default)
    {
        if (membershipIds.Count == 0)
            return [];

        var now = DateTime.UtcNow;
        var found = await memberships.GetManyByIdsAsync(membershipIds, ct);
        return found.Select(m => m.ToContextDto(now)).ToList();
    }

    public Task<int> GetConfirmedCountAsync(Guid communityId, CancellationToken ct = default) =>
        memberships.CountConfirmedAsync(communityId, ct);

    public async Task<bool> AreEligibleAsync(IReadOnlyCollection<Guid> membershipIds, CancellationToken ct = default)
    {
        if (membershipIds.Count == 0)
            return false;

        var found = await memberships.GetManyByIdsAsync(membershipIds, ct);
        return found.Count == membershipIds.Distinct().Count()
               && found.All(m => m.IsActive() && m.IsConfirmed());
    }

    public async Task MuteAsync(Guid membershipId, int days, CancellationToken ct = default)
    {
        var membership = await Require(membershipId, ct);

        membership.Mute(DateTime.UtcNow.AddDays(days));
        await memberships.UpdateAsync(membership, ct);

        await PushRolesChanged(membership, ct);
    }

    public async Task BanAsync(Guid membershipId, Guid intentId, CancellationToken ct = default)
    {
        var membership = await Require(membershipId, ct);

        var entry = ban.Ban(membership, intentId, DateTime.UtcNow);
        await memberships.UpdateAsync(membership, ct);
        await blacklist.AddAsync(entry, ct);

        await PushRolesChanged(membership, ct);
    }

    public async Task ElectManagerAsync(Guid membershipId, CancellationToken ct = default)
    {
        var newManager = await Require(membershipId, ct);

        var currentManager = (await memberships.GetByCommunityAsync(newManager.CommunityId, ct))
            .SingleOrDefault(m => m.HasRole(CommunityRole.Manager));

        election.Elect(currentManager, newManager, DateTime.UtcNow);

        await memberships.UpdateAsync(newManager, ct);
        if (currentManager is not null)
        {
            await memberships.UpdateAsync(currentManager, ct);
            await PushRolesChanged(currentManager, ct);
        }
        await PushRolesChanged(newManager, ct);
    }

    public async Task AddStarsAsync(Guid membershipId, int stars, CancellationToken ct = default)
    {
        var membership = await Require(membershipId, ct);

        membership.AddStars(stars);
        await memberships.UpdateAsync(membership, ct);
    }

    private async Task<Membership> Require(Guid membershipId, CancellationToken ct) =>
        await memberships.GetByIdAsync(membershipId, ct) ?? throw new NotFoundException("Membership not found.");

    private Task PushRolesChanged(Membership membership, CancellationToken ct) =>
        realtime.PushToUserAsync(membership.AccountId,
            new RealtimeMessage("membership.roles.changed", new { communityId = membership.CommunityId }), ct);
}
