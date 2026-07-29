using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Chat.Core.UseCases;

public sealed class MemberDirectory(IInternalMembershipService memberships, IInternalAccountService accounts)
{
    public MembershipContextDto? Context(Guid membershipId) =>
        memberships.GetContexts([membershipId]).SingleOrDefault();

    public IReadOnlyList<MembershipContextDto> Contexts(IReadOnlyCollection<Guid> membershipIds) =>
        membershipIds.Count == 0 ? [] : memberships.GetContexts(membershipIds.Distinct().ToList());

    public IReadOnlyDictionary<Guid, AccountProfileDto> Profiles(IReadOnlyCollection<Guid> membershipIds)
    {
        var empty = new Dictionary<Guid, AccountProfileDto>();
        if (membershipIds.Count == 0)
            return empty;

        var contexts = memberships.GetContexts(membershipIds.Distinct().ToList());
        if (contexts.Count == 0)
            return empty;

        var profiles = accounts.GetProfiles(contexts.Select(c => c.AccountId).Distinct().ToList())
            .ToDictionary(p => p.AccountId);

        return contexts
            .Where(c => profiles.ContainsKey(c.AccountId))
            .ToDictionary(c => c.MembershipId, c => profiles[c.AccountId]);
    }
}
