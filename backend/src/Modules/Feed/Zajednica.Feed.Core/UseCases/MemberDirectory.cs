using Zajednica.Community.Api.Internal;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.UseCases;

public sealed class MemberDirectory(
    IInternalMembershipDirectoryService memberships,
    IInternalAccountService accounts)
{
    public Guid? AccountId(Guid membershipId) =>
        memberships.GetAccounts([membershipId]).SingleOrDefault()?.AccountId;

    public IReadOnlyDictionary<Guid, AccountProfileDto> Profiles(IReadOnlyCollection<Guid> membershipIds)
    {
        var empty = new Dictionary<Guid, AccountProfileDto>();
        if (membershipIds.Count == 0)
            return empty;

        var members = memberships.GetAccounts(membershipIds.Distinct().ToList());
        if (members.Count == 0)
            return empty;

        var profiles = accounts.GetProfiles(members.Select(m => m.AccountId).Distinct().ToList())
            .ToDictionary(p => p.AccountId);

        return members
            .Where(m => profiles.ContainsKey(m.AccountId))
            .ToDictionary(m => m.MembershipId, m => profiles[m.AccountId]);
    }
}
