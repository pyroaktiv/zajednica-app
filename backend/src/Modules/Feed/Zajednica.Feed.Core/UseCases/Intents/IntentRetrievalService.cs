using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentRetrievalService(IIntentRepository intentRepository, IIntentQueryStore intentQueryStore)
{
    public Intent RequireAggregate(Guid intentId, Guid communityId)
    {
        var intent = intentRepository.LoadFromSource(intentId);
        if (intent is null || intent.Initiative.CommunityId != communityId)
            throw new NotFoundException("Intent not found in this community.");

        return intent;
    }

    public IntentView RequireView(Guid intentId, Guid communityId)
    {
        var view = intentQueryStore.GetView(intentId);
        if (view is null || view.CommunityId != communityId)
            throw new NotFoundException("Intent not found in this community.");

        return view;
    }
}
