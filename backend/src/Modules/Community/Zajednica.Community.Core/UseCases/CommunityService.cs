using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;
using Zajednica.Identity.Api.Internal;
using CommunityAggregate = Zajednica.Community.Core.Domain.Community;

namespace Zajednica.Community.Core.UseCases;

public sealed class CommunityService(
    ICommunityRepository communities,
    IMembershipRepository memberships,
    IBlacklistRepository blacklist,
    IInternalAccountService accounts,
    ISecureTokenGenerator tokens,
    IRealtimePusher realtime,
    MembershipAccess access) : ICommunityService
{
    public async Task<CommunityDto> CreateAsync(Guid accountId, CreateCommunityRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var community = new CommunityAggregate(
            request.Name,
            request.Address.ToDomain(),
            tokens.Generate(),
            now,
            CommunityMappers.ToRegistrationNumber(request.RegistrationNumber),
            CommunityMappers.ToTaxId(request.TaxId),
            request.BankAccountNumber);

        await communities.AddAsync(community, ct);
        await memberships.AddAsync(Membership.Creator(accountId, community.Id, now), ct);

        return community.ToDto();
    }

    public async Task<IReadOnlyList<CommunitySummaryDto>> GetMineAsync(Guid accountId, CancellationToken ct = default)
    {
        var mine = (await memberships.GetByAccountAsync(accountId, ct))
            .Where(m => m.IsActive())
            .ToList();
        if (mine.Count == 0)
            return [];

        var found = (await communities.GetManyByIdsAsync(mine.Select(m => m.CommunityId).ToList(), ct))
            .ToDictionary(c => c.Id);

        return mine
            .Where(m => found.ContainsKey(m.CommunityId))
            .Select(m => found[m.CommunityId].ToSummaryDto(m))
            .ToList();
    }

    public async Task<CommunityDto> GetAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        var (community, _) = await access.RequireMemberAsync(accountId, communityId, ct);
        return community.ToDto();
    }

    public async Task<CommunityDto> UpdateAsync(Guid accountId, Guid communityId, UpdateCommunityRequest request, CancellationToken ct = default)
    {
        var (community, actor) = await access.RequireRoleAsync(accountId, communityId, CommunityRole.Manager, ct);

        community.UpdateDetails(
            actor,
            request.Name,
            request.Address.ToDomain(),
            CommunityMappers.ToRegistrationNumber(request.RegistrationNumber),
            CommunityMappers.ToTaxId(request.TaxId),
            request.BankAccountNumber);

        await communities.UpdateAsync(community, ct);
        await realtime.PushToChannelAsync(Channels.Community(communityId),
            new RealtimeMessage("community.updated", new { communityId }), ct);

        return community.ToDto();
    }

    public async Task<CommunityQrDto> GetQrAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        var (community, _) = await access.RequireConfirmedAsync(accountId, communityId, ct);
        return community.ToQrDto();
    }

    public async Task<MembershipDto> JoinAsync(Guid accountId, JoinCommunityRequest request, CancellationToken ct = default)
    {
        var community = await communities.GetByQrTokenAsync(request.QrToken, ct)
            ?? throw new NotFoundException("No community matches this QR code.");

        if (await blacklist.ExistsAsync(accountId, community.Id, ct))
            throw new ForbiddenException("This account is banned from the community.");

        var existing = await memberships.GetAsync(accountId, community.Id, ct);
        if (existing is not null && existing.IsActive())
            throw new EntityValidationException("Already a member of this community.");

        var membership = existing ?? new Membership(accountId, community.Id, DateTime.UtcNow);

        if (existing is null)
            await memberships.AddAsync(membership, ct);
        else
        {
            membership.Rejoin();
            await memberships.UpdateAsync(membership, ct);
        }

        return membership.ToDto(await accounts.GetProfileAsync(accountId, ct));
    }

    public async Task LeaveAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        var (_, membership) = await access.RequireMemberAsync(accountId, communityId, ct);

        membership.Leave(DateTime.UtcNow);
        await memberships.UpdateAsync(membership, ct);

        await realtime.PushToUserAsync(accountId,
            new RealtimeMessage("membership.roles.changed", new { communityId }), ct);
    }
}
