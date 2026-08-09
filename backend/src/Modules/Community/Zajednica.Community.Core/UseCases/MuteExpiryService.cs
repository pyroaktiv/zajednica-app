using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases;

public sealed class MuteExpiryService(IMembershipRepository membershipRepository, MembershipNotifier notifier)
{
    public void EndExpired()
    {
        var now = DateTime.UtcNow;

        foreach (var membership in membershipRepository.GetWithExpiredMute(now))
        {
            if (!membership.EndMuteIfExpired(now))
                continue;

            membershipRepository.Update(membership);
            notifier.MuteChanged(membership);
        }
    }
}
