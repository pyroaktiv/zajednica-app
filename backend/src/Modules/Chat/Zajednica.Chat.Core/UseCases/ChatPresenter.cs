using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Core.Mappers;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatPresenter(MemberDirectory directory, ChatAccess access)
{
    public ChatDetailsDto Details(ChatAggregate chat, Guid viewerMembershipId)
    {
        var counterpartMembershipId = chat.CounterpartOf(viewerMembershipId);
        var profiles = directory.Profiles([counterpartMembershipId]);

        return chat.ToDetailsDto(viewerMembershipId,
            profiles.GetValueOrDefault(counterpartMembershipId), access.ParticipantsEligible(chat));
    }

    public CursorPage<ChatSummaryDto> Summaries(CursorPage<ChatAggregate> page, Guid viewerMembershipId)
    {
        var counterpartIds = page.Items.Select(c => c.CounterpartOf(viewerMembershipId)).ToList();

        return page.ToSummaryPage(viewerMembershipId, directory.Profiles(counterpartIds));
    }
}
