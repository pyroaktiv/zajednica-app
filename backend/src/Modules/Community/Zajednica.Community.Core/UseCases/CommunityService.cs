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
    ICommunityRepository communityRepository,
    IMembershipRepository membershipRepository,
    ISecureTokenGenerator tokenGenerator,
    IRealtimePusher realtimePusher,
    MembershipRequirementsService requirementsService) : ICommunityService
{
    public CommunityDetailsDto Create(Guid accountId, CreateCommunityRequestDto requestDto)
    {
        var now = DateTime.UtcNow;
        var community = new CommunityAggregate(
            requestDto.Name,
            requestDto.Address.ToAddress(),
            tokenGenerator.Generate(),
            now,
            CommunityMappers.ToRegistrationNumber(requestDto.RegistrationNumber),
            CommunityMappers.ToTaxId(requestDto.TaxId),
            requestDto.BankAccountNumber);

        communityRepository.Add(community);
        membershipRepository.Add(Membership.MakeCreator(accountId, community.Id, now));

        return community.ToDetailsDto();
    }

    public IReadOnlyList<MyCommunityDto> GetMine(Guid accountId)
    {
        var mine = (membershipRepository.GetByAccount(accountId))
            .Where(m => m.IsActive())
            .ToList();
        if (mine.Count == 0)
            return [];

        var found = (communityRepository.GetManyByIds(mine.Select(m => m.CommunityId).ToList()))
            .ToDictionary(c => c.Id);

        return mine
            .Where(m => found.ContainsKey(m.CommunityId))
            .Select(m => found[m.CommunityId].ToMyCommunityDto(m))
            .ToList();
    }

    public CommunityDetailsDto Get(Guid accountId, Guid communityId)
    {
        var (community, _) = requirementsService.RequireMember(accountId, communityId);
        return community.ToDetailsDto();
    }

    public CommunityDetailsDto Update(Guid accountId, Guid communityId, UpdateCommunityRequestDto requestDto)
    {
        var (community, _) = requirementsService.RequireRole(accountId, communityId, CommunityRole.Manager);

        community.UpdateDetails(
            requestDto.Name,
            requestDto.Address.ToAddress(),
            CommunityMappers.ToRegistrationNumber(requestDto.RegistrationNumber),
            CommunityMappers.ToTaxId(requestDto.TaxId),
            requestDto.BankAccountNumber);

        communityRepository.Update(community);

        return community.ToDetailsDto();
    }

    public CommunityQrDto GetQr(Guid accountId, Guid communityId)
    {
        var (community, _) = requirementsService.RequireConfirmed(accountId, communityId);
        return community.ToQrDto();
    }

    public JoinedCommunityDto Join(Guid accountId, JoinCommunityRequestDto requestDto)
    {
        var community = communityRepository.GetByQrToken(requestDto.QrToken)
            ?? throw new NotFoundException("No community matches this QR code.");

        var existing = membershipRepository.Get(accountId, community.Id);
        if (existing is null)
        {
            var membership = new Membership(accountId, community.Id, DateTime.UtcNow);
            membershipRepository.Add(membership);
            return membership.ToJoinedDto(community.Name);
        }

        existing.Rejoin();
        membershipRepository.Update(existing);

        return existing.ToJoinedDto(community.Name);
    }

    public void Leave(Guid accountId, Guid communityId)
    {
        var (_, membership) = requirementsService.RequireMember(accountId, communityId);

        membership.Leave(DateTime.UtcNow);
        membershipRepository.Update(membership);

        realtimePusher.PushToUser(accountId,
            new RealtimeMessage("membership.roles.changed", new { communityId }));
    }
}
