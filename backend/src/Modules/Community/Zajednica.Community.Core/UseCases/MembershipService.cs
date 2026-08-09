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
    IMembershipRepository membershipRepository,
    IInternalProfileService internalProfileService,
    MembershipNotifier notifier,
    MembershipRequirementsService requirementsService) : IMembershipService
{
    public MemberProfileDto GetMine(Guid accountId, Guid communityId)
    {
        var (_, membership) = requirementsService.RequireMember(accountId, communityId);
        return membership.ToProfileDto(internalProfileService.GetProfile(accountId), DateTime.UtcNow);
    }

    public UnitNumberDto SetUnitNumber(Guid accountId, Guid communityId, SetUnitNumberRequestDto requestDto)
    {
        var (_, membership) = requirementsService.RequireMember(accountId, communityId);

        membership.SetUnitNumber(requestDto.UnitNumber);
        membershipRepository.Update(membership);

        return membership.ToUnitNumberDto();
    }

    public MemberProfileDto Get(Guid accountId, Guid communityId, Guid membershipId)
    {
        requirementsService.RequireConfirmed(accountId, communityId);

        var target = membershipRepository.GetById(membershipId);
        if (target is null || target.CommunityId != communityId)
            throw new NotFoundException("Membership not found in this community.");

        return target.ToProfileDto(internalProfileService.GetProfile(target.AccountId), DateTime.UtcNow);
    }

    public IReadOnlyList<MemberSummaryDto> GetConfirmed(Guid accountId, Guid communityId)
    {
        requirementsService.RequireConfirmed(accountId, communityId);
        return Cards(communityId, m => m.IsActive() && m.IsConfirmed());
    }

    public IReadOnlyList<MemberSummaryDto> GetIssuers(Guid accountId, Guid communityId)
    {
        requirementsService.RequireMember(accountId, communityId);
        return Cards(communityId, m => m.CanIssueCertifications());
    }

    public IReadOnlyList<MemberSummaryDto> GetUnconfirmed(Guid accountId, Guid communityId)
    {
        requirementsService.RequireAnyRole(accountId, communityId, CommunityRole.Issuer, CommunityRole.Manager);
        return Cards(communityId, m => m.IsActive() && !m.IsConfirmed());
    }

    public MemberSummaryDto? GetManager(Guid accountId, Guid communityId)
    {
        requirementsService.RequireConfirmed(accountId, communityId);
        var cards = Cards(communityId, m => m.IsActive() && m.HasRole(CommunityRole.Manager));
        return cards.SingleOrDefault();
    }

    public IReadOnlyList<MemberSummaryDto> GetRanking(Guid accountId, Guid communityId)
    {
        requirementsService.RequireConfirmed(accountId, communityId);

        var roster = (membershipRepository.GetByCommunity(communityId))
            .Where(m => m.IsActive() && m.IsConfirmed() && m.Stars > 0)
            .OrderByDescending(m => m.Stars)
            .ToList();

        return roster.ToSummaryDtos(Profiles(roster));
    }

    public void GrantIssuer(Guid accountId, Guid communityId, Guid membershipId)
    {
        var (_, actor) = requirementsService.RequireAnyRole(accountId, communityId, CommunityRole.Issuer, CommunityRole.Manager);

        var target = membershipRepository.GetById(membershipId);
        if (target is null || target.CommunityId != communityId)
            throw new NotFoundException("Membership not found in this community.");

        target.Grant(CommunityRole.Issuer, actor.Id, DateTime.UtcNow);
        membershipRepository.Update(target);

        notifier.RolesChanged(target);
    }

    private IReadOnlyList<MemberSummaryDto> Cards(
        Guid communityId, Func<Membership, bool> predicate)
    {
        var roster = (membershipRepository.GetByCommunity(communityId)).Where(predicate).ToList();
        return roster.ToSummaryDtos(Profiles(roster));
    }

    private IReadOnlyDictionary<Guid, InternalProfileDto> Profiles(
        IReadOnlyCollection<Membership> roster)
    {
        if (roster.Count == 0)
            return new Dictionary<Guid, InternalProfileDto>();

        var profiles = internalProfileService.GetProfiles(roster.Select(m => m.AccountId).Distinct().ToList());
        return profiles.ToDictionary(p => p.AccountId);
    }
}
