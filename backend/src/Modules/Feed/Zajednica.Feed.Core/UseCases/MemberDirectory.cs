using Zajednica.Community.Api.Internal;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.UseCases;

public sealed class MemberDirectory(
    IInternalMembershipDirectoryService internalDirectoryService,
    IInternalProfileService internalProfileService)
{
    public Guid? AccountId(Guid membershipId) =>
        internalDirectoryService.GetAccountIdsByMembershipIds([membershipId]).SingleOrDefault()?.AccountId;

    public IReadOnlyDictionary<Guid, InternalProfileDto> Profiles(IReadOnlyCollection<Guid> membershipIds)
    {
        var empty = new Dictionary<Guid, InternalProfileDto>();
        if (membershipIds.Count == 0)
            return empty;

        var members = internalDirectoryService.GetAccountIdsByMembershipIds(membershipIds.Distinct().ToList());
        if (members.Count == 0)
            return empty;

        var profiles = internalProfileService.GetProfiles(members.Select(m => m.AccountId).Distinct().ToList())
            .ToDictionary(p => p.AccountId);

        return members
            .Where(m => profiles.ContainsKey(m.AccountId))
            .ToDictionary(m => m.MembershipId, m => profiles[m.AccountId]);
    }
}
