using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.Mappers;

public static class ChatMappers
{
    public static ChatDetailsDto ToDetailsDto(this ChatAggregate chat, Guid viewerMembershipId,
        IReadOnlyDictionary<Guid, InternalProfileDto> profiles)
    {
        var help = chat as HelpRequestChat;

        return new ChatDetailsDto(
            Id: chat.Id,
            Type: TypeOf(chat),
            Participants: chat.Participants
                .Select(p => new ChatParticipantDto(
                    MembershipId: p.MembershipId,
                    Username: UsernameOf(profiles, p.MembershipId),
                    Role: p.Role?.ToString()))
                .ToList(),
            CanSend: chat.CanSend(viewerMembershipId),
            HelpRequestId: help?.HelpRequestId,
            Status: help?.Status.ToString(),
            AwardedStars: help?.AwardedStars);
    }

    public static ChatSummaryDto ToSummaryDto(this ChatAggregate chat, Guid viewerMembershipId,
        IReadOnlyDictionary<Guid, InternalProfileDto> profiles)
    {
        var help = chat as HelpRequestChat;

        return new ChatSummaryDto(
            Id: chat.Id,
            ParticipantUsernames: chat.Participants
                .Where(p => p.MembershipId != viewerMembershipId)
                .Select(p => UsernameOf(profiles, p.MembershipId))
                .ToList(),
            HelpRequestId: help?.HelpRequestId,
            Status: help?.Status.ToString(),
            LastActivityAt: chat.LastActivityAt,
            HasUnread: chat.HasUnread(viewerMembershipId));
    }

    public static CursorPage<ChatSummaryDto, PageCursor> ToSummaryPage<TChat>(this CursorPage<TChat, PageCursor> page,
        Guid viewerMembershipId, IReadOnlyDictionary<Guid, InternalProfileDto> profiles)
        where TChat : ChatAggregate =>
        new(page.Items.Select(c => c.ToSummaryDto(viewerMembershipId, profiles)).ToList(),
            page.NextCursor);

    private static string UsernameOf(IReadOnlyDictionary<Guid, InternalProfileDto> profiles, Guid membershipId) =>
        profiles.GetValueOrDefault(membershipId)?.Username ?? string.Empty;

    private static string TypeOf(ChatAggregate chat) => chat switch
    {
        DirectChat => "DIRECT",
        HelpRequestChat => "HELP_REQUEST",
        TemporaryChat => "TEMPORARY",
        _ => throw new EntityValidationException("Unknown chat type.")
    };
}
