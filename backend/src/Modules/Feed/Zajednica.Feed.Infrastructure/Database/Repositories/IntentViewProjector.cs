using Microsoft.EntityFrameworkCore;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal static class IntentViewProjector
{
    public static async Task ProjectAsync(FeedDbContext db, Intent intent, CancellationToken ct)
    {
        var view = await db.IntentViews.FirstOrDefaultAsync(v => v.Id == intent.Id, ct);

        if (view is null)
            db.IntentViews.Add(new IntentView(intent));
        else
            view.Refresh(intent);
    }
}
