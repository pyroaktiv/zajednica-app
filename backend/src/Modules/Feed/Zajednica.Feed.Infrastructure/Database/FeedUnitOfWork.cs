using Zajednica.BuildingBlocks.Infrastructure.Database;
using Zajednica.Feed.Core.UseCases;

namespace Zajednica.Feed.Infrastructure.Database;

internal sealed class FeedUnitOfWork(FeedDbContext db) : UnitOfWork<FeedDbContext>(db), IFeedUnitOfWork { }
