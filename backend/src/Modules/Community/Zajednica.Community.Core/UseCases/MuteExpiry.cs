using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases;

public sealed class MuteExpiry(IMembershipRepository memberships, MembershipNotifier notifier)
{
    public void EndExpired()
    {
        var now = DateTime.UtcNow;

        foreach (var membership in memberships.GetWithExpiredMute(now))
        {
            if (!membership.EndMuteIfExpired(now))
                continue;

            memberships.Update(membership);
            notifier.MuteChanged(membership);
        }
    }
}
