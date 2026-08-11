namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface ICommunityRepository
{
    void Add(Community community);
    void Update(Community community);

    Community? GetById(Guid id);
    Community? GetByQrToken(string qrToken);
    IReadOnlyList<Community> GetManyByIds(IReadOnlyCollection<Guid> ids);
}
