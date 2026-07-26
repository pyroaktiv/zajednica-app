using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Mappers;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases;

public sealed class IntentPresenter(AuthorDirectory authors)
{
    public IntentDetailsDto Details(Intent intent, Guid readerMembershipId)
    {
        var target = intent.Action as UserTargetingAction;

        var ids = new List<Guid> { intent.AuthorMembershipId };
        if (target is not null)
            ids.Add(target.TargetMembershipId);

        var profiles = authors.For(ids);

        return intent.ToDetailsDto(
            profiles.GetValueOrDefault(intent.AuthorMembershipId),
            target?.TargetMembershipId,
            target is null ? null : profiles.GetValueOrDefault(target.TargetMembershipId),
            readerMembershipId);
    }

    public IntentDetailsDto Details(IntentView view, bool? myVote)
    {
        var ids = new List<Guid> { view.AuthorMembershipId };
        if (view.TargetMembershipId is { } target)
            ids.Add(target);

        var profiles = authors.For(ids);

        return view.ToDetailsDto(
            profiles.GetValueOrDefault(view.AuthorMembershipId),
            view.TargetMembershipId is { } id ? profiles.GetValueOrDefault(id) : null,
            myVote);
    }

    public IReadOnlyList<IntentVoterDto> Voters(IReadOnlyList<IntentVoteView> votes)
    {
        var profiles = authors.For(votes.Select(v => v.VoterMembershipId).ToList());

        return votes
            .Select(v => v.ToVoterDto(profiles.GetValueOrDefault(v.VoterMembershipId)))
            .ToList();
    }

    public CursorPage<IntentSummaryDto> Summaries(CursorPage<IntentView> page)
    {
        var profiles = authors.For(page.Items
            .Where(v => v.TargetMembershipId is not null)
            .Select(v => v.TargetMembershipId!.Value)
            .ToList());

        return new CursorPage<IntentSummaryDto>(
            page.Items
                .Select(v => v.ToSummaryDto(
                    v.TargetMembershipId is { } target ? profiles.GetValueOrDefault(target) : null))
                .ToList(),
            page.NextCursor);
    }
}
