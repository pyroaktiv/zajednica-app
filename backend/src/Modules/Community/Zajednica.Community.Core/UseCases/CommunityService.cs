using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;
using CommunityAggregate = Zajednica.Community.Core.Domain.Community;

namespace Zajednica.Community.Core.UseCases;

public sealed class CommunityService(
    ICommunityRepository communities,
    IMembershipRepository memberships,
    IBlacklistRepository blacklist,
    ISecureTokenGenerator tokens,
    IRealtimePusher realtime,
    MembershipAccess access) : ICommunityService
{
    public CommunityDetailsDto Create(Guid accountId, CreateCommunityRequest request)
    {
        var now = DateTime.UtcNow;
        var community = new CommunityAggregate(
            request.Name,
            request.Address.ToAddress(),
            tokens.Generate(),
            now,
            CommunityMappers.ToRegistrationNumber(request.RegistrationNumber),
            CommunityMappers.ToTaxId(request.TaxId),
            request.BankAccountNumber);

        communities.Add(community);
        memberships.Add(Membership.MakeCreator(accountId, community.Id, now));

        return community.ToDetailsDto();
    }

    public IReadOnlyList<MyCommunityDto> GetMine(Guid accountId)
    {
        var mine = (memberships.GetByAccount(accountId))
            .Where(m => m.IsActive())
            .ToList();
        if (mine.Count == 0)
            return [];

        var found = (communities.GetManyByIds(mine.Select(m => m.CommunityId).ToList()))
            .ToDictionary(c => c.Id);

        return mine
            .Where(m => found.ContainsKey(m.CommunityId))
            .Select(m => found[m.CommunityId].ToMyCommunityDto(m))
            .ToList();
    }

    public CommunityDetailsDto Get(Guid accountId, Guid communityId)
    {
        var (community, _) = access.RequireMember(accountId, communityId);
        return community.ToDetailsDto();
    }

    public CommunityDetailsDto Update(Guid accountId, Guid communityId, UpdateCommunityRequest request)
    {
        var (community, actor) = access.RequireRole(accountId, communityId, CommunityRole.Manager);

        community.UpdateDetails(
            actor,
            request.Name,
            request.Address.ToAddress(),
            CommunityMappers.ToRegistrationNumber(request.RegistrationNumber),
            CommunityMappers.ToTaxId(request.TaxId),
            request.BankAccountNumber);

        communities.Update(community);

        return community.ToDetailsDto();
    }

    public CommunityQrDto GetQr(Guid accountId, Guid communityId)
    {
        var (community, _) = access.RequireConfirmed(accountId, communityId);
        return community.ToQrDto();
    }

    public JoinedCommunityDto Join(Guid accountId, JoinCommunityRequest request)
    {
        var community = communities.GetByQrToken(request.QrToken)
            ?? throw new NotFoundException("No community matches this QR code.");

        if (blacklist.Exists(accountId, community.Id))
            throw new ForbiddenException("This account is banned from the community.");

        var existing = memberships.Get(accountId, community.Id);
        if (existing is not null && existing.IsActive())
            throw new EntityValidationException("Already a member of this community.");

        var membership = existing ?? new Membership(accountId, community.Id, DateTime.UtcNow);

        if (existing is null)
            memberships.Add(membership);
        else
        {
            membership.Rejoin();
            memberships.Update(membership);
        }

        return membership.ToJoinedDto(community.Name);
    }

    public void Leave(Guid accountId, Guid communityId)
    {
        var (_, membership) = access.RequireMember(accountId, communityId);

        membership.Leave(DateTime.UtcNow);
        memberships.Update(membership);

        realtime.PushToUser(accountId,
            new RealtimeMessage("membership.roles.changed", new { communityId }));
    }
}
