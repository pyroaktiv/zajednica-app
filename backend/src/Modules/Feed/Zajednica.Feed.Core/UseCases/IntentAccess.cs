using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases;

public sealed class IntentAccess(IIntentRepository intents, IIntentQueryStore intentQueries)
{
    public Intent Require(Guid intentId, Guid communityId)
    {
        var intent = intents.Get(intentId);
        if (intent is null || intent.CommunityId != communityId)
            throw new NotFoundException("Intent not found in this community.");

        return intent;
    }

    public IntentView RequireView(Guid intentId, Guid communityId)
    {
        var view = intentQueries.GetView(intentId);
        if (view is null || view.CommunityId != communityId)
            throw new NotFoundException("Intent not found in this community.");

        return view;
    }
}
