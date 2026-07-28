using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Core.Domain;

namespace Zajednica.Chat.Tests.Unit;

public class ChatTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid Mine = Guid.NewGuid();
    private static readonly Guid Theirs = Guid.NewGuid();

    private static DirectChat Direct() => new(Community, Mine, Theirs, Now);

    [Fact]
    public void A_direct_chat_has_two_participants_without_roles()
    {
        var chat = Direct();

        chat.Participants.Select(p => p.MembershipId).ShouldBe([Mine, Theirs]);
        chat.Participants.Select(p => p.Role).ShouldAllBe(role => role == null);
        chat.CounterpartOf(Mine).ShouldBe(Theirs);
    }

    [Fact]
    public void A_member_cannot_open_a_direct_chat_with_themselves()
    {
        Should.Throw<EntityValidationException>(() => new DirectChat(Community, Mine, Mine, Now));
    }

    [Fact]
    public void Only_a_participant_of_an_eligible_chat_sends_messages()
    {
        var chat = Direct();

        Should.Throw<ForbiddenException>(() => chat.SendText(Guid.NewGuid(), "Zdravo", true, Now));
        Should.Throw<EntityValidationException>(() => chat.SendText(Mine, "Zdravo", false, Now));

        var message = chat.SendText(Mine, "  Zdravo komsija  ", true, Now);

        message.Text.ShouldBe("Zdravo komsija");
        message.ChatId.ShouldBe(chat.Id);
        chat.Messages.ShouldHaveSingleItem();
        chat.LastActivityAt.ShouldBe(Now);
    }

    [Fact]
    public void A_voice_message_carries_its_recording_and_duration()
    {
        var chat = Direct();

        var message = chat.SendVoice(Mine, "https://cdn.local/1.m4a", 12, true, Now);

        message.AudioUrl.ShouldBe("https://cdn.local/1.m4a");
        message.DurationSeconds.ShouldBe(12);
        Should.Throw<EntityValidationException>(() => chat.SendVoice(Mine, "https://cdn.local/2.m4a", 0, true, Now));
    }

    [Fact]
    public void Read_status_is_a_watermark_that_only_moves_forward()
    {
        var chat = Direct();
        chat.SendText(Theirs, "Prva poruka", true, Now);

        chat.HasUnread(Mine).ShouldBeTrue();
        chat.HasUnread(Theirs).ShouldBeFalse();

        chat.MarkRead(Mine, Now.AddSeconds(1));
        chat.HasUnread(Mine).ShouldBeFalse();

        chat.MarkRead(Mine, Now.AddHours(-1));
        chat.ParticipantOf(Mine)!.LastReadAt.ShouldBe(Now.AddSeconds(1));

        chat.SendText(Theirs, "Druga poruka", true, Now.AddMinutes(1));
        chat.HasUnread(Mine).ShouldBeTrue();

        Should.Throw<ForbiddenException>(() => chat.MarkRead(Guid.NewGuid(), Now));
    }

    [Fact]
    public void A_temporary_chat_pairs_a_newcomer_with_an_issuer()
    {
        var chat = new TemporaryChat(Community, Mine, Theirs, Now);

        chat.ParticipantOf(Mine)!.Role.ShouldBe(ChatParticipantRole.Uncertified);
        chat.Participant(ChatParticipantRole.Issuer)!.MembershipId.ShouldBe(Theirs);
        chat.CanSend(Mine, true).ShouldBeTrue();
    }
}
