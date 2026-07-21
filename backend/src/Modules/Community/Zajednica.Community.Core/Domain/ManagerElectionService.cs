using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

// At most one manager per community: an invariant that spans two memberships, so neither of
// them can own it. The current manager is null when the community has none.
public class ManagerElectionService
{
    public void Elect(Membership? currentManager, Membership newManager, DateTime now)
    {
        if (currentManager is not null)
        {
            if (currentManager.CommunityId != newManager.CommunityId)
                throw new EntityValidationException("Memberships belong to different communities.");
            if (currentManager.Id == newManager.Id)
                throw new EntityValidationException("Membership is already the manager.");

            newManager.Grant(CommunityRole.Manager, null, now);
            currentManager.Revoke(CommunityRole.Manager);
            return;
        }

        newManager.Grant(CommunityRole.Manager, null, now);
    }
}
