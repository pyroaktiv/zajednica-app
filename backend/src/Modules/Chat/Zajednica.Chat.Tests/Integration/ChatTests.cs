using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Community.Api.Internal;

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
        chat.Participants.Select(p => p.MembershipId).ShouldBe([owner.MembershipId, neighbour.MembershipId]);
        ParticipantOf(chat, neighbour.MembershipId).Username.ShouldNotBeNullOrEmpty();
        chat.CanSend.ShouldBeTrue();
        OpenDirect(scope, neighbour.AccountId, community.Id, owner.MembershipId).Id.ShouldBe(chat.Id);

        SendText(scope, owner.AccountId, community.Id, chat.Id, "Prva");
        SendText(scope, neighbour.AccountId, community.Id, chat.Id, "Druga");
        SendText(scope, owner.AccountId, community.Id, chat.Id, "Treca");

        var first = Page(scope, owner.AccountId, community.Id, chat.Id, null, 2);
        first.Items.Select(m => m.Text).ShouldBe(["Treca", "Druga"]);
        first.Items[0].Type.ShouldBe("TEXT");
        first.Items[0].SenderUsername.ShouldNotBeNullOrEmpty();
        first.NextCursor.ShouldNotBeNull();

        var rest = Page(scope, owner.AccountId, community.Id, chat.Id, first.NextCursor, 2);
        rest.Items.Select(m => m.Text).ShouldBe(["Prva"]);
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

        var mine = Direct(scope, owner.AccountId, community.Id);
        mine.Items.Select(c => c.Id).ShouldBe([newer.Id, older.Id]);
        mine.Items.Select(c => c.HasUnread).ShouldAllBe(unread => unread == false);

        var theirs = Direct(scope, first.AccountId, community.Id);
        theirs.Items.Single().HasUnread.ShouldBeTrue();

        Messages(scope, first.AccountId).MarkRead(community.Id, older.Id);

        Direct(scope, first.AccountId, community.Id).Items.Single().HasUnread.ShouldBeFalse();
    }

    [Fact]
    public void Each_kind_of_chat_is_listed_on_its_own_endpoint()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var neighbour = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var newcomer = AddUnconfirmedMember(scope, owner.AccountId, community.Id);

        var direct = OpenDirect(scope, owner.AccountId, community.Id, neighbour.MembershipId);
        var post = CreateHelpRequest(scope, owner.AccountId, community.Id, "Treba mi pomoc oko selidbe.");
        var help = Respond(scope, neighbour.AccountId, community.Id, post.Id);
        var temporary = OpenTemporary(scope, newcomer.AccountId, community.Id, owner.MembershipId);

        Direct(scope, owner.AccountId, community.Id).Items.Single().Id.ShouldBe(direct.Id);
        Temporary(scope, owner.AccountId, community.Id).Items.Single().Id.ShouldBe(temporary.Id);

        var listedHelp = HelpRequests(scope, owner.AccountId, community.Id).Items.Single();
        listedHelp.Id.ShouldBe(help.Id);
        listedHelp.HelpRequestId.ShouldBe(post.Id);
        listedHelp.ParticipantUsernames.ShouldHaveSingleItem().ShouldNotBeNullOrEmpty();

        Temporary(scope, newcomer.AccountId, community.Id).Items.Single().Id.ShouldBe(temporary.Id);
        Direct(scope, newcomer.AccountId, community.Id).Items.ShouldBeEmpty();
    }

    [Fact]
    public void A_concluded_help_request_chat_stays_in_the_list()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, requester) = CreateCommunity(scope);
        var helper = AddConfirmedMember(scope, requester.AccountId, community.Id);
        var post = CreateHelpRequest(scope, requester.AccountId, community.Id, "Treba mi pomoc.");

        var chat = Respond(scope, helper.AccountId, community.Id, post.Id);
        HelpChats(scope, requester.AccountId).ConcludeWithoutReward(community.Id, chat.Id);

        var listed = HelpRequests(scope, helper.AccountId, community.Id).Items.Single();
        listed.Id.ShouldBe(chat.Id);
        listed.Status.ShouldBe("Concluded");
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
        ParticipantOf(chat, newcomer.MembershipId).Role.ShouldBe("Uncertified");
        SendText(scope, newcomer.AccountId, community.Id, chat.Id, "Kada mozemo da se nadjemo?");

        Confirm(scope, owner.AccountId, newcomer.AccountId, community.Id);

        Db(scope).Chats.Any(c => c.Id == chat.Id).ShouldBeFalse();
    }

    [Fact]
    public void A_muted_member_reads_their_chats_but_cannot_write_anywhere()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var muted = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var neighbour = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var post = CreateHelpRequest(scope, owner.AccountId, community.Id, "Treba mi pomoc.");

        var chat = OpenDirect(scope, muted.AccountId, community.Id, owner.MembershipId);
        SendText(scope, muted.AccountId, community.Id, chat.Id, "Pre utisavanja");

        scope.ServiceProvider.GetRequiredService<IInternalMembershipCommandService>().Mute(muted.MembershipId);
        CommunityDb(scope).ChangeTracker.Clear();

        Should.Throw<ForbiddenException>(() => SendText(scope, muted.AccountId, community.Id, chat.Id, "Posle"));
        Should.Throw<ForbiddenException>(() =>
            OpenDirect(scope, muted.AccountId, community.Id, neighbour.MembershipId));
        Should.Throw<ForbiddenException>(() => Respond(scope, muted.AccountId, community.Id, post.Id));

        Page(scope, muted.AccountId, community.Id, chat.Id, null, 10)
            .Items.Select(m => m.Text).ShouldBe(["Pre utisavanja"]);
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

    private static CursorPage<MessageDto, PageCursor> Page(IServiceScope scope, Guid accountId, Guid communityId, Guid chatId,
        PageCursor? before, int limit) =>
        Value<CursorPage<MessageDto, PageCursor>>(Messages(scope, accountId)
            .GetPage(communityId, chatId, before, limit).Result!);

    private static CursorPage<ChatSummaryDto, PageCursor> Direct(IServiceScope scope, Guid accountId, Guid communityId) =>
        Value<CursorPage<ChatSummaryDto, PageCursor>>(Chats(scope, accountId).GetDirectPage(communityId, null, 10).Result!);

    private static CursorPage<ChatSummaryDto, PageCursor> HelpRequests(IServiceScope scope, Guid accountId, Guid communityId) =>
        Value<CursorPage<ChatSummaryDto, PageCursor>>(Chats(scope, accountId).GetHelpRequestPage(communityId, null, 10).Result!);

    private static CursorPage<ChatSummaryDto, PageCursor> Temporary(IServiceScope scope, Guid accountId, Guid communityId) =>
        Value<CursorPage<ChatSummaryDto, PageCursor>>(Chats(scope, accountId).GetTemporaryPage(communityId, null, 10).Result!);
}
