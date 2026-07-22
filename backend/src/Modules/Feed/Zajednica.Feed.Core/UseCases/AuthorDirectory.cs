using Zajednica.Community.Api.Internal;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.UseCases;

public sealed class AuthorDirectory(IInternalMembershipService memberships, IInternalAccountService accounts)
{
    public async Task<IReadOnlyDictionary<Guid, AccountProfileDto>> ForAsync(
        IReadOnlyCollection<Guid> membershipIds, CancellationToken ct = default)
    {
        var empty = new Dictionary<Guid, AccountProfileDto>();
        if (membershipIds.Count == 0)
            return empty;

        var contexts = await memberships.GetContextsAsync(membershipIds.Distinct().ToList(), ct);
        if (contexts.Count == 0)
            return empty;

        var profiles = (await accounts.GetProfilesAsync(contexts.Select(c => c.AccountId).Distinct().ToList(), ct))
            .ToDictionary(p => p.AccountId);

        return contexts
            .Where(c => profiles.ContainsKey(c.AccountId))
            .ToDictionary(c => c.MembershipId, c => profiles[c.AccountId]);
    }
}
