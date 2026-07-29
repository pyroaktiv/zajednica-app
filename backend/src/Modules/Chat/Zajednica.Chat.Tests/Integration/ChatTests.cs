using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.Messages;

namespace Zajednica.Chat.Tests.Integration;

[Collection("Sequential")]
public class ChatTests : BaseChatIntegrationTest
{
    public ChatTests(ChatTestFactory factory) : base(factory) { }

    [Fact]
    public void A_direct_chat_is_opened_once_and_read_one_cursor_page_of_messages_at_a_time()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var neighbour = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var chat = OpenDirect(scope, owner.AccountId, community.Id, neighbour.MembershipId);

        chat.Type.ShouldBe("DIRECT");
        chat.CounterpartMembershipId.ShouldBe(neighbour.MembershipId);
        chat.Title.ShouldNotBeNullOrEmpty();
        chat.CanSend.ShouldBeTrue();
        OpenDirect(scope, neighbour.AccountId, community.Id, owner.MembershipId).Id.ShouldBe(chat.Id);

        SendText(scope, owner.AccountId, community.Id, chat.Id, "Prva");
        SendText(scope, neighbour.AccountId, community.Id, chat.Id, "Druga");
        SendText(scope, owner.AccountId, community.Id, chat.Id, "Treca");

        var first = Page(scope, owner.AccountId, community.Id, chat.Id, null, 2);
        first.Items.Select(m => m.Text).ShouldBe(["Prva", "Druga"]);
        first.Items[0].Type.ShouldBe("TEXT");
        first.Items[0].SenderUsername.ShouldNotBeNullOrEmpty();
        first.NextCursor.ShouldNotBeNull();

        var rest = Page(scope, owner.AccountId, community.Id, chat.Id, first.NextCursor, 2);
        rest.Items.Select(m => m.Text).ShouldBe(["Treca"]);
        rest.NextCursor.ShouldBeNull();
    }

    [Fact]
    public void The_chat_list_is_ordered_by_last_activity_and_unread_until_the_watermark_moves()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var first = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var older = OpenDirect(scope, owner.AccountId, community.Id, first.MembershipId);
        var newer = OpenDirect(scope, owner.AccountId, community.Id, second.MembershipId);
        SendText(scope, owner.AccountId, community.Id, older.Id, "Prva");
        SendText(scope, owner.AccountId, community.Id, newer.Id, "Druga");

        var mine = List(scope, owner.AccountId, community.Id);
        mine.Items.Select(c => c.Id).ShouldBe([newer.Id, older.Id]);
        mine.Items.Select(c => c.HasUnread).ShouldAllBe(unread => unread == false);

        var theirs = List(scope, first.AccountId, community.Id);
        theirs.Items.Single().HasUnread.ShouldBeTrue();

        Messages(scope, first.AccountId).MarkRead(community.Id, older.Id);

        List(scope, first.AccountId, community.Id).Items.Single().HasUnread.ShouldBeFalse();
    }

    [Fact]
    public void A_newcomer_chats_only_with_an_issuer_and_confirmation_removes_that_chat()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var newcomer = AddUnconfirmedMember(scope, owner.AccountId, community.Id);

        Should.Throw<ForbiddenException>(() =>
            OpenDirect(scope, newcomer.AccountId, community.Id, owner.MembershipId));

        var chat = OpenTemporary(scope, newcomer.AccountId, community.Id, owner.MembershipId);

        chat.Type.ShouldBe("TEMPORARY");
        chat.MyRole.ShouldBe("Uncertified");
        SendText(scope, newcomer.AccountId, community.Id, chat.Id, "Kada mozemo da se nadjemo?");

        Confirm(scope, owner.AccountId, newcomer.AccountId, community.Id);

        Db(scope).Chats.Any(c => c.Id == chat.Id).ShouldBeFalse();
    }

    [Fact]
    public void A_member_who_left_the_community_can_no_longer_be_messaged()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var neighbour = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var chat = OpenDirect(scope, owner.AccountId, community.Id, neighbour.MembershipId);

        Leave(scope, neighbour.AccountId, community.Id);

        Should.Throw<EntityValidationException>(() =>
            SendText(scope, owner.AccountId, community.Id, chat.Id, "Jesi tu?"));
        Value<ChatDetailsDto>(Chats(scope, owner.AccountId).Get(community.Id, chat.Id).Result!)
            .CanSend.ShouldBeFalse();
    }

    [Fact]
    public void A_chat_of_others_is_not_found_for_an_outsider()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var neighbour = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var outsider = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var chat = OpenDirect(scope, owner.AccountId, community.Id, neighbour.MembershipId);

        Should.Throw<NotFoundException>(() => Chats(scope, outsider.AccountId).Get(community.Id, chat.Id));
    }

    private static CursorPage<MessageDto> Page(IServiceScope scope, Guid accountId, Guid communityId, Guid chatId,
        DateTime? after, int limit) =>
        Value<CursorPage<MessageDto>>(Messages(scope, accountId)
            .GetPage(communityId, chatId, after, limit).Result!);

    private static CursorPage<ChatSummaryDto> List(IServiceScope scope, Guid accountId, Guid communityId) =>
        Value<CursorPage<ChatSummaryDto>>(Chats(scope, accountId).GetPage(communityId, null, 10).Result!);
}
