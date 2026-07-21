using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Community.Core.UseCases;

public sealed class MembershipService(
    IMembershipRepository memberships,
    IInternalAccountService accounts,
    IRealtimePusher realtime,
    MembershipAccess access) : IMembershipService
{
    public async Task<MembershipDto> GetMineAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        var (_, membership) = await access.RequireMemberAsync(accountId, communityId, ct);
        return membership.ToDto(await accounts.GetProfileAsync(accountId, ct));
    }

    public async Task<MembershipDto> SetUnitNumberAsync(Guid accountId, Guid communityId, SetUnitNumberRequest request, CancellationToken ct = default)
    {
        var (_, membership) = await access.RequireMemberAsync(accountId, communityId, ct);

        membership.SetUnitNumber(request.UnitNumber);
        await memberships.UpdateAsync(membership, ct);

        return membership.ToDto(await accounts.GetProfileAsync(accountId, ct));
    }

    public async Task<MembershipDto> GetAsync(Guid accountId, Guid communityId, Guid membershipId, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);

        var target = await memberships.GetByIdAsync(membershipId, ct);
        if (target is null || target.CommunityId != communityId)
            throw new NotFoundException("Membership not found in this community.");

        return target.ToDto(await accounts.GetProfileAsync(target.AccountId, ct));
    }

    public async Task<IReadOnlyList<CommunityMemberDto>> GetConfirmedAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);
        return await CardsAsync(communityId, m => m.IsActive() && m.IsConfirmed(), ct);
    }

    public async Task<IReadOnlyList<CommunityMemberDto>> GetIssuersAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        await access.RequireMemberAsync(accountId, communityId, ct);
        return await CardsAsync(communityId, m => m.IsActive() && m.Roles.Count > 0, ct);
    }

    public async Task<IReadOnlyList<CommunityMemberDto>> GetUnconfirmedAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        await access.RequireAnyRoleAsync(accountId, communityId, ct, CommunityRole.Issuer, CommunityRole.Manager);
        return await CardsAsync(communityId, m => m.IsActive() && !m.IsConfirmed(), ct);
    }

    public async Task<CommunityMemberDto?> GetManagerAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);
        var cards = await CardsAsync(communityId, m => m.IsActive() && m.HasRole(CommunityRole.Manager), ct);
        return cards.SingleOrDefault();
    }

    public async Task<IReadOnlyList<CommunityMemberDto>> GetRankingAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);

        var roster = (await memberships.GetByCommunityAsync(communityId, ct))
            .Where(m => m.IsActive() && m.IsConfirmed() && m.Stars > 0)
            .OrderByDescending(m => m.Stars)
            .ToList();

        return roster.ToMemberDtos(await ProfilesAsync(roster, ct));
    }

    public async Task GrantIssuerAsync(Guid accountId, Guid communityId, Guid membershipId, CancellationToken ct = default)
    {
        var (_, actor) = await access.RequireRoleAsync(accountId, communityId, CommunityRole.Issuer, ct);

        var target = await memberships.GetByIdAsync(membershipId, ct);
        if (target is null || target.CommunityId != communityId)
            throw new NotFoundException("Membership not found in this community.");

        target.Grant(CommunityRole.Issuer, actor.Id, DateTime.UtcNow);
        await memberships.UpdateAsync(target, ct);

        await realtime.PushToUserAsync(target.AccountId,
            new RealtimeMessage("membership.roles.changed", new { communityId }), ct);
    }

    private async Task<IReadOnlyList<CommunityMemberDto>> CardsAsync(
        Guid communityId, Func<Membership, bool> predicate, CancellationToken ct)
    {
        var roster = (await memberships.GetByCommunityAsync(communityId, ct)).Where(predicate).ToList();
        return roster.ToMemberDtos(await ProfilesAsync(roster, ct));
    }

    private async Task<IReadOnlyDictionary<Guid, AccountProfileDto>> ProfilesAsync(
        IReadOnlyCollection<Membership> roster, CancellationToken ct)
    {
        if (roster.Count == 0)
            return new Dictionary<Guid, AccountProfileDto>();

        var profiles = await accounts.GetProfilesAsync(roster.Select(m => m.AccountId).Distinct().ToList(), ct);
        return profiles.ToDictionary(p => p.AccountId);
    }
}
