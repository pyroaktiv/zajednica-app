using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Dto.Memberships;
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
    MembershipNotifier notifier,
    MembershipAccess access) : IMembershipService
{
    public MemberProfileDto GetMine(Guid accountId, Guid communityId)
    {
        var (_, membership) = access.RequireMember(accountId, communityId);
        return membership.ToProfileDto(accounts.GetProfile(accountId), DateTime.UtcNow);
    }

    public UnitNumberDto SetUnitNumber(Guid accountId, Guid communityId, SetUnitNumberRequest request)
    {
        var (_, membership) = access.RequireMember(accountId, communityId);

        membership.SetUnitNumber(request.UnitNumber);
        memberships.Update(membership);

        return membership.ToUnitNumberDto();
    }

    public MemberProfileDto Get(Guid accountId, Guid communityId, Guid membershipId)
    {
        access.RequireConfirmed(accountId, communityId);

        var target = memberships.GetById(membershipId);
        if (target is null || target.CommunityId != communityId)
            throw new NotFoundException("Membership not found in this community.");

        return target.ToProfileDto(accounts.GetProfile(target.AccountId), DateTime.UtcNow);
    }

    public IReadOnlyList<MemberSummaryDto> GetConfirmed(Guid accountId, Guid communityId)
    {
        access.RequireConfirmed(accountId, communityId);
        return Cards(communityId, m => m.IsActive() && m.IsConfirmed());
    }

    public IReadOnlyList<MemberSummaryDto> GetIssuers(Guid accountId, Guid communityId)
    {
        access.RequireMember(accountId, communityId);
        return Cards(communityId, m => m.IsActive() && m.HasRole(CommunityRole.Issuer));
    }

    public IReadOnlyList<MemberSummaryDto> GetUnconfirmed(Guid accountId, Guid communityId)
    {
        access.RequireAnyRole(accountId, communityId, CommunityRole.Issuer, CommunityRole.Manager);
        return Cards(communityId, m => m.IsActive() && !m.IsConfirmed());
    }

    public MemberSummaryDto? GetManager(Guid accountId, Guid communityId)
    {
        access.RequireConfirmed(accountId, communityId);
        var cards = Cards(communityId, m => m.IsActive() && m.HasRole(CommunityRole.Manager));
        return cards.SingleOrDefault();
    }

    public IReadOnlyList<MemberSummaryDto> GetRanking(Guid accountId, Guid communityId)
    {
        access.RequireConfirmed(accountId, communityId);

        var roster = (memberships.GetByCommunity(communityId))
            .Where(m => m.IsActive() && m.IsConfirmed() && m.Stars > 0)
            .OrderByDescending(m => m.Stars)
            .ToList();

        return roster.ToSummaryDtos(Profiles(roster));
    }

    public void GrantIssuer(Guid accountId, Guid communityId, Guid membershipId)
    {
        var (_, actor) = access.RequireRole(accountId, communityId, CommunityRole.Issuer);

        var target = memberships.GetById(membershipId);
        if (target is null || target.CommunityId != communityId)
            throw new NotFoundException("Membership not found in this community.");

        target.Grant(CommunityRole.Issuer, actor.Id, DateTime.UtcNow);
        memberships.Update(target);

        notifier.RolesChanged(target);
    }

    private IReadOnlyList<MemberSummaryDto> Cards(
        Guid communityId, Func<Membership, bool> predicate)
    {
        var roster = (memberships.GetByCommunity(communityId)).Where(predicate).ToList();
        return roster.ToSummaryDtos(Profiles(roster));
    }

    private IReadOnlyDictionary<Guid, AccountProfileDto> Profiles(
        IReadOnlyCollection<Membership> roster)
    {
        if (roster.Count == 0)
            return new Dictionary<Guid, AccountProfileDto>();

        var profiles = accounts.GetProfiles(roster.Select(m => m.AccountId).Distinct().ToList());
        return profiles.ToDictionary(p => p.AccountId);
    }
}
