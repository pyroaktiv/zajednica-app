using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Feed.Core.Domain;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.UseCases;

public sealed class MemberRequirementsService(IInternalMembershipFactsService internalMembershipFactsService)
{
    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        Confirmed(accountId, communityId).MembershipId;

    public VoterContext RequireVoter(Guid accountId, Guid communityId)
    {
        var voter = Confirmed(accountId, communityId);
        return new VoterContext(voter.MembershipId, voter.CertifiedAt);
    }

    public Guid RequireUnmutedConfirmed(Guid accountId, Guid communityId)
    {
        var facts = Confirmed(accountId, communityId);
        EnsureUnmuted(facts);
        return facts.MembershipId;
    }

    public MemberStandingContext StandingOf(Guid communityId, Guid membershipId)
    {
        var facts = internalMembershipFactsService.FindByMembershipInCommunity(communityId, membershipId);

        var status = facts switch
        {
            { IsBanned: true } => MembershipStatus.Banned,
            { IsActive: true, IsConfirmed: true } => MembershipStatus.Confirmed,
            _ => MembershipStatus.Unconfirmed
        };

        var role = facts is { IsManager: true } ? MembershipRole.Manager : MembershipRole.None;

        return new MemberStandingContext(membershipId, status, role);
    }

    private InternalMembershipFactsDto Active(Guid accountId, Guid communityId)
    {
        var facts = internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId);
        if (facts is null)
            throw new ForbiddenException("Not a member of this community.");
        if (!facts.IsActive)
            throw new ForbiddenException("Membership is not active.");

        return facts;
    }

    private InternalMembershipFactsDto Confirmed(Guid accountId, Guid communityId)
    {
        var facts = Active(accountId, communityId);
        if (!facts.IsConfirmed)
            throw new ForbiddenException("Only a confirmed member can do this.");

        return facts;
    }

    private static void EnsureUnmuted(InternalMembershipFactsDto facts)
    {
        if (facts.MutedUntil is { } until && until > DateTime.UtcNow)
            throw new ForbiddenException("You are muted in this community.");
    }
}
