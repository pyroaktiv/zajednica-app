using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IIntentRepository
{
    void Add(Intent intent);
    void Update(Intent intent);

    Intent? Get(Guid id);
}
