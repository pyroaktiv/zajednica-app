using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Core.Mappers;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatPresenter(MemberDirectory directory)
{
    public ChatDetailsDto Details(ChatAggregate chat, Guid viewerMembershipId)
    {
        var profiles = directory.Profiles(chat.Participants.Select(p => p.MembershipId).ToList());

        return chat.ToDetailsDto(viewerMembershipId, profiles);
    }

    public CursorPage<ChatSummaryDto, DateTime> Summaries<TChat>(CursorPage<TChat, DateTime> page, Guid viewerMembershipId)
        where TChat : ChatAggregate
    {
        var otherMembershipIds = page.Items
            .SelectMany(c => c.Participants)
            .Select(p => p.MembershipId)
            .Where(id => id != viewerMembershipId)
            .ToList();

        return page.ToSummaryPage(viewerMembershipId, directory.Profiles(otherMembershipIds));
    }
}
