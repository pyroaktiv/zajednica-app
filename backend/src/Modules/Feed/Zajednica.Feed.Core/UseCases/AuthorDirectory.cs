using Zajednica.Community.Api.Internal;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.UseCases;

public sealed class AuthorDirectory(IInternalMembershipService memberships, IInternalAccountService accounts)
{
    public IReadOnlyDictionary<Guid, AccountProfileDto> For(
        IReadOnlyCollection<Guid> membershipIds)
    {
        var empty = new Dictionary<Guid, AccountProfileDto>();
        if (membershipIds.Count == 0)
            return empty;

        var contexts = memberships.GetContexts(membershipIds.Distinct().ToList());
        if (contexts.Count == 0)
            return empty;

        var profiles = (accounts.GetProfiles(contexts.Select(c => c.AccountId).Distinct().ToList()))
            .ToDictionary(p => p.AccountId);

        return contexts
            .Where(c => profiles.ContainsKey(c.AccountId))
            .ToDictionary(c => c.MembershipId, c => profiles[c.AccountId]);
    }
}
