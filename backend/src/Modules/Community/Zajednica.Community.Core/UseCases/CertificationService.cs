using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;
using DomainCertificationService = Zajednica.Community.Core.Domain.CertificationService;

namespace Zajednica.Community.Core.UseCases;

public sealed class CertificationService(
    ICertificationChallengeRepository challenges,
    ICertificateRepository certificates,
    IMembershipRepository memberships,
    ISecureTokenGenerator tokens,
    INotificationSender notifications,
    IRealtimePusher realtime,
    DomainCertificationService certification,
    MembershipAccess access) : ICertificationService
{
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(2);

    public CertificationChallengeDto CreateChallenge(Guid accountId, Guid communityId)
    {
        var (_, issuer) = access.RequireRole(accountId, communityId, CommunityRole.Issuer);

        var challenge = new CertificationChallenge(
            communityId, issuer.Id, tokens.Generate(), DateTime.UtcNow.Add(ChallengeLifetime));
        challenges.Add(challenge);

        return challenge.ToDto();
    }

    public void CancelChallenge(Guid accountId, Guid communityId, Guid challengeId)
    {
        var (_, issuer) = access.RequireRole(accountId, communityId, CommunityRole.Issuer);

        var challenge = challenges.GetById(challengeId);
        if (challenge is null || challenge.CommunityId != communityId)
            throw new NotFoundException("Challenge not found in this community.");
        if (challenge.IssuerMembershipId != issuer.Id)
            throw new ForbiddenException("Only the issuer who created the challenge can cancel it.");

        challenges.Remove(challenge);
    }

    public CertificationResultDto Confirm(Guid accountId, ConfirmCertificationRequest request)
    {
        var now = DateTime.UtcNow;
        var challenge = challenges.GetByToken(request.Token)
            ?? throw new NotFoundException("Certification challenge not found.");

        if (!challenge.IsValid(now))
        {
            challenges.Remove(challenge);
            throw new EntityValidationException("Certification challenge has expired.");
        }

        var candidate = memberships.Get(accountId, challenge.CommunityId)
            ?? throw new ForbiddenException("Not a member of this community.");
        var issuer = memberships.GetById(challenge.IssuerMembershipId)
            ?? throw new NotFoundException("Issuer membership not found.");

        var certificate = certification.Certify(issuer, candidate, now);

        memberships.Update(candidate);
        certificates.Add(certificate);
        challenges.Remove(challenge);

        realtime.PushToUser(issuer.AccountId,
            new RealtimeMessage("certification.confirmed", new { challengeId = challenge.Id, membershipId = candidate.Id }));
        realtime.PushToUser(accountId,
            new RealtimeMessage("membership.roles.changed", new { communityId = challenge.CommunityId }));
        notifications.Send(new NotificationRequest(
            accountId, "Potvrda članstva", "Vaše članstvo u zajednici je potvrđeno.", NotificationPriority.Default));

        return candidate.ToCertificationResultDto(certificate.Date);
    }
}
