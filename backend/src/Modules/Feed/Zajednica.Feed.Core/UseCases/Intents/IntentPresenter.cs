using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Core.Mappers;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentPresenter(MemberDirectory directory)
{
    public IntentDetailsDto Details(IntentView view, bool? myVote)
    {
        var ids = new List<Guid> { view.AuthorMembershipId };
        if (view.TargetMembershipId is { } target)
            ids.Add(target);

        var profiles = directory.Profiles(ids);

        return view.ToDetailsDto(
            profiles.GetValueOrDefault(view.AuthorMembershipId),
            view.TargetMembershipId is { } id ? profiles.GetValueOrDefault(id) : null,
            myVote);
    }

    public IReadOnlyList<IntentVoterDto> Voters(IReadOnlyList<IntentVoteView> votes)
    {
        var profiles = directory.Profiles(votes.Select(v => v.VoterMembershipId).ToList());

        return votes
            .Select(v => v.ToVoterDto(profiles.GetValueOrDefault(v.VoterMembershipId)))
            .ToList();
    }

    public CursorPage<IntentSummaryDto, PageCursor> Summaries(CursorPage<IntentView, PageCursor> page)
    {
        var profiles = directory.Profiles(page.Items
            .Where(v => v.TargetMembershipId is not null)
            .Select(v => v.TargetMembershipId!.Value)
            .ToList());

        return new CursorPage<IntentSummaryDto, PageCursor>(
            page.Items
                .Select(v => v.ToSummaryDto(
                    v.TargetMembershipId is { } target ? profiles.GetValueOrDefault(target) : null))
                .ToList(),
            page.NextCursor);
    }
}
