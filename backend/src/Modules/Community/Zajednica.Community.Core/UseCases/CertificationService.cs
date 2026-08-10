using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Chat.Api.Internal;
using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;
using DomainCertificationService = Zajednica.Community.Core.Domain.CertificationService;

namespace Zajednica.Community.Core.UseCases;

public sealed class CertificationService(
    ICertificationChallengeRepository challengeRepository,
    IMembershipRepository membershipRepository,
    ISecureTokenGenerator tokenGenerator,
    IInternalChatService internalChatService,
    IRealtimePusher realtimePusher,
    DomainCertificationService certificationService,
    MembershipRequirementsService requirementsService) : ICertificationService
{
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(2);

    public CertificationChallengeDto CreateChallenge(Guid accountId, Guid communityId)
    {
        var (_, issuer) = requirementsService.RequireAnyRole(accountId, communityId, CommunityRole.Issuer, CommunityRole.Manager);

        var challenge = new CertificationChallenge(
            communityId, issuer.Id, tokenGenerator.Generate(), DateTime.UtcNow.Add(ChallengeLifetime));
        challengeRepository.Add(challenge);

        return challenge.ToDto();
    }

    public void CancelChallenge(Guid accountId, Guid communityId, Guid challengeId)
    {
        var (_, issuer) = requirementsService.RequireAnyRole(accountId, communityId, CommunityRole.Issuer, CommunityRole.Manager);

        var challenge = challengeRepository.GetById(challengeId);
        if (challenge is null || challenge.CommunityId != communityId)
            throw new NotFoundException("Challenge not found in this community.");
        if (challenge.IssuerMembershipId != issuer.Id)
            throw new ForbiddenException("Only the issuer who created the challenge can cancel it.");

        challengeRepository.Remove(challenge);
    }

    public CertificationResultDto Confirm(Guid accountId, ConfirmCertificationRequestDto requestDto)
    {
        var now = DateTime.UtcNow;
        var challenge = challengeRepository.GetByToken(requestDto.Token)
            ?? throw new NotFoundException("Certification challenge not found.");

        if (!challenge.IsValid(now))
        {
            challengeRepository.Remove(challenge);
            throw new EntityValidationException("Certification challenge has expired.");
        }

        var candidate = membershipRepository.Get(accountId, challenge.CommunityId)
            ?? throw new ForbiddenException("Not a member of this community.");
        var issuer = membershipRepository.GetById(challenge.IssuerMembershipId)
            ?? throw new NotFoundException("Issuer membership not found.");

        certificationService.Certify(issuer, candidate, now);

        membershipRepository.Update(candidate);
        challengeRepository.Remove(challenge);
        internalChatService.DeleteTemporaryChats(challenge.CommunityId, candidate.Id);

        realtimePusher.PushToUser(issuer.AccountId,
            new RealtimeMessage("certification.confirmed", new { challengeId = challenge.Id, membershipId = candidate.Id }));
        realtimePusher.PushToUser(accountId,
            new RealtimeMessage("membership.roles.changed", new { communityId = challenge.CommunityId }));

        return candidate.ToCertificationResultDto();
    }
}
