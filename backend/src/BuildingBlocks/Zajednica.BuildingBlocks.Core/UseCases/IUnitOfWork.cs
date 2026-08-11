namespace Zajednica.BuildingBlocks.Core.UseCases;

public interface IUnitOfWork
{
    void BeginTransaction();
    void Save();
    void Commit();
    void Rollback();
}
