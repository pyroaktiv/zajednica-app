using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.BuildingBlocks.Infrastructure.Database;

public class UnitOfWork<TDbContext>(TDbContext db) : IUnitOfWork where TDbContext : DbContext
{
    private IDbContextTransaction? _transaction;

    public void BeginTransaction() => _transaction ??= db.Database.BeginTransaction();

    public void Save()
    {
        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new ConcurrencyConflictException("A concurrent change conflicted with this operation.", e);
        }
    }

    public void Commit()
    {
        _transaction?.Commit();
        Discard();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        db.ChangeTracker.Clear();
        Discard();
    }

    private void Discard()
    {
        _transaction?.Dispose();
        _transaction = null;
    }
}
