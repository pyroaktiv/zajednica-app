using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IIntentRepository
{
    void Add(Intent intent);
    void Update(Intent intent);

    Intent? LoadFromSource(Guid id);
    IReadOnlyList<Guid> GetDueIds(DateTime now);
    IReadOnlyList<Intent> GetOpenByTarget(Guid communityId, Guid targetMembershipId);
}
