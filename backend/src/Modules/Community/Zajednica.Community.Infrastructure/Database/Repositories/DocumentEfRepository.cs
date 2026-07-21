using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.BuildingBlocks.Infrastructure.Database;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class DocumentEfRepository(CommunityDbContext db) : IDocumentRepository
{
    public Task AddAsync(Document document, CancellationToken ct = default)
    {
        db.Documents.Add(document);
        return db.SaveChangesAsync(ct);
    }

    public Task RemoveAsync(Document document, CancellationToken ct = default)
    {
        db.Documents.Remove(document);
        return db.SaveChangesAsync(ct);
    }

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<PagedResult<Document>> GetPagedAsync(Guid communityId, int page, int pageSize, CancellationToken ct = default) =>
        db.Documents
            .Where(d => d.CommunityId == communityId)
            .OrderByDescending(d => d.Date)
            .GetPaged(page, pageSize);
}
