using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Feed.Core.Domain;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.UseCases;

public sealed class MemberRequirementsService(IInternalMembershipFactsService internalMembershipFactsService)
{
    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireConfirmed().MembershipId;

    public VoterContext RequireVoter(Guid accountId, Guid communityId)
    {
        var voter = internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireConfirmed();
        return new VoterContext(voter.MembershipId, voter.CertifiedAt);
    }

    public Guid RequireUnmutedConfirmed(Guid accountId, Guid communityId) =>
        internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireConfirmed().RequireUnmuted(DateTime.UtcNow).MembershipId;

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
}
