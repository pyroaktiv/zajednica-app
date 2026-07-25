using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.BuildingBlocks.Infrastructure.Database;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class DocumentEfRepository(CommunityDbContext db) : IDocumentRepository
{
    public void Add(Document document)
    {
        db.Documents.Add(document);
        db.SaveChanges();
    }

    public void Remove(Document document)
    {
        db.Documents.Remove(document);
        db.SaveChanges();
    }

    public Document? GetById(Guid id) =>
        db.Documents.FirstOrDefault(d => d.Id == id);

    public PagedResult<Document> GetPaged(Guid communityId, int page, int pageSize) =>
        db.Documents
            .Where(d => d.CommunityId == communityId)
            .OrderByDescending(d => d.Date)
            .GetPaged(page, pageSize)
            .Result;
}
