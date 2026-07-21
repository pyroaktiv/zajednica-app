using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;
using DomainCertificationService = Zajednica.Community.Core.Domain.CertificationService;

namespace Zajednica.Community.Core.UseCases;

public sealed class CertificationService(
    ICertificationChallengeRepository challenges,
    ICertificateRepository certificates,
    IMembershipRepository memberships,
    IInternalAccountService accounts,
    ISecureTokenGenerator tokens,
    INotificationSender notifications,
    IRealtimePusher realtime,
    DomainCertificationService certification,
    MembershipAccess access) : ICertificationService
{
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(2);

    public async Task<CertificationChallengeDto> CreateChallengeAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        var (_, issuer) = await access.RequireRoleAsync(accountId, communityId, CommunityRole.Issuer, ct);

        var challenge = new CertificationChallenge(
            communityId, issuer.Id, tokens.Generate(), DateTime.UtcNow.Add(ChallengeLifetime));
        await challenges.AddAsync(challenge, ct);

        return challenge.ToDto();
    }

    public async Task CancelChallengeAsync(Guid accountId, Guid communityId, Guid challengeId, CancellationToken ct = default)
    {
        var (_, issuer) = await access.RequireRoleAsync(accountId, communityId, CommunityRole.Issuer, ct);

        var challenge = await challenges.GetByIdAsync(challengeId, ct);
        if (challenge is null || challenge.CommunityId != communityId)
            throw new NotFoundException("Challenge not found in this community.");
        if (challenge.IssuerMembershipId != issuer.Id)
            throw new ForbiddenException("Only the issuer who created the challenge can cancel it.");

        await challenges.RemoveAsync(challenge, ct);
    }

    public async Task<MembershipDto> ConfirmAsync(Guid accountId, ConfirmCertificationRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var challenge = await challenges.GetByTokenAsync(request.Token, ct)
            ?? throw new NotFoundException("Certification challenge not found.");

        if (!challenge.IsValid(now))
        {
            await challenges.RemoveAsync(challenge, ct);
            throw new EntityValidationException("Certification challenge has expired.");
        }

        var candidate = await memberships.GetAsync(accountId, challenge.CommunityId, ct)
            ?? throw new ForbiddenException("Not a member of this community.");
        var issuer = await memberships.GetByIdAsync(challenge.IssuerMembershipId, ct)
            ?? throw new NotFoundException("Issuer membership not found.");

        var certificate = certification.Certify(issuer, candidate, now);

        await memberships.UpdateAsync(candidate, ct);
        await certificates.AddAsync(certificate, ct);
        await challenges.RemoveAsync(challenge, ct);

        await realtime.PushToUserAsync(issuer.AccountId,
            new RealtimeMessage("certification.confirmed", new { challengeId = challenge.Id, membershipId = candidate.Id }), ct);
        await realtime.PushToUserAsync(accountId,
            new RealtimeMessage("membership.roles.changed", new { communityId = challenge.CommunityId }), ct);
        await notifications.SendAsync(new NotificationRequest(
            accountId, "Potvrda članstva", "Vaše članstvo u zajednici je potvrđeno.", NotificationPriority.Default), ct);

        return candidate.ToDto(await accounts.GetProfileAsync(accountId, ct));
    }

    public async Task<TrustGraphDto> GetTrustGraphAsync(Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);

        var confirmed = (await memberships.GetByCommunityAsync(communityId, ct))
            .Where(m => m.IsConfirmed())
            .ToList();
        var edges = await certificates.GetByCommunityAsync(communityId, ct);

        var profiles = confirmed.Count == 0
            ? new Dictionary<Guid, AccountProfileDto>()
            : (await accounts.GetProfilesAsync(confirmed.Select(m => m.AccountId).Distinct().ToList(), ct))
                .ToDictionary(p => p.AccountId);

        return CertificationMappers.ToTrustGraph(confirmed, edges, profiles);
    }
}
